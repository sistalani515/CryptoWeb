using Azure;
using CryptoWeb.Shared.Models.Responses.FreeLTCFun;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Helper.HtmlHelper
{
    public static class HtmlHelper
    {
        public static async Task<HtmlDocument?> ToHtml(this HttpResponseMessage responseMessage)
        {
			try
			{
                var responseText = await responseMessage.Content!.ReadAsStringAsync();
                var document = new HtmlDocument();
                document.LoadHtml(responseText);
                return document;
            }
			catch (Exception)
			{
                return null!;
			}
        }
        public static async Task<HtmlDocument?> ToHtml(this TlsClient.Core.Models.Responses.Response responseMessage)
        {
            try
            {
                var responseText = responseMessage.Body!;
                var document = new HtmlDocument();
                document.LoadHtml(responseText);
                return document;
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public static string GetMessage(HtmlDocument document)
        {
            try
            {
                var scriptNodes = document.DocumentNode.SelectNodes("//script");
                if (scriptNodes != null)
                {
                    foreach (var script in scriptNodes)
                    {
                        string scriptText = script.InnerText;
                        if (scriptText.Contains("Toast.fire") && scriptText.Contains("icon"))
                        {
                            // Gunakan Regex untuk menangkap nilai 'html'
                            var match = Regex.Match(scriptText, @"text:\s*'([^']*)'", RegexOptions.Singleline);
                            if (match.Success)
                            {
                                string htmlMessage = match.Groups[1].Value;
                                if (!string.IsNullOrWhiteSpace(htmlMessage)) return htmlMessage;
                            }
                        }
                        if (scriptText.Contains("Swal.fire") && scriptText.Contains("icon"))
                        {
                            // Gunakan Regex untuk menangkap nilai 'html'
                            var match = Regex.Match(scriptText, @"html:\s*'([^']*)'", RegexOptions.Singleline);
                            if (match.Success)
                            {
                                string htmlMessage = match.Groups[1].Value;
                                return htmlMessage;
                            }
                        }
                    }
                    return "";
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string GetClaimed(string message)
        {
            try
            {
                if (!message!.ToLower().Contains("to your faucetpay account")) return "0";
                string numericPart = Regex.Match(message, @"\d+(\.\d+)?").Value;
                if (!string.IsNullOrWhiteSpace(numericPart)) return numericPart;
                return "0";
            }
            catch (Exception)
            {
                return "0";
            }
        }

        public static string ExtractHiddenInputsAsQueryString(HtmlDocument html)
        {

            var inputs = html.DocumentNode
                .SelectNodes("//input[@type='hidden']")
                ?.Select(node =>
                    WebUtility.UrlEncode(node.GetAttributeValue("name", "")) + "=" +
                    WebUtility.UrlEncode(node.GetAttributeValue("value", ""))
                );

            return inputs != null ? string.Join("&", inputs) : string.Empty;
        }

        private static List<string> supportedSL = [
           "btshort",
            "faucetshort",
            ];

        public static Dictionary<string, int>? PageToSL(HtmlDocument doc)
        {
            return doc.DocumentNode
                .SelectNodes("//div[contains(@class,'balance-card-body')]")?
                .Select(card =>
                {
                    var name = card.SelectSingleNode(".//h4[@class='card-title mt-0']")?.InnerText.Trim();
                    var link = card.SelectSingleNode(".//a[@href]")?.GetAttributeValue("href", "").Trim();
                    var badge = card.SelectSingleNode(".//span[contains(@class,'badge-info')]")?.InnerText.Trim();
                    var leftValue = int.TryParse(badge?.Split('/')[0], out var val) ? val : 0;
                    return new { Name = name!, Link = link!, LeftValue = leftValue };
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Link) 
                    //&& x.Name.ToLower().Contains("btshort") ||
                    //x.Name.ToLower().Contains("faucetshort") 
                    //&& supportedSL.Contains(x.Name!)
                    )
                .ToDictionary(x => $"{x.Name!.ToLower()}|{x.Link!}|{x.Link.StringToNumber()}", x => x.LeftValue);
        }

        public static int StringToNumber(this string value)
        {
            var match = Regex.Match(value, @"/check/(\d+)/", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var id))
            {
                return id;
            }
            return 0;
        }

        public static Dictionary<string, bool> GetChalleng(HtmlDocument doc)
        {
            try
            {
                // Select all buttons with class 'challenge_btn'
                var buttons = doc.DocumentNode.SelectNodes("//button[contains(@class, 'challenge_btn')]");
                var results = new Dictionary<string, bool>();
                if (buttons != null)
                {
                    foreach (var btn in buttons)
                    {
                        // Get button value attribute
                        string link = btn.GetAttributeValue("value", string.Empty);

                        // Find span with class badge bg-light text-dark inside this button
                        var badge = btn.SelectSingleNode(".//span[contains(@class, 'badge') and contains(@class,'bg-light')]");

                        string badgeText = badge?.InnerText!.Trim()!;
                        var parts = badgeText.Split('/');

                        if (parts.Length == 2 &&
                            int.TryParse(parts[0].Trim(), out int left) &&
                            int.TryParse(parts[1].Trim(), out int right))
                        {
                            bool status = left >= right;

                            // Use the link as key, if empty fallback to "disabled"
                            results[string.IsNullOrWhiteSpace(link) ? "disabled" : link] = status;
                        }
                    }
                }
                return results;


            }
            catch (Exception)
            {
                return new Dictionary<string, bool>();
            }
        }

    }
}
