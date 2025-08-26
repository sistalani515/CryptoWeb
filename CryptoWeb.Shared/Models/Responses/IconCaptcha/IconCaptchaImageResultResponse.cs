using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Responses.IconCaptcha
{
    public class IconCaptchaImageResultResponse
    {
        [JsonProperty("identifier")]
        public string? Identifier { get; set; }
        [JsonProperty("completed")]
        public bool Completed { get; set; }
        [JsonProperty("expiredAt")]
        public long ExpiredAt { get; set; }
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        public IconCaptchaImageResponse Coordinates { get; set; } = new IconCaptchaImageResponse();
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
