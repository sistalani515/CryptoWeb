using CryptoWeb.Shared.Models.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TlsClient.Core;
using TlsClient.Core.Helpers.Builders;
using TlsClient.Core.Models;
using TlsClient.Core.Models.Entities;
using TlsClient.Core.Models.Requests;
using TlsClient.Core.Models.Responses;

namespace CryptoWeb.Shared.Helper.HttpHelper
{
    public static class TlsHttpHelper
    {
        private static readonly ConcurrentDictionary<string, TlsClient.Core.TlsClient> _clients = new();
        private static readonly ConcurrentDictionary<string, List<TlsClientCookie>> _cookies = new();

        private static void LoadCookiesFromSession(FaucetUser userFaucet, string baseUrl, List<TlsClientCookie> container)
        {
            if (string.IsNullOrWhiteSpace(userFaucet.Session))
                return;

            if (container.Count > 0)
                return; // Already loaded

            var cookies = ParseCookies(userFaucet.Session);
            foreach (var (cookieName, cookieValue) in cookies)
            {
                container.Add(new TlsClientCookie(cookieName, cookieValue));
            }
        }

        public static Dictionary<string, string> ParseCookies(string cookieHeader)
        {
            var cookies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(cookieHeader)) return cookies;
            var parts = cookieHeader.Split(';');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var index = trimmed.IndexOf('=');
                if (index > 0)
                {
                    var key = trimmed.Substring(0, index).Trim();
                    var value = trimmed.Substring(index + 1).Trim();
                    cookies[key] = value;
                }
            }
            return cookies;
        }

        public static async Task<TlsClient.Core.TlsClient> GetTlsClientAsync(FaucetUser faucetUser, string baseUrl, bool follow = true)
        {
            string baseKey = $"{faucetUser.Host!}-{faucetUser.Email!}";
            string clientKey = $"{baseKey}-redirect:{follow}";
            return _clients.GetOrAdd(clientKey, _ =>
            {
                var options = new TlsClientOptions(
                    clientIdentifier: TlsClientIdentifier.Chrome132,
                    userAgent: string.IsNullOrWhiteSpace(faucetUser.UA)
                        ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36"
                        : faucetUser.UA)
                {
                    FollowRedirects = follow,
                    Timeout = TimeSpan.FromSeconds(30)
                };

                var client = new TlsClient.Core.TlsClient(options);

                var cookieContainer = _cookies.GetOrAdd(baseKey, _ => new List<TlsClientCookie>());
                LoadCookiesFromSession(faucetUser, baseUrl, cookieContainer);

                if (cookieContainer.Count > 0)
                {
                    client.AddCookiesAsync(baseUrl, cookieContainer).Wait();
                }

                return client;
            });
        }

        public static async Task<Response> GetAsync(FaucetUser faucetUser, string url, string cookie = "", bool follow = true)
        {
            var client = await GetTlsClientAsync(faucetUser, url, follow);

            var request = new RequestBuilder()
                .WithUrl(url)
                .WithMethod(HttpMethod.Get)
                .Build();
            if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.Add("cookie", cookie);
            return await client.RequestAsync(request);
        }

        public static async Task<Response> PostAsync(FaucetUser faucetUser, string url, string body, string cookie = "", bool follow = true)
        {
            var client = await GetTlsClientAsync(faucetUser, url, follow);

            var request = new RequestBuilder()
                .WithUrl(url)
                .WithMethod(HttpMethod.Post)
                .WithHeader("content-type", "application/x-www-form-urlencoded")
                .WithBody(body)
                .Build();
            if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.Add("cookie", cookie);
            return await client.RequestAsync(request);
        }


        public static async Task<Response> Exceute(FaucetUser faucetUser, string url, HttpMethod httpMethod, string body = "", string cookie = "", bool follow = true)
        {
            try
            {
                if(httpMethod == HttpMethod.Get)
                {
                    return await GetAsync(faucetUser, url, cookie, follow);
                }
                else
                {
                    return await PostAsync(faucetUser, url, body, cookie, follow);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Downloads a file (image, pdf, etc) from the given URL using TLS client.
        /// </summary>
        public static async Task<string> DownloadFileAsync(
            FaucetUser faucetUser,
            string url,
            string savePath,
            bool follow = true)
        {
            // Make GET request
            Response response = await GetAsync(faucetUser, url, faucetUser.Session ?? "", follow);

            if (response == null || response.Body == null)
                throw new Exception("Failed to download file, empty response body.");

            // Prefer BodyAsBytes if available
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response.Body);

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath!);
            }
            // Ensure directory exists

            await File.WriteAllBytesAsync(savePath, bytes);

            return savePath;
        }

        /// <summary>
        /// Downloads a file and returns it as Base64 string (without saving to disk).
        /// </summary>
        public static async Task<string> DownloadFileAsBase64Async(
            FaucetUser faucetUser,
            string url,
            bool follow = true)
        {
            Response response = await GetAsync(faucetUser, url, faucetUser.Session ?? "", follow);

            if (response == null || response.Body == null)
                throw new Exception("Failed to download file, empty response body.");

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response.Body);

            return Convert.ToBase64String(bytes);
        }
    }
}
