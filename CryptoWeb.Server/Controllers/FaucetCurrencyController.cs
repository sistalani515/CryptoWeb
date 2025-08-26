using CryptoWeb.Shared.Models.Entities;
using CryptoWeb.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWeb.Server.Controllers
{
    public class FaucetCurrencyController : BaseController
    {
        private readonly IFaucetCurrencyService faucetCurrencyService;

        public FaucetCurrencyController(IFaucetCurrencyService faucetCurrencyService)
        {
            this.faucetCurrencyService = faucetCurrencyService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(string hostName)
        {
            try
            {
                var r = await faucetCurrencyService.GetAllHostCurrencyByHost(hostName);
                return Ok(r);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Insert([FromQuery] string hostName, string name)
        {
            try
            {
                var faucetListCurrency= new FaucetListCurrency { HostName = hostName, Name = name };
                var r = await faucetCurrencyService.InsertHostCurrency(faucetListCurrency);
                return Ok(r);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<IActionResult> Update(FaucetListCurrency faucetListCurrency)
        {
            try
            {
                var r = await faucetCurrencyService.UpdateHostCurrency(faucetListCurrency);
                return Ok(r);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
