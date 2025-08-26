using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Responses.FreeLTCFun
{
    public class FreeLTCFunFaucetRequest
    {
        public bool IsValid { get; set; }
        public bool IsLimit { get; set; }
        public string? CSRF {  get; set; }
        public string? CaptchaToken { get; set; }
        public string? WebToken { get; set; }
    }
}
