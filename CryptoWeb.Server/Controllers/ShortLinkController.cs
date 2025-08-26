using CryptoWeb.Shared.Services.Interfaces.ShortLink;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    public class ShortLinkController : BaseController
    {
        private readonly IFaucetShortLinkService faucetShortLinkService;

        public ShortLinkController(IFaucetShortLinkService faucetShortLinkService)
        {
            this.faucetShortLinkService = faucetShortLinkService;
        }

        [HttpGet]
        public async Task<IActionResult> SolveSL([FromQuery] string path, string type = "btshort")
        {
            try
            {
                var r = await faucetShortLinkService.SolveSL(type, path);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> OpenUrl([FromQuery] string url, string cookie = "Non")
        {
            try
            {
                if (cookie == "Non") cookie = "";
                var r = await faucetShortLinkService.OpenURL(url, cookie);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
