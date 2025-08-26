using CryptoWeb.Shared.Models.Responses.FreeLTCFun;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Helper.FreeLTCFun
{
    public static class FreeLTCFunHtmlHelper
    {
        public static FreeLTCFunLoginPageResponse GetLogin(this HtmlDocument document)
        {
			try
			{
                FreeLTCFunLoginPageResponse response = new FreeLTCFunLoginPageResponse();
                var content = document.DocumentNode.InnerHtml;
                if (
                        content!.Contains("below if you are ready to end your current session") ||
                        content.Contains("login success") ||
                        content.Contains("/logout") ||
                        content.Contains("key=\"t-logout\"") ||
                        content.Contains("new account registered")||                        
                        content.Contains("Dashboard")||
                        content.Contains("lh-1 mb-1 font-weight-bold")
                        ) response.IsLogged = true;
                if (response.IsLogged)
                {
                    return response;
                }
                var capthcaToken = document.DocumentNode.SelectSingleNode("//input[@name='_iconcaptcha-token']");
                if(capthcaToken != null) response.CaptchaToken = capthcaToken.GetAttributeValue("value", "");
                var csrfToken = document.DocumentNode.SelectSingleNode("//input[@name='csrf_token_name']");
                if(csrfToken != null) response.CSRF = csrfToken.GetAttributeValue("value", "");
                return response;

            }
            catch (Exception)
			{
                return null!;
			}
        }

        public static FreeLTCFunFaucetRequest GetFaucetToken(this HtmlDocument document)
        {
            try
            {
                FreeLTCFunFaucetRequest response = new FreeLTCFunFaucetRequest();
                var content = document.DocumentNode.InnerText;
                var alert = document.DocumentNode
                .SelectSingleNode("//div[contains(@class,'alert-danger') and contains(@class,'text-center')]")
                ?.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(alert)) return response ;
                var capthcaToken = document.DocumentNode.SelectSingleNode("//input[@name='_iconcaptcha-token']");
                if (capthcaToken != null) response.CaptchaToken = capthcaToken.GetAttributeValue("value", "");

                var csrfToken = document.DocumentNode.SelectSingleNode("//input[@name='csrf_token_name']");
                if (csrfToken != null) response.CSRF = csrfToken.GetAttributeValue("value", "");

                var tokenWeb = document.DocumentNode.SelectSingleNode("//input[@name='token']");
                if (tokenWeb != null) response.WebToken = tokenWeb!.GetAttributeValue("value", "");
                if (!string.IsNullOrWhiteSpace(response.CSRF)) response.IsValid = true;
                return response;

            }
            catch (Exception)
            {
                return null!;
            }
        }

    }
}
