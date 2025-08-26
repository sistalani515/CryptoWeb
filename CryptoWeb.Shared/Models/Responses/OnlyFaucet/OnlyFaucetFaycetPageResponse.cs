using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Responses.OnlyFaucet
{
    public class OnlyFaucetFaycetPageResponse
    {
        public bool IsValid { get; set; }
        public string? CSRF { get; set; }
        public string? Token { get; set; }
        public string? JSToken { get; set; }
        public string? Nonce { get; set; }
        public string? ImageLink { get; set; }
    }
}
