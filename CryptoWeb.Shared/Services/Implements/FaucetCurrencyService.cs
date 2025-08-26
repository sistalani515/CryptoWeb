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
    public class FaucetCurrencyService : IFaucetCurrencyService
    {
        private readonly IDbConnection dbConnection;
        public FaucetCurrencyService(AppDbContext appDbContext)
        {
            dbConnection = appDbContext.Database.GetDbConnection();
        }

        public async Task<IEnumerable<FaucetListCurrency>> GetAllHostCurrencyByHost(string hostName)
        {
            try
            {
                return await SQLQueryHelper.SelectByAsync<FaucetListCurrency>(dbConnection, new Dictionary<string, object>
                {
                    {"HostName", hostName }
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<FaucetListCurrency> GetHostCurrencyByName(string hostName, string name)
        {
            try
            {
                return await SQLQueryHelper.SelectOneByAsync<FaucetListCurrency>(dbConnection, new Dictionary<string, object>
                {
                    {"HostName", hostName },
                    {"Name", name },
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> InsertHostCurrency(FaucetListCurrency faucetListCurrency)
        {
            try
            {
                var r = await GetHostCurrencyByName(faucetListCurrency.HostName!, faucetListCurrency.Name!);
                if (r != null) return 0;
                return await SQLQueryHelper.InsertAsync(dbConnection, faucetListCurrency);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> UpdateHostCurrency(FaucetListCurrency faucetListCurrency)
        {
            try
            {
                var r = await GetHostCurrencyByName(faucetListCurrency.HostName!, faucetListCurrency.Name!);
                if (r == null) return 0;
                r.IsActive = faucetListCurrency.IsActive;
                return await SQLQueryHelper.UpdateAsync2(dbConnection, faucetListCurrency, [
                    propa => propa.HostName!,
                    propa => propa.Name!,
                    ], [
                        propa => propa.IsActive
                        ]);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<FaucetUserCurrency>> GetAllUserCurrency(string hostName, string user)
        {
            try
            {
                return await SQLQueryHelper.SelectByAsync<FaucetUserCurrency>(dbConnection, new Dictionary<string, object>
                {
                    {"HostName", hostName },
                    {"Email", user },
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<FaucetUserCurrency> GetUserCurrencyByName(string hostName, string user, string currency)
        {
            try
            {
                return await SQLQueryHelper.SelectOneByAsync<FaucetUserCurrency>(dbConnection, new Dictionary<string, object>
                {
                    {"HostName", hostName },
                    {"Email", user },
                    {"Name", currency },
                    {"Date", DateTime.UtcNow.ToString("yyyyMMdd") },
                });
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> InsertUserCurrency(FaucetUserCurrency currency)
        {
            try
            {
                var r = await GetUserCurrencyByName(currency.HostName!, currency.Email!, currency.Name!);
                if (r != null) return 0;
                return await SQLQueryHelper.InsertAsync(dbConnection, currency);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<int> UpdateUserCurrency(FaucetUserCurrency currency)
        {
            try
            {
                var r = await GetUserCurrencyByName(currency.HostName!, currency.Email!, currency.Name!);
                if (r == null) return 0;
                r.TotalClaim = currency.TotalClaim;
                r.TotalAmount = currency.TotalAmount;
                r.IsActive = currency.IsActive;

                return await SQLQueryHelper.UpdateAsync2(dbConnection, currency,
                    [
                    propa => propa.HostName!,
                    propa => propa.Email!,
                    propa => propa.Name!,
                    propa => propa.Date!,
                    ], [
                        propa => propa.TotalAmount!,
                        propa => propa.TotalClaim,
                        propa => propa.IsActive,
                        propa => propa.Changed!
                        ]);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
