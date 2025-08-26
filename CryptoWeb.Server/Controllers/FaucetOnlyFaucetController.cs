using CryptoWeb.Shared.Services.Interfaces.OnlyFaucet;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    
    public class FaucetOnlyFaucetController : BaseController
    {
        private readonly IFaucetOnlyFaucetService faucetOnlyFaucetService;

        public FaucetOnlyFaucetController(IFaucetOnlyFaucetService faucetOnlyFaucetService)
        {
            this.faucetOnlyFaucetService = faucetOnlyFaucetService;
        }


        [HttpGet]
        public async Task<IActionResult> GetLoginPage(string hostName="onlyfaucetcom", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var res = await faucetOnlyFaucetService.GetLoginPage(hostName, email);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> DoLogin(string hostName = "onlyfaucetcom", string email = "sakhaumarridho@gmail.com")
        {
            try
            {
                var res = await faucetOnlyFaucetService.Login(hostName, email);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetFaucetPage(string hostName = "onlyfaucetcom", string email = "sakhaumarridho@gmail.com", string currency = "ltc")
        {
            try
            {
                var res = await faucetOnlyFaucetService.GetFaucetPage(hostName, email, currency);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
