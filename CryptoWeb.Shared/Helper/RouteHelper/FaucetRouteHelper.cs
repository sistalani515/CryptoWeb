using CryptoWeb.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Helper.RouteHelper
{
    public class FaucetRouteHelper
    {
        public static string FaucetPage(string typeCurrency, FaucetHost host) => host.Type != 0 ? $"app/faucet?currency={typeCurrency.ToUpper()}" : $"faucet/currency/{typeCurrency}";
        public static string LoginPost(FaucetHost host) => host.Type != 0 ? "/app/auth/validation" : "auth/login";
        public static string Challenge(FaucetHost host) => host.Type != 0 ? "/app/auth/challenge" : "/challenge";
        public static string SLPage(FaucetHost host, string name) => host.Type != 0 ? $"/app/links/currency/{name}" : $"/links/currency/{name}";
        public static string SolveSL(FaucetHost host, string name, int id) => host.Type != 0 ? $"/app/links/go/{id}/{name.ToUpper()}" : $"/links/go/{id}/{name.ToUpper()}";
        public const string CaptchaRequest = "icaptcha/req";
        public static string ClaimFaucet(string typeCurrency, FaucetHost host) => host.Type != 0 ? $"app/faucet/verify?currency={typeCurrency.ToUpper()}" : $"faucet/verify/{typeCurrency}";

    }
}