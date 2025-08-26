using CryptoWeb.Shared.Helper.HttpHelper;
using CryptoWeb.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CryptoWeb.Server.Controllers
{
    public class TestController : BaseController
    {
        private readonly IFaucetUserService faucetUserService;
        private readonly ILogger<TestController> logger;

        public TestController(IFaucetUserService faucetUserService, ILogger<TestController> logger)
        {
            this.faucetUserService = faucetUserService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> NormalHttp([FromQuery] string url)
        {
            try
            {
                var user = await faucetUserService.GetByName("earnsolanaxyz", "sakhaumarridho@gmail.com");
                var client = HttpHelper.GetHttpClient(user, "https://earnsolana.xyz");
                var respnse = await client.GetAsync(url);
                var response = await respnse.Content!.ReadAsStringAsync();
                return Ok(response);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> TlsHttp([FromQuery] string url, string cookie = "Non")
        {
            try
            {
                if(cookie == "Non") cookie = "";
                var user = await faucetUserService.GetByName("earnsolanaxyz", "sakhaumarridho@gmail.com");
                var client = await TlsHttpHelper.GetAsync(user, url, cookie: cookie);
                var headers = client.Headers;
                foreach(var header in headers!)
                {
                    foreach(var h in header.Value)
                    {
                        logger.LogInformation($"{header.Key}:{h}");

                    }
                }
                var response = client.Body;
                return Ok(response);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
