using CryptoWeb.Shared.Helper.HtmlHelper;
using CryptoWeb.Shared.Helper.HttpHelper;
using CryptoWeb.Shared.Helper.OnlyFaucet;
using CryptoWeb.Shared.Helper.RouteHelper;
using CryptoWeb.Shared.Models.Entities;
using CryptoWeb.Shared.Models.Responses.OnlyFaucet;
using CryptoWeb.Shared.Services.Interfaces;
using CryptoWeb.Shared.Services.Interfaces.OnlyFaucet;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TlsClient.Core.Models.Responses;

namespace CryptoWeb.Shared.Services.Implements.OnlyFaucet
{
    public class FaucetOnlyFaucetService : IFaucetOnlyFaucetService
    {
        private readonly IFaucetHostService faucetHostService;
        private readonly IFaucetUserService faucetUserService;
        private readonly ILogger<FaucetOnlyFaucetService> logger;

        public FaucetOnlyFaucetService(IFaucetHostService faucetHostService, IFaucetUserService faucetUserService,
            ILogger<FaucetOnlyFaucetService> logger
            )
        {
            this.faucetHostService = faucetHostService;
            this.faucetUserService = faucetUserService;
            this.logger = logger;
        }

        public async Task<OnlyFaucetLoginPageResponse> GetLoginPage(string hostName, string email)
        {
            try
            {
                var host = await faucetHostService.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host tidak aktif");
                var user = await faucetUserService.GetByName(hostName, email);
                if (user == null || !user.IsActive || user.IsDeleted || user.IsLocked) throw new Exception("User tidak aktif");
                var response = await GetResponseTls(email, hostName, "/", HttpMethod.Get);
                var doc = await HtmlHelper.ToHtml(response);
                var responsex = OnlyFaucetHtmlHelper.GetLoginToken(doc!);
                return responsex;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> Login(string hostName, string email)
        {
            try
            {
                var host = await faucetHostService.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host tidak aktif");

                var pageLogin = await GetLoginPage(hostName, email);
                if (pageLogin == null) throw new Exception("Gagal get login page");
                if (pageLogin.IsLogged) return pageLogin.IsLogged;

                var body = $"wallet={email}&csrf_token_name={pageLogin.CSRFToken!}";
                var response = await GetResponseTls(email, hostName, FaucetRouteHelper.LoginPost(host), HttpMethod.Post, body);

                var doc = await HtmlHelper.ToHtml(response);

                var isLogged =  OnlyFaucetHtmlHelper.GetLoginToken(doc!);
                return isLogged.IsLogged;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<OnlyFaucetFaycetPageResponse> GetFaucetPage(string hostName, string email, string currency, bool loop = false)
        {
            try
            {
                var host = await faucetHostService.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host tidak aktif");
                var user = await faucetUserService.GetByName(hostName, email);
                if (user == null || !host.IsActive) throw new Exception("User tidak aktif");

                var pageLogin = await GetLoginPage(hostName, email);
                if (pageLogin == null) throw new Exception("Gagal get login page");
                if (!pageLogin.IsLogged && !loop)
                {
                    await Login(hostName, email);
                    loop = true;
                    return await GetFaucetPage(hostName, email, currency, loop);  
                }

                var response = await GetResponseTls(email, hostName, FaucetRouteHelper.FaucetPage(currency, host), HttpMethod.Get);

                var doc = await HtmlHelper.ToHtml(response);
                var xresponse = OnlyFaucetHtmlHelper.GetFaucetToken(doc!);
                var solverIcon = await OnlyFaucetIconHelper.GetOddIconIndexFromImageUrlAsync(xresponse.ImageLink!);
                logger.LogInformation($"Solver : {solverIcon}");

                return xresponse;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<TlsClient.Core.Models.Responses.Response> GetResponseTls(
    string email,
    string hostName,
    string path,
    HttpMethod httpMethod,
    string body = "",
    bool follow = true)
        {
            try
            {
                var host = await faucetHostService.GetByName(hostName);
                if (host == null || !host.IsActive)
                    throw new Exception("Host not active");
                var user = await faucetUserService.GetByName(host.HostName!, email);
                if (user == null || !user.IsActive || user.IsDeleted || user.IsLocked)
                    throw new Exception("User not active");
                string url = host.BaseURL!.TrimEnd('/') + "/" + path.TrimStart('/');
                var response = await TlsHttpHelper.Exceute(user, url, httpMethod, body, user.Session!, follow);
                await UpdateCookie(hostName, email, response);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> UpdateCookie(string host, string email, Response response)
        {
            try
            {

                if (response == null || !response.Headers!.TryGetValue("Set-Cookie", out var setCookieHeaders))
                    return false;
                var user = await faucetUserService.GetByName(host, email);
                if (user == null || !user.IsActive) return false;
                var newCookies = new Dictionary<string, string>();
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
                            newCookies[key] = value;
                        }
                    }
                }

                if (newCookies.Count == 0)
                    return false;


                var oldCookies = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(user.Session))
                {
                    var parts = user.Session.Split(';');
                    foreach (var part in parts)
                    {
                        var nameValue = part.Trim();
                        if (string.IsNullOrEmpty(nameValue)) continue;

                        var kvp = nameValue.Split('=', 2);
                        if (kvp.Length == 2)
                        {
                            var key = kvp[0].Trim();
                            var value = kvp[1].Trim();
                            if (!HttpHelper.IsCookieAttribute(key))
                            {
                                oldCookies[key] = value;
                            }
                        }
                    }
                }

                foreach (var (key, value) in newCookies)
                {
                    if (!HttpHelper.IsCookieAttribute(key))
                    {
                        oldCookies[key] = value;

                    }
                }
                var mergedCookieString = string.Join("; ", oldCookies.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                user.Session = mergedCookieString;

                await faucetUserService.Udpate(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }
    }
}
