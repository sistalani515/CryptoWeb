using CryptoWeb.Shared.Models.Responses.OnlyFaucet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Helper.OnlyFaucet
{
    public static class OnlyFaucetIconHelper
    {



        public static async Task<string> GetOddIconIndexFromImageUrlAsync(string imageUrl)
        {
            var _httpClient = new HttpClient();
            // Download image
            byte[] imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            string base64Image = Convert.ToBase64String(imageBytes);

            // Build payload using inlineData (embedded image)
            var requestBody = new
            {
                contents = new object[]
                {
                new
                {
                    parts = new object[]
                    {
                        new { text = "This image contains 5 icons in a row (left to right). Which one is visually different (odd one out)? Provide only the number index (1–5)." },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = "image/png",
                                data = base64Image
                            }
                        }
                    }
                }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-vision:generateContent?key=AIzaSyCSJ4u2G1zgWzrWJPVFTwsVx5wvdq-SsHc";
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var textResult = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textResult?.Trim();
        }
    }
}
