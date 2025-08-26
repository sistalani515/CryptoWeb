using Azure;
using CryptoWeb.Shared.Helper.FreeLTCFun;
using CryptoWeb.Shared.Helper.HtmlHelper;
using CryptoWeb.Shared.Helper.HttpHelper;
using CryptoWeb.Shared.Helper.IconCaptcha;
using CryptoWeb.Shared.Helper.RouteHelper;
using CryptoWeb.Shared.Helper.SQLHelper;
using CryptoWeb.Shared.Models.Entities;
using CryptoWeb.Shared.Models.Requests.IconCaptcha;
using CryptoWeb.Shared.Models.Responses.FreeLTCFun;
using CryptoWeb.Shared.Models.Responses.IconCaptcha;
using CryptoWeb.Shared.Services.Interfaces;
using CryptoWeb.Shared.Services.Interfaces.FreeLTCFun;
using CryptoWeb.Shared.Services.Interfaces.ShortLink;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TlsClient.Core.Models.Responses;

namespace CryptoWeb.Shared.Services.Implements.FreeLTCFun
{
    public class FaucetFreeLTCFunService : IFaucetFreeLTCFunService
    {
        private readonly IFaucetHostService faucetHost;
        private readonly IFaucetUserService faucetUser;
        private readonly IFaucetCurrencyService faucetCurrencyService;
        private readonly IFaucetShortLinkService faucetShortLinkService;
        private readonly ILogger<FaucetFreeLTCFunService> logger;

        public FaucetFreeLTCFunService(IFaucetHostService faucetHost, 
            IFaucetUserService faucetUser, 
            IFaucetCurrencyService faucetCurrencyService,
            IFaucetShortLinkService faucetShortLinkService,
            ILogger<FaucetFreeLTCFunService> logger)
        {
            this.faucetHost = faucetHost;
            this.faucetUser = faucetUser;
            this.faucetCurrencyService = faucetCurrencyService;
            this.faucetShortLinkService = faucetShortLinkService;
            this.logger = logger;
        }

        public async Task<FreeLTCFunFaucetRequest> GetFaucetPage(string hostName, string email, string currencyName)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");
                var currency = await faucetCurrencyService.GetHostCurrencyByName(hostName, currencyName);
                if (currency == null || !currency.IsActive) throw new Exception("Currency not Active");
                var response = await GetResponse(email, hostName, FaucetRouteHelper.FaucetPage(currencyName, host), HttpMethod.Get);
                var html = await response.ToHtml();
                logger.LogInformation(html!.DocumentNode.InnerHtml.Contains("lh-1 mb-1 font-weight-bold").ToString());
                var isLoggin = html!.GetLogin();
                if (!isLoggin.IsLogged)
                {
                    var doLogin =  await DoLogin(hostName, email);
                    if (!doLogin.IsLogged) throw new Exception("Gagal re-login");
                    return await GetFaucetPage(hostName, email, currencyName);
                }
                var token = html!.GetFaucetToken();
                return token;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<FreeLTCFunLoginPageResponse> GetLoginPage(string hostName, string email)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");
                var response = await GetResponse(email, hostName, "", HttpMethod.Get);
                var html = await response.ToHtml();
                return html!.GetLogin();
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<FreeLTCFunLoginPageResponse> DoLogin(string hostName, string email)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");
                var loginPage = await GetLoginPage(hostName, email);
                if (loginPage == null) throw new Exception("Gagal get login page");
                if (loginPage.IsLogged) return loginPage;
                var IconCaptcha = await SolveIcon(hostName, email, loginPage.CaptchaToken!);
                var body = $"wallet={email}" +
                            $"&csrf_token_name={loginPage.CSRF!}" +
                            $"&captcha=icaptcha" +
                            $"&_iconcaptcha-token={loginPage.CaptchaToken!}&ic-rq=1" +
                            $"&ic-wid={IconCaptcha.Coordinates.WidgetId!}" +
                            $"&ic-cid={IconCaptcha.Coordinates.Identifier!}&ic-hp=";
                var response = await GetResponse(email, hostName, FaucetRouteHelper.LoginPost(host), HttpMethod.Post, body, "form");
                var doc = await response.ToHtml();
                var isLogin = doc!.GetLogin();

