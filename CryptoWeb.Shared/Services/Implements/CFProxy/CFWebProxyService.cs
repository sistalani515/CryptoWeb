using CryptoWeb.Shared.Helper.CFProxy;
using CryptoWeb.Shared.Services.Interfaces.CFProxy;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Implements.CFProxy
{
    public class CFWebProxyService  : ICFWebProxyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _workerUrl;

        public CFWebProxyService(HttpClient httpClient, IOptions<CFProxyConfig> options)
        {
            this._httpClient = httpClient;
            this._workerUrl = options.Value.ProxyUrl!;
        }
        public async Task<HttpResponseMessage> SendAsync(
       string targetUrl,
       HttpMethod method,
       object body = null!,
       string bodyType = "",
       Dictionary<string, string> headers = null!,
       bool followRedirects = true)
        {
            // Wrap through Cloudflare Worker
            var proxyUrl = $"{_workerUrl}?url={Uri.EscapeDataString(targetUrl)}";

            var request = new HttpRequestMessage(method, proxyUrl);

            // Forward headers
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            // Body handling
            if (method != HttpMethod.Get && body != null)
            {
                if (bodyType == "form")
                {
                    request.Content = new StringContent(body.ToString()!, Encoding.UTF8, "application/x-www-form-urlencoded");
                }
                else if (bodyType == "multi")
                {
                    var formData = new MultipartFormDataContent
                {
                    { new StringContent(body.ToString()!), "payload" }
                };
                    request.Content = formData;
                }
                else if (body is string s)
                {
                    request.Content = new StringContent(s, Encoding.UTF8, "application/json");
                }
            }

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return response;
        }
    }
}
