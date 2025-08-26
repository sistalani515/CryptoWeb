using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Responses.OnlyFaucet
{
    public class OnlyFaucetLoginPageResponse
    {
        public bool IsLogged {  get; set; }
        public string? CSRFToken { get; set; }
    }
}
