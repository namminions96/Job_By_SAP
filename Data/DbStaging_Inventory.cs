using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BluePosVoucher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePosVoucher.Data
{
    public class DbStaging_Inventory : DbContext
    {
        public DbSet<CARStockBalance> CARStockBalances { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string connectString = configuration["DbStaging_Inventory"];
            optionsBuilder.UseSqlServer(connectString);
        }
        
    }
}
