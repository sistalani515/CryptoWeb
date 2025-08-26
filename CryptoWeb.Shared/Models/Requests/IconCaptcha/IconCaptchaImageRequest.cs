using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Requests.IconCaptcha
{
    public class IconCaptchaImageRequest
    {
        [JsonProperty("widgetId")]
        public string? WidgetId { get; set; }
        [JsonProperty("action")]
        public string? Action { get; set; } = "LOAD";
        [JsonProperty("theme")]
        public string? Theme { get; set; } = "light";
        [JsonProperty("token")]
        public string? Token { get; set; }
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("initTimestamp")]
        public long IntTimeStamp { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
