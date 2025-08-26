using CryptoWeb.Shared.Models.Responses.FreeLTCFun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces.ShortLink
{
    public interface IFaucetShortLinkService
    {
        Task<HttpResponseMessage> OpenURL(string url, string setCookie);
        Task<FreeLTCFunSLResponse> SolveSL(string type, string path);
    }
}
