using CryptoWeb.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoWeb.Shared.Models.Databases
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions){}
        public virtual DbSet<FaucetUser> FaucetUsers { get; set; }
        public virtual DbSet<FaucetHost> FaucetHosts { get; set; }
        public virtual DbSet<FaucetListCurrency> FaucetListCurrencies { get; set; }
        public virtual DbSet<FaucetUserCurrency> FaucetUserCurrencies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FaucetUser>().HasKey(e => new { e.Email, e.Host });
            modelBuilder.Entity<FaucetListCurrency>().HasKey(e => new { e.HostName, e.Name });
            modelBuilder.Entity<FaucetUserCurrency>().HasKey(e => new { e.HostName, e.Email, e.Name, e.Date });
            base.OnModelCreating(modelBuilder);
        }
    }
}
