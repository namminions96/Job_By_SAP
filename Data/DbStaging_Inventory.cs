using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BluePosVoucher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Job_By_SAP.Models;

namespace Job_By_SAP.Data
{
    public class DbStaging_Inventory : DbContext
    {
        public DbSet<CARStockBalance> CARStockBalances { get; set; }
        public DbSet<MailConfig> mailConfigs { get; set; }
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
