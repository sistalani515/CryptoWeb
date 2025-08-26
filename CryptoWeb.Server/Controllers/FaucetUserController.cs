using CryptoWeb.Shared.Models.Entities;
using CryptoWeb.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    public class FaucetUserController : BaseController
    {
        private readonly IFaucetUserService faucetUserService;

        public FaucetUserController(IFaucetUserService faucetUserService)
        {
            this.faucetUserService = faucetUserService;
        }



        [HttpGet]
        public async Task<IActionResult> GetAllByHost([FromQuery] string hostName)
        {
            try
            {
                var r = await faucetUserService.GetAllByHost(hostName);
                return Ok(r);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllByName([FromQuery] string hostName, string email)
        {
            try
            {
                var r = await faucetUserService.GetByName(hostName, email);
                return Ok(r);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] FaucetUser faucetUser)
        {
            try
            {
                var r = await faucetUserService.Insert(faucetUser);
                return Ok(r);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] FaucetUser faucetUser)
        {
            try
            {
                var r = await faucetUserService.Udpate(faucetUser);
                return Ok(r);
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
