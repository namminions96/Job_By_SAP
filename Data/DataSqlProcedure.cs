using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePosVoucher.Data
{
    public class DataSqlProcedure
    {
        private readonly ILogger _logger;
        public DataSqlProcedure(ILogger logger)
        {
            _logger = logger;
        }
        public void Insert_TK_CarStockBalance()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                string connectionString = configuration["DbStaging_Inventory"];
                _logger.Information("Run: SP_INSERT_CARSTOCKBALANCE_TK");
                    //var result = db.Messages.FromSqlRaw("Exec SP_INSERT_SALE_PRICE_ONLINE").ToList();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        var timeout = 300;
                        // Thực hiện truy vấn sử dụng Dapper
                        var results = connection.Query("SP_INSERT_CARSTOCKBALANCE_TK", commandType: CommandType.StoredProcedure, commandTimeout: timeout);

                        _logger.Information("Run: SP_INSERT_CARSTOCKBALANCE_TK Data: OK");
                    }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi Exec Procedures ");
            }
        }
        public void GetconfigMail()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                string connectionString = configuration["DbStaging_Inventory"];
                _logger.Information("Get: SP_Config_Mail");
                //var result = db.Messages.FromSqlRaw("Exec SP_INSERT_SALE_PRICE_ONLINE").ToList();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var timeout = 300;
                    // Thực hiện truy vấn sử dụng Dapper
                    var results = connection.Query("SP_Config_Mail", commandType: CommandType.StoredProcedure, commandTimeout: timeout);

                    _logger.Information("Get: SP_Config_Mail Data: OK");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi Exec Procedures ");
            }
        }
    }
}
