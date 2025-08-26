using CryptoWeb.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces
{
    public interface IFaucetCurrencyService
    {
        Task<IEnumerable<FaucetListCurrency>> GetAllHostCurrencyByHost(string hostName);
        Task<FaucetListCurrency> GetHostCurrencyByName(string hostName, string name);
        Task<int> InsertHostCurrency(FaucetListCurrency faucetListCurrency);
        Task<int> UpdateHostCurrency(FaucetListCurrency faucetListCurrency);
        Task<IEnumerable<FaucetUserCurrency>> GetAllUserCurrency(string hostName, string user);
        Task<FaucetUserCurrency> GetUserCurrencyByName(string hostName, string user, string currency);
        Task<int> InsertUserCurrency(FaucetUserCurrency currency);
        Task<int> UpdateUserCurrency(FaucetUserCurrency currency);
    }
}
