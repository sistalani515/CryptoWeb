using CryptoWeb.Shared.Models.Entities;
using CryptoWeb.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    public class FaucetHostController : BaseController
    {
        private readonly IFaucetHostService faucetHostService;

        public FaucetHostController(IFaucetHostService faucetHostService)
        {
            this.faucetHostService = faucetHostService;
        }

        [HttpGet]
        public  async Task<IActionResult> GetAll()
        {
            try
            {
                var r = await faucetHostService.GetAll();
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByHostName([FromQuery] string hostName)
        {
            try
            {
                var r = await faucetHostService.GetByName(hostName);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] FaucetHost faucetHost)
        {
            try
            {
                var r = await faucetHostService.Insert(faucetHost);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FaucetHost faucetHost)
        {
            try
            {
                var r = await faucetHostService.Update(faucetHost);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        public async Task<IActionResult> ToggleStatus([FromQuery] string hostName = "all", bool hold = false)
        {
            try
            {
                if (hostName == "all") hostName = "";
                var r = await faucetHostService.ToggleHold(hold, hostName);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
