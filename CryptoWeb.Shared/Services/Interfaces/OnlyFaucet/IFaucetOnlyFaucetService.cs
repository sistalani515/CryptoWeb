using CryptoWeb.Shared.Helper.OnlyFaucet;
using CryptoWeb.Shared.Models.Responses.OnlyFaucet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces.OnlyFaucet
{
    public interface IFaucetOnlyFaucetService
    {
        Task<OnlyFaucetLoginPageResponse> GetLoginPage(string hostName, string email);
        Task<bool> Login(string hostName, string email);
        Task<OnlyFaucetFaycetPageResponse> GetFaucetPage(string hostName, string email, string currency, bool loop = false);
    }
}
