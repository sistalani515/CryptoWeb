using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Responses.IconCaptcha
{
    public class IconCaptchaImageResponse
    {
        [JsonProperty("widgetId")]
        public string? WidgetId { get; set; }

        [JsonProperty("identifier")]
        public string? Identifier { get; set; }
        [JsonProperty("challenge")]
        public string? Challenge { get; set; }
        [JsonProperty("expiredAt")]
        public long ExpiredAt { get; set; }
        [JsonProperty("timestamp")]
        public long IntTimestamp { get; set; }
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
