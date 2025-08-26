using CryptoWeb.Shared.Services.Interfaces;
using CryptoWeb.Shared.Services.Interfaces.FreeLTCFun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenCvSharp.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Workers.FreeLTCFunWorker
{
    public class FreeLTCFunClaimWorker : BackgroundService
    {
        private readonly ILogger<FreeLTCFunClaimWorker> logger;

        public FreeLTCFunClaimWorker(IServiceProvider serviceProvider, ILogger<FreeLTCFunClaimWorker> logger)
        {
            ServiceProvider = serviceProvider;
            this.logger = logger;
        }
        public IServiceProvider ServiceProvider { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var scope = ServiceProvider.CreateScope();
                var freeLTCFunService = scope.ServiceProvider.GetRequiredService<IFaucetFreeLTCFunService>();
                var faucetHostService = scope.ServiceProvider.GetRequiredService<IFaucetHostService>();
                var faucetUserService = scope.ServiceProvider.GetRequiredService<IFaucetUserService>();
                var faucetCurrencyService = scope.ServiceProvider.GetRequiredService<IFaucetCurrencyService>();
                logger.LogInformation($"Start FreeLTCFunClaimWorker");
                await Task.Delay(1000, stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    var hosts = await faucetHostService.GetAll();
                    foreach ( var host in hosts.Where(e => e.IsActive && !e.IsHold))
                    {
                        var users = await faucetUserService.GetAllByHost(host.HostName!);
                        foreach(var user in users.Where(e => e.Host == host.HostName! && e.IsActive && !e.IsLocked && !e.IsDeleted && (!e.IsSL || !e.SLCompleted.HasValue || (DateTime.UtcNow - e.SLCompleted.Value).TotalDays >=1)))
                        {
                            var currencies = await faucetCurrencyService.GetAllHostCurrencyByHost(host.HostName!);
                            int count = 0;
                            bool slfull = false;
                            foreach (var currency in currencies.Where(e => e.IsActive))
                            {
                                count++;
                                if (slfull) continue;
                                try
                                {

                                    //if (user.IsSL && (!user.SLCompleted.HasValue || (DateTime.UtcNow - user.SLCompleted.Value).TotalDays < 1))
                                    //{
                                    //    try
                                    //    {
                                    //        var slSolve = await freeLTCFunService.SolveSL(host.HostName!, user.Email!, currency.Name!);
                                    //        if (!slSolve)
                                    //        {
                                    //            slfull = true;
                                    //            break;
                                    //        }
                                    //        user.IsSL = false;
                                    //        await faucetUserService.Udpate(user);
                                    //        var userCurrencyx = await faucetCurrencyService.GetUserCurrencyByName(host.HostName!, user.Email!, currency.Name!);
                                    //        if (userCurrencyx != null)
                                    //        {
                                    //            userCurrencyx.IsActive = true;
                                    //            await faucetCurrencyService.UpdateUserCurrency(userCurrencyx);
                                    //        }
                                    //    }
                                    //    catch (Exception)
                                    //    {
                                    //        continue;
                                    //    }
                                    //}

                                    var userCurrency = await faucetCurrencyService.GetUserCurrencyByName(host.HostName!, user.Email!, currency.Name!);
                                    if(userCurrency != null && (!userCurrency.IsActive 
                                        && (userCurrency.Changed.HasValue && (DateTime.UtcNow - userCurrency.Changed.Value).TotalHours < 2)))
                                    {
                                        await Task.Delay(1000, stoppingToken);
                                        continue;
                                    }
                                    var claim = await freeLTCFunService.DoClaim(host.HostName!, user.Email!, currency.Name!, true);
                                    if (string.IsNullOrWhiteSpace(claim)) throw new Exception("Claim response null");
                                    logger.LogInformation($"{host.HostName}|{user.Email!}|{currency.Name!}=>{claim}");
                                    if (claim.ToLower().Contains("shortlink")) user.IsSL = true;
                                    
                                }
                                catch (Exception ex)
                                {
                                    logger.LogInformation($"{host.HostName}|{user.Email!}|{currency.Name!}=>{ex.Message}");
                                }
                                if (user.IsSL)
                                {
                                    try
                                    {
                                        var slSolve = await freeLTCFunService.SolveSL(host.HostName!, user.Email!, currency.Name!);
                                        if (!slSolve) continue;
                                        user.IsSL = false;
                                        await faucetUserService.Udpate(user);
                                        var userCurrency = await faucetCurrencyService.GetUserCurrencyByName(host.HostName!, user.Email!, currency.Name!);
                                        if(userCurrency != null)
                                        {
                                            userCurrency.IsActive = true;
                                            await faucetCurrencyService.UpdateUserCurrency(userCurrency);
                                        }
                                        try
                                        {
                                            await freeLTCFunService.BatchSolveChallenge(host.HostName!, user.Email!, currency.Name!);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }
                                await Task.Delay(1000, stoppingToken);
                            }
                            if(user.IsSL && count == currencies.Count())
                            {
                                user.SLCompleted = DateTime.UtcNow;
                                await faucetUserService.Udpate(user);
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
