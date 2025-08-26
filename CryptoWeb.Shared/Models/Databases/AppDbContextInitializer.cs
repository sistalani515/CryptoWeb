using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Databases
{
    public class AppDbContextInitializer
    {
        private readonly IServiceProvider serviceProvider;

        public AppDbContextInitializer(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();//<AppDbContext>();
                var dbSrv = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbSrv.Database.Migrate();
            }
            catch (Exception)
            {

            }
        }
    }
}
