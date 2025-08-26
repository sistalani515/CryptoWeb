using CryptoWeb.Shared.Helper.SQLHelper;
using CryptoWeb.Shared.Models.Databases;
using CryptoWeb.Shared.Models.Entities;
using CryptoWeb.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Services.Implements
{
    public class FaucetUserService : IFaucetUserService
    {
        private readonly IDbConnection dbConnection;
        public FaucetUserService(AppDbContext appDbContext)
        {
            dbConnection = appDbContext.Database.GetDbConnection();
        }
        public async Task<IEnumerable<FaucetUser>> GetAllByHost(string hostName)
        {
            try
            {
                return await SQLQueryHelper.SelectByAsync<FaucetUser>(dbConnection, new Dictionary<string, object>
                {
                    {"Host", hostName},
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<FaucetUser> GetByName(string hostName, string email)
        {
            try
            {
                return await SQLQueryHelper.SelectOneByAsync<FaucetUser>(dbConnection, new Dictionary<string, object>
                {
                    {"Host", hostName},
                    {"Email", email},
                });
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> Insert(FaucetUser faucetUser)
        {
            try
            {
                var r = await GetByName(faucetUser.Host!, faucetUser.Email!);
                if (r != null) return 0;
                return await SQLQueryHelper.InsertAsync(dbConnection, faucetUser);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> Udpate(FaucetUser faucetUser)
        {
            try
            {
                var r = await GetByName(faucetUser.Host!, faucetUser.Email!);
                if (r == null) return 0;

                r.Session = faucetUser.Session;
                r.IsLocked = faucetUser.IsLocked;
                r.Password = faucetUser.Password;
                r.IsDeleted = faucetUser.IsDeleted;
                r.IsActive = faucetUser.IsActive;
                r.LastClaim = faucetUser.LastClaim;
                r.LastLogin = faucetUser.LastLogin;
                r.TodayClaim = faucetUser.TodayClaim;
                r.TotalClaim = faucetUser.TotalClaim;
                r.IsSL = faucetUser.IsSL;
                r.SLCompleted = faucetUser.SLCompleted;
                r.LastClaim = faucetUser.LastClaim;
                return await SQLQueryHelper.UpdateAsync2(dbConnection, r, [ p=> p.Host!, p => p.Email!], 
                    [
                    p=> p.Session!,
                    p=> p.IsLocked,
                    p=> p.Password!,
                    p=> p.IsDeleted,
                    p=> p.IsActive,
                    p=> p.LastClaim!,
                    p=> p.LastLogin!,
                    p=> p.TotalClaim!,
                    p=> p.TodayClaim!,
                    p=> p.IsSL!,
                    p=> p.SLCompleted!,
                    p=> p.LastClaim!,
                    ]);
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
