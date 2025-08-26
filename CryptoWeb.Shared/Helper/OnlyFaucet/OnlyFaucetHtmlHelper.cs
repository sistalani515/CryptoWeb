using Azure.Core;
using CryptoWeb.Shared.Models.Responses.OnlyFaucet;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CryptoWeb.Shared.Helper.OnlyFaucet
{
    public static class OnlyFaucetHtmlHelper
    {
        public static OnlyFaucetLoginPageResponse GetLoginToken(HtmlDocument doc)
        {
            var csrfToken = doc.DocumentNode.SelectSingleNode("//input[@name='csrf_token_name']");
            var response = new OnlyFaucetLoginPageResponse();
            var content = doc.DocumentNode.InnerHtml;
            if (
                    content.Contains("/auth/logout")
                    ) response.IsLogged = true;

            if (csrfToken != null )
            {
                response.CSRFToken = csrfToken.GetAttributeValue("value", "");
            }
            else
            {
                return new OnlyFaucetLoginPageResponse();
            }
            return response;
        }

        public static OnlyFaucetFaycetPageResponse GetFaucetToken(HtmlDocument doc)
        {
            OnlyFaucetFaycetPageResponse response = new OnlyFaucetFaycetPageResponse();
            var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(text(), 'var nonce')]");
            if (scriptNode != null)
            {
                string scriptText = scriptNode.InnerText;
                var nonceMatch = Regex.Match(scriptText, @"var\s+nonce\s*=\s*'([^']+)'");
                string nonce = nonceMatch.Success ? nonceMatch.Groups[1].Value : null!;
                var imgMatch = Regex.Match(scriptText, @"img\.src\s*=\s*'([^']+)'");
                string imgSrc = imgMatch.Success ? imgMatch.Groups[1].Value : null!;
                response.Nonce = nonce;
                response.ImageLink = imgSrc;
            }
            if (!string.IsNullOrWhiteSpace(response.Nonce) && !string.IsNullOrWhiteSpace(response.ImageLink)) response.IsValid = true;
            return response;
        }


    }
}
