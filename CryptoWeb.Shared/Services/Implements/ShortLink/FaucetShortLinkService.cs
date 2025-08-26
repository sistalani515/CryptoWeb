using CryptoWeb.Shared.Helper.HtmlHelper;
using CryptoWeb.Shared.Helper.HttpHelper;
using CryptoWeb.Shared.Models.Responses.FreeLTCFun;
using CryptoWeb.Shared.Services.Interfaces.ShortLink;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Implements.ShortLink
{
    public class FaucetShortLinkService : IFaucetShortLinkService
    {
        private readonly ILogger<FaucetShortLinkService> logger;

        public FaucetShortLinkService(ILogger<FaucetShortLinkService> logger)
        {
            this.logger = logger;
        }


        public async Task<FreeLTCFunSLResponse> SolveSL(string type, string path)
        {
            try
            {
                string url = "";
                switch(type)
                {
                    case "btshort":
                        url = $"https://link.btshort.in/{path}";
                        break;
                    case "faucetshort":
                        url = $"https://link.faucetshort.in/{path}";
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrWhiteSpace(url)) throw new Exception("type invalid");
                var response = await OpenURL(url, "");
                var result = await response.Content.ReadFromJsonAsync<FreeLTCFunSLResponse>();
                return result!;
            }
            catch (Exception)
            {

                throw;
            }
        }
		public async Task<HttpResponseMessage> OpenURL(string url, string setCookie)
		{
			try
			{
				var (response, cookie) = await GetResponse(url, HttpMethod.Get, setCookieX: setCookie);
                var doc = await HtmlHelper.ToHtml(response);
                var body = HtmlHelper.ExtractHiddenInputsAsQueryString(doc!);
                var newUri = new Uri(url);
                string urlAb = "https://" +  newUri.Host + "/links/go";
                await Task.Delay(10000);
                var (newResponse, newCookie) = await GetResponse(urlAb, HttpMethod.Post, body, cookie);
                return newResponse;
			}
			catch (Exception)
			{

				throw;
			}
		}

        private async Task<(HttpResponseMessage response, string cookie)> GetResponse(
    string urlpath,
    HttpMethod method,
    object body = null!,
    string setCookieX = "")
        {
            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(method, urlpath);
                request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not;A=Brand\";v=\"99\", \"Google Chrome\";v=\"139\", \"Chromium\";v=\"139\"");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
                request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
                if (method != HttpMethod.Get && body != null)
                {
                    var bodyString = body.ToString() ?? string.Empty;
                    request.Content = new StringContent(bodyString, Encoding.UTF8, "application/x-www-form-urlencoded");
                    request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                }

                // Parse existing cookies into a dictionary
                var cookieDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(setCookieX))
                {
                    foreach (var cookiePart in setCookieX.Split('&', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var kv = cookiePart.Split('=', 2);
                        if (kv.Length == 2)
                            cookieDict[kv[0].Trim()] = kv[1].Trim();
                    }

                    request.Headers.Add("Cookie", string.Join("; ", cookieDict.Select(kv => $"{kv.Key}={kv.Value}")));
                }

                var response = await client.SendAsync(request);

                // Process Set-Cookie headers from response
                var setCookieHeaders = response.Headers
                    .Where(h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(h => h.Value);

                foreach (var setCookie in setCookieHeaders)
                {
                    var nameValue = setCookie.Split(';', 2)[0].Trim();
                    var parts = nameValue.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value) && !HttpHelper.IsCookieAttribute(key))
                        {
                            // Update or add cookie
                            cookieDict[key] = value;
                        }
                    }
                }

                // Return updated cookies in & format
                var updatedCookieString = string.Join("&", cookieDict.Select(kv => $"{kv.Key}={kv.Value}"));

                return (response, updatedCookieString);
            }
            catch
            {
                throw;
            }
        }

    }
}
