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
    public class FaucetHostService : IFaucetHostService
    {
        private readonly IDbConnection dbConnection;
        public FaucetHostService(AppDbContext appDbContext)
        {
            dbConnection = appDbContext.Database.GetDbConnection();
        }


        public async Task<IEnumerable<FaucetHost>> GetAll()
        {
            try
            {
                return await SQLQueryHelper.SelectAllAsync<FaucetHost>(dbConnection);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<FaucetHost> GetByName(string hostName)
        {
            try
            {
                return await SQLQueryHelper.SelectOneByAsync<FaucetHost>(dbConnection, new Dictionary<string, object>
                {
                    { "HostName", hostName }
                });
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> Insert(FaucetHost faucetHost)
        {
            try
            {
                var r = await GetByName(faucetHost.HostName!);
                if (r != null) return 0;
                return await SQLQueryHelper.InsertAsync(dbConnection, faucetHost);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<int> Update(FaucetHost faucetHost)
        {
            try
            {
                var r = await GetByName(faucetHost.HostName!);
                if (r == null) return 0;
                r.Delay = faucetHost.Delay;
                r.MaxDayClaim = faucetHost.MaxDayClaim;
                r.IsActive = faucetHost.IsActive;
                r.MaxThread = faucetHost.MaxThread;
                r.Changed = DateTime.UtcNow;
                r.IsHold = faucetHost.IsHold;
                return await SQLQueryHelper.UpdateAsync(dbConnection, r, p => p.HostName!, [
                    p => p.Delay,
                    p => p.MaxDayClaim,
                    p => p.IsActive,
                    p => p.MaxThread,
                    p => p.Changed!,
                    p => p.IsHold!,
                    ]);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> ToggleHold(bool hold = false, string name = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    var hosts = await GetAll();
                    foreach(var host in hosts)
                    {
                        host.IsHold = hold;
                        await Update(host);
                    }
                    return 1;
                }
                else
                {
                    var host = await GetByName(name);
                    if(host == null) return 0;
                    host.IsHold = hold;
                    await Update(host);
                    return 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
