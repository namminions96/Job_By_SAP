using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace BluePosVoucher
{
    public class InbVoucherSap
    {
        private readonly ILogger _logger;
        public InbVoucherSap(ILogger logger)
        {
            _logger = logger;
        }
        IConfiguration configuration = new ConfigurationBuilder()
         .SetBasePath(AppContext.BaseDirectory)
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
         .Build();
        public async Task<string>  CallApiSAPCreate(string VoucherNumber, double? Value, string From_Date, string Expiry_Date, string SiteCode, string BonusBuy, string Article_No, string POSTerminal)
        {
            string apiUrl = configuration["ApiCreateVoucherCreate"];
            var dataArray = new[]
            {
              new
               {
                VoucherNumber,
                Value,
                From_Date,
                Expiry_Date,
                SiteCode,
                BonusBuy,
                Article_No,
                POSTerminal
               }

            };
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(dataArray);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    _logger.Information("SeriaNo: " + VoucherNumber+" " + await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode)
                    {
                        return "200";
                    }
                    else
                    {
                        return "400";
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error:", ex);
                    return null;
                }
            }
        }
        public  async Task<string> CallApiSAPUpdate(string CompanyCode, string VoucherNumber, string ArticleNo, string ArticleType, string Status, string SiteCode, string POSTerminal)
        {
            string apiUrl = configuration["ApiCreateVoucherUpdate"];
            var dataArray = new[]
                {
                    new
                    {
                    CompanyCode,
                    VoucherNumber,
                    ArticleNo,
                    ArticleType,
                    Status,
                    SiteCode,
                    POSTerminal
                    }
                  
                };
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(dataArray);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiUrl, content);
                    _logger.Information("SeriaNo: "+VoucherNumber+ " " + await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode)
                    {
                        return "200";
                    }
                    else
                    {
                        return "400";
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error:", ex);
                    return null;
                }

            }
           
        }

    }
}
