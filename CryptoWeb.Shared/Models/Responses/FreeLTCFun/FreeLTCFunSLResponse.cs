using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Responses.FreeLTCFun
{
    public class FreeLTCFunSLResponse
    {
        [JsonProperty("status")]
        public string? Status { get; set; }
        [JsonProperty("message")]
        public string? Message { get; set; }
        [JsonProperty("url")]
        public string? Url { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
