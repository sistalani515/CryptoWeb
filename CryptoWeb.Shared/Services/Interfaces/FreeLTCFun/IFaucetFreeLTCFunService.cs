using CryptoWeb.Shared.Models.Responses.FreeLTCFun;
using CryptoWeb.Shared.Models.Responses.IconCaptcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces.FreeLTCFun
{
    public interface IFaucetFreeLTCFunService
    {
        Task<FreeLTCFunFaucetRequest> GetFaucetPage(string hostName, string email, string currencyName);
        Task<FreeLTCFunLoginPageResponse> GetLoginPage(string hostName, string email);
        Task<FreeLTCFunLoginPageResponse> DoLogin(string hostName, string email);
        Task<string> DoBacthLogin(string email);
        Task<IconCaptchaImageResponse> GetIconCaptcha(string hostName, string email, string xIconCaptcha);
        Task<IconCaptchaImageResultResponse> SolveIcon(string hostName, string email, string xIconCaptcha);
        Task<Dictionary<string, int>> GetSLPage(string hostName, string email, string currencyName);
        Task<FreeLTCFunFaucetRequest> GetLinkSL(string hostName, string email, string link);
        Task<bool> SolveSL(string hostName, string email, string currencyName);
        Task<string> DoClaim(string hostName, string email, string currencyName, bool force = false);
        Task<(FreeLTCFunFaucetRequest, Dictionary<string, bool>)> GetChallenges(string hostName, string email);
        Task<string> SolveChallenge(string hostName, string email, FreeLTCFunFaucetRequest faucetPage, string url, string name);
        Task<string> BatchSolveChallenge(string hostName, string email, string currency);
        Task<string> RawSolveChallenge(string hostName, string email, string url, string name, string csrf, string iconToken);
    }
}
