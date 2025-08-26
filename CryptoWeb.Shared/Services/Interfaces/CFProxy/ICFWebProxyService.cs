using CryptoWeb.Shared.Services.Implements.CFProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces.CFProxy
{
    public interface ICFWebProxyService
    {
        Task<HttpResponseMessage> SendAsync(
       string targetUrl,
       HttpMethod method,
       object body = null!,
       string bodyType = "",
       Dictionary<string, string> headers = null!,
       bool followRedirects = true);
    }
}