                if (isLogin.IsLogged)
                {
                    try
                    {
                        var user = await faucetUser.GetByName(hostName, email);
                        if (user != null)
                        {
                            user.LastLogin = DateTime.UtcNow;
                            await faucetUser.Udpate(user);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    
                }
                return isLogin;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> DoBacthLogin(string email)
        {
            try
            {
                var hosts = await faucetHost.GetAll();
                string result = $"";
                foreach(var host in hosts)
                {
                    var user = await faucetUser.GetByName(host.HostName!, email);
                    if(user == null)
                    {
                        await faucetUser.Insert(new FaucetUser
                        {
                            Host = host.HostName!,
                            Email = email,
                            IsActive = true
                        });
                    }
                    try
                    {
                        var login = await DoLogin(host.HostName!, email);
                        result += $"{email}|{host.HostName}=>{login.IsLogged}\n";
                    }
                    catch (Exception)
                    {
                        result += $"{email}|{host.HostName}=>error\n";

                    }

                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<string> DoClaim(string hostName, string email, string currencyName, bool force = false)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive)
                {
                    await Task.Delay(1000);
                    throw new Exception("Host not active");
                }
                var userx = await faucetUser.GetByName(host.HostName!, email);
                if (userx == null || !userx.IsActive || userx.IsDeleted || userx.IsLocked)
                {
                    await Task.Delay(1000);
                    throw new Exception("User not active");
                }
                var userCurrency = await faucetCurrencyService.GetUserCurrencyByName(hostName, email, currencyName);
                if(userCurrency != null && !force)
                {
                    if (!userCurrency!.IsActive)
                    {
                        await Task.Delay(1000);
                        throw new Exception("Currency not active");
                    }
                    if (userCurrency.Changed.HasValue && (DateTime.UtcNow - userCurrency.Changed.Value).TotalSeconds < 10) throw new Exception("Set delay currency");
                }
                var faucetPage = await GetFaucetPage(hostName, email, currencyName);
                if (faucetPage == null || !faucetPage.IsValid) throw new Exception("Gagal get FaucetPage");
                IconCaptchaImageResultResponse solveCaptcha = new IconCaptchaImageResultResponse();
                if (!string.IsNullOrWhiteSpace(faucetPage.CaptchaToken))
                {
                    solveCaptcha = await SolveIcon(hostName, email, faucetPage.CaptchaToken!);
                    if (solveCaptcha == null || !solveCaptcha.Completed) throw new Exception("Gagal solve IconCaptcha");
                }

                string body = "";
                if(host.HostName == "claimcryptoin")
                {
                    body = $"csrf_token_name={faucetPage.CSRF!}" +
                            $"&token={faucetPage.WebToken!}" +
                            $"&wallet={userx.Email!}" +
                            $"&captcha=recaptchav3" +
                            $"&recaptchav3=0cAFcWeA7YJpogB2dRO0T3MWGWuvuQFr0N69zE9aGRm8nt9o6SxyDVoQFMvCj_rR5QSjt7ACABfN_rKYblOV4noJMluHxMd_djJSScCIezu0q7Z3mHjj4vfonKPod73y70cLEpJmzfWo4PYyYm1tVT9qIN7yFOxa9oB-MsMvZnj8EG84l-pgGm0_-ETcfiB7bcELSH6ZwrLip40THhqDaocgNiA_ZMnJ9vLv374M9R6LQO9ky24j4TsP0fJxo7q1E_eJ8CCzocfwbRit8yQq4MhMNqgd17xVOFm759mNMph8WbLNo0q0ndDPuXouExKj1_k7R2EZ4VTQZaG8Jq1Up_IVOJcq2AtBlHb2w16kCrzBobrdZqivfvfikmwe-j-asSzeGJxAH94bDIsOjGJlE6hffrZQdBBuDbi9pPLJLPoyI121J5ezQkvuR4iQli4r1j6zXNvqjj4AhvIIUBxeO8liAo1Ob1XVq4qSIU0p5HCaVvLKR1O6kD-LeWmM7KLF3hErwtxzVwwFKuzN2AKIEArXmy6Ce4erOHN9wMtCEnLVxR_Po0yUw9xPR5i6gHq3JTf27TRJLbgii8z90jfoYEfuKnO-mqiI5un-YBwCfTC1ZQtCnmmoZvq33GxUT_SB5QECHbYieh0chMtQ7JR6XVSsFsfzrLw9z-t2oCF2N2Eh6FFAQgFFNH5nK1XZr_IF7HqpHmPN9lkRddubhpt8SQAPCEDlFf4GZ6ANvQYzDtiFoZNwnXRuL2KHCX1D-Pp3ndO5a0796UJIi3Y1lMhFDMHUjJEKFcFb9SG6EEyEKNsiuryNmYB1-ZhLw";
                }
                else
                {
                    body = $"csrf_token_name={faucetPage.CSRF!}" +
                       $"&token={faucetPage.WebToken!}&captcha=icaptcha" +
                       $"&_iconcaptcha-token={faucetPage.CaptchaToken}&ic-rq=1" +
                       $"&ic-wid={solveCaptcha.Coordinates.WidgetId!}" +
                       $"&ic-cid={solveCaptcha.Coordinates.Identifier!}&ic-hp=" +
                       $"&wallet={email}";

                }


                var response = await GetResponse(email, hostName, FaucetRouteHelper.ClaimFaucet(currencyName, host), HttpMethod.Post, body, "form");
                var doc = await response.ToHtml();
                var message = HtmlHelper.GetMessage(doc!);
                var value = HtmlHelper.GetClaimed(message);
                bool isClaimed = value != null && decimal.Parse(value) > 0;
                if (userCurrency == null)
                {
                    await faucetCurrencyService.InsertUserCurrency(new FaucetUserCurrency
                    {
                        IsActive = isClaimed,
                        TotalAmount = isClaimed ? value : "0",
                        Date = DateTime.UtcNow.ToString("yyyyMMdd"),
                        Created = DateTime.UtcNow,
                        Email = email,
                        HostName = hostName,
                        Name = currencyName,
                        TotalClaim = isClaimed ? 1 : 0,
                        Changed = DateTime.UtcNow
                    });
                    if (isClaimed)
                    {
                        userx.LastClaim = DateTime.UtcNow;
                        await faucetUser.Udpate(userx);
                    }
                    
                }
                else
                {
                    userCurrency.IsActive = isClaimed;
                    userCurrency.TotalClaim += isClaimed ? 1 : 0;
                    userCurrency.TotalAmount =( decimal.Parse(userCurrency!.TotalAmount!) + decimal.Parse(value!)).ToString();
                    userCurrency.Changed = DateTime.UtcNow;
                    await faucetCurrencyService.UpdateUserCurrency(userCurrency);
                    if (isClaimed)
                    {
                        userx.LastClaim = DateTime.UtcNow;
                        await faucetUser.Udpate(userx);
                    }
                }
                //if(message.ToLower().Contains("invalid claim"))
                //{
                //    if(userCurrency == null)
                //    {
                //        var userCurrencyx = await faucetCurrencyService.GetUserCurrencyByName(hostName, email, currencyName);
                //        if(userCurrencyx != null)
                //        {
                //            userCurrencyx.IsActive = true;
                //            await faucetCurrencyService.UpdateUserCurrency(userCurrencyx);

                //        }
                //    }
                //    else
                //    {
                //        userCurrency.IsActive = true;
                //        await faucetCurrencyService.UpdateUserCurrency(userCurrency);

                //    }
                //}
                if (message.ToLower().Contains("shortlink"))
                {
                    try
                    {
                        var user = await faucetUser.GetByName(hostName, email);
                        if(user != null)
                        {
                            user.IsSL = true;
                            await faucetUser.Udpate(user);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                if (message.ToLower().Contains("blacklist"))
                {
                    try
                    {
                        var user = await faucetUser.GetByName(hostName, email);
                        if (user != null)
                        {
                            user.IsLocked = true;
                            await faucetUser.Udpate(user);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                if (message.ToLower().Contains("sent")) message = $"[{userCurrency!.TotalClaim}]-{message}";
                return message;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IconCaptchaImageResponse> GetIconCaptcha(string hostName, string email, string xIconCaptcha)
        {
            try
            {
                var body = new IconCaptchaImageRequest
                {
                    WidgetId = IconCaptchaHelper.GenerateWidgetId(),
                    Token = xIconCaptcha,
                    Timestamp = IconCaptchaHelper.GetCurrentUnixTimestampMillis()

                };
                body.IntTimeStamp = IconCaptchaHelper.GenerateTimestamp(body.Timestamp);
                var encBody = IconCaptchaHelper.EncodeBase64(body.ToString());
                var response = await GetResponse(email, hostName, FaucetRouteHelper.CaptchaRequest, HttpMethod.Post, encBody, "multi", xIconCaptcha);
                var imageResponse = await response.ResponseToImage(body.WidgetId!);
                return imageResponse;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IconCaptchaImageResultResponse> SolveIcon(string hostName, string email, string xIconCaptcha)
        {
            try
            {
                var imageRaw = await GetIconCaptcha(hostName, email, xIconCaptcha);
                if (imageRaw == null || imageRaw.X == 0) throw new Exception("Gagal get IconCapthca");
                var body = new
                {
                    widgetId = imageRaw.WidgetId!,
                    challengeId = imageRaw.Identifier!,
                    action = "SELECTION",
                    x = imageRaw.X,
                    y = imageRaw.Y,
                    width = 320,
                    token = xIconCaptcha,
                    timestamp = IconCaptchaHelper.GetCurrentUnixTimestampMillis(),
                    initTimestamp = IconCaptchaHelper.GenerateTimestamp(IconCaptchaHelper.GetCurrentUnixTimestampMillis())
                };
                var bodyJson = JsonConvert.SerializeObject(body);
                var encBody = IconCaptchaHelper.EncodeBase64(bodyJson);
                var response = await GetResponse(email, hostName, FaucetRouteHelper.CaptchaRequest, HttpMethod.Post, encBody, "multi", xIconCaptcha);
                var imageResponse = await response.ImageToResult();
                if (imageResponse == null || !imageResponse.Completed) throw new Exception("Gagal solve IconCapthca");
                imageResponse.Coordinates = imageRaw;
                return imageResponse;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetSLPage(string hostName, string email, string currencyName)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");
                var response = await GetResponse(email, hostName, FaucetRouteHelper.SLPage(host, currencyName), HttpMethod.Get);
                var doc = await response.ToHtml();
                var isLoggin = doc!.GetLogin();
                if (!isLoggin.IsLogged)
                {
                    var doLogin = await DoLogin(hostName, email);
                    if (!doLogin.IsLogged) throw new Exception("Gagal re-login");
                    return await GetSLPage(hostName, email, currencyName);
                }
                var result = HtmlHelper.PageToSL(doc!);
                return result!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<FreeLTCFunFaucetRequest> GetLinkSL(string hostName, string email, string link)
        {
            try
            {
                var response = await GetResponse(email, hostName, link, HttpMethod.Get);
                var doc = await response.ToHtml();
                var token = doc!.GetFaucetToken();
                return token;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<string> supportedSL = [
            "btshort",
            "faucetshort",
            ];

        public async Task<bool> SolveSL(string hostName, string email, string currencyName)
        {
            try
            {
                var SLs = await GetSLPage(hostName, email, currencyName);
                bool result = false;
                bool run = false;
                foreach (var sl in SLs.Where(e => supportedSL.Contains(e.Key.Split("|")[0])))
                {
                    try
                    {
                        run = true;
                        string type = sl.Key.Split("|")[0];
                        var slLink = await GetLinkSL(hostName, email, sl.Key.Split("|")[1]);
                        if (slLink == null || !slLink.IsValid) continue;
                        var solveCaptcha = await SolveIcon(hostName, email, slLink.CaptchaToken!);
                        if (solveCaptcha == null || !solveCaptcha.Completed) throw new Exception("Gagal solve IconCaptcha");
                        var body = $"csrf_token_name={slLink.CSRF!}" +
                            $"&token={slLink.WebToken!}&captcha=icaptcha" +
                            $"&_iconcaptcha-token={slLink.CaptchaToken}&ic-rq=1" +
                            $"&ic-wid={solveCaptcha.Coordinates.WidgetId!}" +
                            $"&ic-cid={solveCaptcha.Coordinates.Identifier!}&ic-hp=";
                        var host = await faucetHost.GetByName(hostName);
                        if (host == null || !host.IsActive) throw new Exception("Host not active");
                        var response = await GetResponse(email, hostName, 
                            FaucetRouteHelper.SolveSL(host, currencyName, 
                            sl.Key.StringToNumber()), HttpMethod.Post, body, "form", follow:false);
                        if ((response.StatusCode == System.Net.HttpStatusCode.Found || response.StatusCode == System.Net.HttpStatusCode.SeeOther) == false) continue;
                        var locationHeader = response.Headers!.FirstOrDefault(e => e.Key.ToLower().Contains("location"))!.Value.FirstOrDefault()!;
                        if (string.IsNullOrWhiteSpace(locationHeader)) continue;
                        string path = new Uri(locationHeader).LocalPath;
                        var solved = await faucetShortLinkService.SolveSL(type, path);
                        return await ConfirmSL(hostName, email, solved);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
                }
                if (!run)
                {
                    try
                    {
                        var user = await faucetUser.GetByName(hostName, email);
                        if(user != null)
                        {
                            user.SLCompleted = DateTime.UtcNow;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ConfirmSL(string hostName, string email, FreeLTCFunSLResponse sLResponse)
        {
            try
            {
                if (sLResponse == null || !sLResponse.Status!.ToLower().Contains("success")) return false;

                var response = await GetResponse(email, hostName, sLResponse.Url!, HttpMethod.Get);
                var doc = await response.ToHtml();
                var message = HtmlHelper.GetMessage(doc!);
                //logger.LogInformation(message);
                if (message.ToLower().Contains("blacklist"))
                {
                    try
                    {
                        var user = await faucetUser.GetByName(hostName, email);
                        if (user != null)
                        {
                            user.IsLocked = true;
                            await faucetUser.Udpate(user);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    return false;
                }
                if (message.ToLower().Contains("not have sufficient")) return false;

                var amount = HtmlHelper.GetClaimed(message);
                if (amount == "0") return false;

                try
                {
                    var user = await faucetUser.GetByName(hostName, email);
                    if (user != null)
                    {
                        user.IsSL = false;
                        await faucetUser.Udpate(user);
                    }
                }
                catch (Exception)
                {

                }
                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(FreeLTCFunFaucetRequest, Dictionary<string, bool>)> GetChallenges(string hostName, string email)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");
                var r = await GetResponse(email, hostName, FaucetRouteHelper.Challenge(host), HttpMethod.Get);
                var doc = await r.ToHtml();
                var challenges = HtmlHelper.GetChalleng(doc!);
                var captcha = doc!.GetFaucetToken();
                return (captcha, challenges);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<string> SolveChallenge(string hostName, string email, FreeLTCFunFaucetRequest faucetPage, string url, string name)
        {
            try
            {
                if (faucetPage == null || string.IsNullOrWhiteSpace(faucetPage.CaptchaToken)) throw new Exception("Gagal get capthca");
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");

                var solveCaptcha = await SolveIcon(hostName, email, faucetPage.CaptchaToken!);
                if (solveCaptcha == null || !solveCaptcha.Completed) throw new Exception("Gagal solve captcha");
                var body = $"csrf_token_name={faucetPage.CSRF!}" +
                    $"&currency={name.ToUpper()}" +
                    $"&token={faucetPage.WebToken!}&captcha=icaptcha" +
                    $"&_iconcaptcha-token={faucetPage.CaptchaToken}&ic-rq=1" +
                    $"&ic-wid={solveCaptcha.Coordinates.WidgetId!}" +
                    $"&ic-cid={solveCaptcha.Coordinates.Identifier!}&ic-hp=";
                var response = await GetResponse(email, hostName, url, HttpMethod.Post, body, "form");
                var doc = await response.ToHtml();
                var message = HtmlHelper.GetMessage(doc!);
                var claimed = HtmlHelper.GetClaimed(message);
                logger.LogInformation(message);
                return claimed;

            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<string> RawSolveChallenge(string hostName, string email, string url, string name, string csrf, string iconToken)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");

                var solveCaptcha = await SolveIcon(hostName, email, iconToken);
                if (solveCaptcha == null || !solveCaptcha.Completed) throw new Exception("Gagal solve captcha");
                var body = $"csrf_token_name={csrf}" +
                    $"&currency={name.ToUpper()}" +
                    $"&captcha=icaptcha" +
                    $"&_iconcaptcha-token={iconToken}&ic-rq=1" +
                    $"&ic-wid={solveCaptcha.Coordinates.WidgetId!}" +
                    $"&ic-cid={solveCaptcha.Coordinates.Identifier!}&ic-hp=";
                var response = await GetResponse(email, hostName, url, HttpMethod.Post, body, "form");
                var doc = await response.ToHtml();
                var message = HtmlHelper.GetMessage(doc!);
                var claimed = HtmlHelper.GetClaimed(message);
                logger.LogInformation(message);
                return claimed;

            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<string> BatchSolveChallenge(string hostName, string email, string currency)
        {
            try
            {
                var (captcha, challenge) = await GetChallenges(hostName, email);
                if (captcha == null && string.IsNullOrWhiteSpace(captcha!.CaptchaToken)) throw new Exception("Gagal get capctha");
                foreach(var chall in challenge.Where(e => e.Value == true))
                {
                    var solve = await SolveChallenge(hostName, email, captcha, chall.Key, currency);
                    if(solve != "0") return solve; 
                }
                return "0";
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<bool> UpdateCookie(string host, string email, HttpResponseMessage response)
        {
            try
            {
                if (response == null || !response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
                    return false;
                var user = await faucetUser.GetByName(host, email);
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

                await faucetUser.Udpate(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }


    //    private async Task<TlsClient.Core.Models.Responses.Response> GetResponseTls(
    //string email,
    //string hostName,
    //string path,
    //HttpMethod httpMethod,
    //object body = null!,
    //string typeBody = "",
    //string xIconCaptcha = "",
    //bool follow = true)
    //    {
    //        try
    //        {
    //            var host = await faucetHost.GetByName(hostName);
    //            if (host == null || !host.IsActive)
    //                throw new Exception("Host not active");

    //            var user = await faucetUser.GetByName(host.HostName!, email);
    //            if (user == null || !user.IsActive || user.IsDeleted || user.IsLocked)
    //                throw new Exception("User not active");

    //            string url = host.BaseURL!.TrimEnd('/') + "/" + path.TrimStart('/');

    //            if (httpMethod == HttpMethod.Get)
    //            {
    //                // GET request
    //                return await TlsHttpHelper.GetAsync(user, url);
    //            }
    //            else if (httpMethod == HttpMethod.Post)
    //            {
    //                // POST request
    //                if (typeBody == "form" && body != null)
    //                {
    //                    if (body is Dictionary<string, string> formData)
    //                    {
    //                        return await TlsHttpHelper.PostAsync(user, url, formData);
    //                    }
    //                    else
    //                    {
    //                        throw new Exception("POST body must be Dictionary<string,string> for form type");
    //                    }
    //                }
    //                else
    //                {
    //                    throw new Exception("Unsupported POST type or body is null");
    //                }
    //            }
    //            else
    //            {
    //                throw new Exception("Unsupported HttpMethod");
    //            }

    //            // Optionally, update cookies if you want to store them
    //            // await UpdateTlsCookies(hostName, email);

    //            return null!;
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //    }


        private async Task<HttpResponseMessage> GetResponse(string email, string hostName, string path, HttpMethod httpMethod,
            object body = null!, string typeBody = "", string xIconCapthca = "", bool follow = true)
        {
            try
            {
                var host = await faucetHost.GetByName(hostName);
                if (host == null || !host.IsActive) throw new Exception("Host not active");
                var user = await faucetUser.GetByName(host.HostName!, email);
                if (user == null || !user.IsActive || user.IsDeleted || user.IsLocked) throw new Exception("User not active");
                var httpClient = HttpHelper.GetHttpClient(user, host.BaseURL!, follow);
                var request = new HttpRequestMessage(httpMethod, path);
                if (!string.IsNullOrWhiteSpace(xIconCapthca)) request.Headers.Add($"X-IconCaptcha-Token", xIconCapthca);
                if (httpMethod != HttpMethod.Get && body != null && !string.IsNullOrWhiteSpace(typeBody))
                {
                    if (typeBody == "form")
                    {
                        request.Content = new StringContent(body.ToString()!, Encoding.UTF8, "application/x-www-form-urlencoded");
                    }
                    else if (typeBody == "multi")
                    {
                        var formData = new MultipartFormDataContent
                        {
                            { new StringContent(body.ToString()!), "payload" }
                        };
                        request.Content = formData;
                    }
                }
                var response = await httpClient.SendAsync(request);
                await UpdateCookie(hostName, email, response);
                return response;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
