using CryptoWeb.Shared.Models.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TlsClient.Core;

namespace CryptoWeb.Shared.Helper.HttpHelper
{
    public static class HttpHelper
    {
        private static ConcurrentDictionary<string, HttpClient> _httpClients => [];
        private static ConcurrentDictionary<string, CookieContainer> _cookieContainers => [];
        private static void LoadCookiesFromSession(FaucetUser userFaucet, string baseUrl, CookieContainer container)
        {
            if (string.IsNullOrWhiteSpace(userFaucet.Session))
                return;

            if (container.Count > 0)
                return; // Already loaded
            var cookies = ParseCookies(userFaucet.Session);
            var uri = new Uri(baseUrl);

            foreach (var (cookieName, cookieValue) in cookies)
            {
                container.Add(uri, new Cookie(cookieName, cookieValue));
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


        public static bool IsCookieAttribute(string key)
        {
            var attributes = new[]
            {
        "Path", "Domain", "Max-Age", "Expires", "Secure", "HttpOnly", "SameSite"
    };
            return attributes.Contains(key, StringComparer.OrdinalIgnoreCase);
        }
        public static bool IsCommonCookie(string key)
        {
            var attributes = new[]
            {
        "ci_session","csrf_cookie_name", "AccountAlready","_ym_d","_ym_isad","_ym_uid","captcha","PHPSESSID", "phpsessid", "cf_clearance"
    };
            return attributes.Contains(key, StringComparer.OrdinalIgnoreCase);
        }
        public static HttpClient GetHttpClient(FaucetUser faucetUser, string baseUrl, bool follow = true)
        {
            string baseKey = $"{faucetUser.Host!}-{faucetUser.Email!}";

            // Always reuse the same cookie container for the same user/host

            // Unique key for HttpClient instance depending on redirect setting
            string clientKey = $"{baseKey}-redirect:{follow}";
            try
            {
                return _httpClients.GetOrAdd(clientKey, _ =>
                {

                    var cookieContainer = _cookieContainers.GetOrAdd(baseKey, _ => new CookieContainer());
                    var handler = new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = cookieContainer,
                        AllowAutoRedirect = follow
                    };
                    LoadCookiesFromSession(faucetUser, baseUrl, cookieContainer);
                    var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
                    //httpClient.DefaultRequestHeaders.Add("Referer",baseUrl + "dashboard");// = new Uri(baseUrl + "/dashboard");
                    httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");// = new Uri(baseUrl);

                    httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("id-ID,id;q=0.9,en-US;q=0.8,en;q=0.7");

                    httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                    //httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not;A=Brand\";v=\"99\", \"Google Chrome\";v=\"139\", \"Chromium\";v=\"139\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-full-version", "\"139.0.7258.128\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-arch", "\"x86\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform-version", "\"19.0.0\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-model", "\"\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-bitness", "\"64\"");
                    httpClient.DefaultRequestHeaders.Add("sec-ch-ua-full-version-list", "\"Not;A=Brand\";v=\"99.0.0.0\", \"Google Chrome\";v=\"139.0.7258.128\", \"Chromium\";v=\"139.0.7258.128\"");

                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                    httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");

                    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");


                    if (!string.IsNullOrWhiteSpace(faucetUser.IpAddress)) httpClient.DefaultRequestHeaders.Add("x-forwarded-for", faucetUser.IpAddress!);
                    if (!string.IsNullOrWhiteSpace(faucetUser.UA))
                    {
                        httpClient.DefaultRequestHeaders.Add("User-Agent", faucetUser.UA!);
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Add("User-Agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36");
                    }
                    httpClient.DefaultRequestHeaders.Referrer = new Uri(baseUrl);
                    return httpClient;

                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static HttpClient GetHttpClientCF(FaucetUser faucetUser, string baseUrl, bool follow = true)
        {
            string baseKey = $"{faucetUser.Host!}-{faucetUser.Email!}";
            string clientKey = $"{baseKey}-redirect:{follow}-proxy";

            return _httpClients.GetOrAdd(clientKey, _ =>
            {
                var cookieContainer = _cookieContainers.GetOrAdd(baseKey, _ => new CookieContainer());
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = cookieContainer,
                    AllowAutoRedirect = follow
                };
                LoadCookiesFromSession(faucetUser, baseUrl, cookieContainer);
                var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

                // --- Browser-like headers ---
                httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
                httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("id-ID,id;q=0.9,en-US;q=0.8,en;q=0.7");

                httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");

                httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not;A=Brand\";v=\"99\", \"Google Chrome\";v=\"139\", \"Chromium\";v=\"139\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-full-version", "\"139.0.7258.128\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-arch", "\"x86\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform-version", "\"19.0.0\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-bitness", "\"64\"");
                httpClient.DefaultRequestHeaders.Add("sec-ch-ua-full-version-list", "\"Not;A=Brand\";v=\"99.0.0.0\", \"Google Chrome\";v=\"139.0.7258.128\", \"Chromium\";v=\"139.0.7258.128\"");

                httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                if (!string.IsNullOrWhiteSpace(faucetUser.IpAddress))
                    httpClient.DefaultRequestHeaders.Add("x-forwarded-for", faucetUser.IpAddress!);

                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                    string.IsNullOrWhiteSpace(faucetUser.UA)
                    ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36"
                    : faucetUser.UA
                );

                return httpClient;
            });
        }

    }
}
