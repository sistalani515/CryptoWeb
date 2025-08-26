using CryptoWeb.Shared.Helper.CFProxy;
using CryptoWeb.Shared.Models.Databases;
using CryptoWeb.Shared.Services.Implements;
using CryptoWeb.Shared.Services.Implements.CFProxy;
using CryptoWeb.Shared.Services.Implements.FreeLTCFun;
using CryptoWeb.Shared.Services.Implements.OnlyFaucet;
using CryptoWeb.Shared.Services.Implements.ShortLink;
using CryptoWeb.Shared.Services.Interfaces;
using CryptoWeb.Shared.Services.Interfaces.CFProxy;
using CryptoWeb.Shared.Services.Interfaces.FreeLTCFun;
using CryptoWeb.Shared.Services.Interfaces.OnlyFaucet;
using CryptoWeb.Shared.Services.Interfaces.ShortLink;
using CryptoWeb.Shared.Workers.FreeLTCFunWorker;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.DI
{
    public static class DIHelper
    {
        public static IServiceCollection InjectServer(this IServiceCollection services, IConfiguration configuration, string assemblyName)
        {
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(configuration.GetConnectionString("SQLLite"), n => n.MigrationsAssembly(assemblyName)));
            return services;
        }
        public static IServiceCollection InjectFaucet(this IServiceCollection services)
        {
            services.AddScoped<IFaucetHostService, FaucetHostService>();
            services.AddScoped<IFaucetUserService, FaucetUserService>();
            services.AddScoped<IFaucetFreeLTCFunService, FaucetFreeLTCFunService>();
            services.AddScoped<IFaucetCurrencyService, FaucetCurrencyService>();
            services.AddScoped<IFaucetShortLinkService, FaucetShortLinkService>();
            services.AddScoped<IFaucetOnlyFaucetService, FaucetOnlyFaucetService>();
            services.AddHostedService<FreeLTCFunClaimWorker>();
            return services;
        }

        public static IServiceCollection AddCFProxy(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CFProxyConfig>(configuration.GetSection("CFProxyConfig"));
            services.AddHttpClient<ICFWebProxyService, CFWebProxyService>();
            services.AddScoped<IFaucetFreeLTCFunService2, FaucetFreeLTCFunService2>();

            return services;
        }


        public static IServiceCollection SetSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen().AddSwaggerGenNewtonsoftSupport();
            return services;
        }

        public static WebApplication SetUseSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            return app;
        }

    }
}
