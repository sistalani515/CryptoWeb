using CryptoWeb.Shared.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Interfaces
{
    public interface IFaucetUserService
    {
        Task<IEnumerable<FaucetUser>> GetAllByHost(string hostName);
        Task<FaucetUser> GetByName(string hostName, string email);
        Task<int> Insert(FaucetUser faucetUser);
        Task<int> Udpate(FaucetUser faucetUser);
    }
}
