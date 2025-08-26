using CryptoWeb.Shared.Services.Interfaces.FreeLTCFun;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    public class FaucetFreeLTCFunController : BaseController
    {
        private readonly IFaucetFreeLTCFunService faucetFreeLTCFunService;
        private readonly ILogger<FaucetFreeLTCFunController> logger;

        public FaucetFreeLTCFunController(IFaucetFreeLTCFunService faucetFreeLTCFunService,
            ILogger<FaucetFreeLTCFunController> logger
            )
        {
            this.faucetFreeLTCFunService = faucetFreeLTCFunService;
            this.logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetFaucetPage([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string currencyName = "ltc")
        {
            try
            {
                var r = await faucetFreeLTCFunService.GetFaucetPage(hostName, email, currencyName);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetLoginPage([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var r = await faucetFreeLTCFunService.GetLoginPage(hostName, email);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCaptcha([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string xIconCapthca = "")
        {
            try
            {
                var r = await faucetFreeLTCFunService.GetIconCaptcha(hostName, email, xIconCapthca);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> SolveCaptcha([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string xIconCapthca = "")
        {
            try
            {
                var r = await faucetFreeLTCFunService.SolveIcon(hostName, email, xIconCapthca);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> DoLogin([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var r = await faucetFreeLTCFunService.DoLogin(hostName, email);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> BatchDoLogin([FromQuery] string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var r = await faucetFreeLTCFunService.DoBacthLogin(email);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DoClaim([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string currencyName = "ltc")
        {
            try
            {
                var r = await faucetFreeLTCFunService.DoClaim(hostName, email, currencyName);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSLPage([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string currencyName = "ltc")
        {
            try
            {
                var r = await faucetFreeLTCFunService.GetSLPage(hostName, email, currencyName);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetLinkSL([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string link = "ltc")
        {
            try
            {
                var r = await faucetFreeLTCFunService.GetLinkSL(hostName, email, link);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> SolveSL([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com", string currency = "ltc")
        {
            try
            {
                var r = await faucetFreeLTCFunService.SolveSL(hostName, email, currency);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChallenges([FromQuery] string hostName = "freeltc", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var r = await faucetFreeLTCFunService.GetChallenges(hostName, email);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> SolveChallenge([FromQuery] string currency, string hostName = "freeltc", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var (cx,ch) = await faucetFreeLTCFunService.GetChallenges(hostName, email);
                foreach(var c in ch)
                {
                    var r = await faucetFreeLTCFunService.SolveChallenge(hostName, email,cx, c.Key, currency);
                    return Ok(r);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> RawSolveChallenge([FromQuery] string currency, string url, string csrf, string icon,  string hostName = "freeltc", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var cx = await faucetFreeLTCFunService.RawSolveChallenge(hostName, email, url, currency, csrf, icon);
                return Ok(cx);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

