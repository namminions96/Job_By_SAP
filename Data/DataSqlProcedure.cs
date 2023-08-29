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

        public List<string> DataStoreXml()
        {
            try
            {

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                string connectionString = configuration["DbStaging_Inventory"];
                _logger.Information("Get: DataStoreXml");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var timeout = 300;
                    string query = "SELECT Site  FROM [Inventory].[dbo].[CARStockBalances] where Status='0' group by Site";
                    List<string> siteList = connection.Query<string>(query).AsList();
                    return siteList;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi Select Data ");
                return null;
            }
        }
        public string ConvertSQLtoXML( string query)
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string connectionString = configuration["DbStaging_Inventory"];

            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<ns0:MT_VINID_Stock_Change_In xmlns:ns0=\"urn:Vincommerce:SAPBW:To:VinID:StockChange\">");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            xml.AppendLine("<StockChange>");

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string fieldName = reader.GetName(i);
                                string fieldValue = reader[i].ToString();

                                // Chuyển đổi các dấu phẩy thành dấu chấm trong các trường số
                                if (fieldName.EndsWith("Qty"))
                                {
                                    fieldValue = fieldValue.Replace(",", ".");
                                }

                                xml.AppendLine($"<{fieldName}>{fieldValue}</{fieldName}>");
                            }

                            xml.AppendLine("</StockChange>");
                        }
                    }
                }
            }

            xml.AppendLine("</Data>");

            return xml.ToString();
        }
    }
}
