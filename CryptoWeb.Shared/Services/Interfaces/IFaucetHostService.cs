using CryptoWeb.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces
{
    public interface IFaucetHostService
    {
        Task<IEnumerable<FaucetHost>> GetAll();
        Task<FaucetHost> GetByName(string hostName);
        Task<int> Insert(FaucetHost faucetHost);
        Task<int> Update(FaucetHost faucetHost);
        Task<int> ToggleHold(bool hold = false, string name = "");
    }
}
