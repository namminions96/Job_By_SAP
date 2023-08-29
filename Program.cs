using BluePosVoucher;
using BluePosVoucher.Data;
using BluePosVoucher.Models;
using Dapper;
using Job_By_SAP;
using Job_By_SAP.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Read_xml;
using Serilog;
using System;
using System.Data;
using System.Text;
using System.Xml.Linq;

internal class Program
{
    private static IConfiguration _configuration;
    private static ILogger _logger;
    private static async Task Main(string[] args)
    {
        _logger = SerilogLogger.GetLogger();
        InbVoucherSap inbVoucherSap1 = new InbVoucherSap(_logger);
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
// --------------------------------Voucher SAP---------------------------------------------------------- -
        var connectionStringsSection = configuration.GetSection("ConnectionStrings");
        var connectionStrings = connectionStringsSection.GetChildren();
        try
        {
            foreach (var connectionString in connectionStrings)
            {
                _logger.Information("Connect DB : " + connectionString.Key);
                using (SqlConnection connection = new SqlConnection(connectionString.Value))
                {
                    connection.Open();
                    var timeout = 300;
                    var parameters = new
                    {
                        Status = "",
                        Date = "",
                        Retry = 0
                    };
                    var results = connection.Query("INB_Voucher ", parameters, commandType: CommandType.StoredProcedure, commandTimeout: timeout).ToList();
                    Inb_Voucher inb_Voucher = new Inb_Voucher();
                    foreach (var result in results)
                    {
                        inb_Voucher.Voucher_Type = result.Voucher_Type;
                        inb_Voucher.SerialNo = result.SerialNo;
                        inb_Voucher.Voucher_Value = result.Voucher_Value;
                        inb_Voucher.Voucher_Currency = result.Voucher_Currency;
                        inb_Voucher.Validity_From_Date = result.Validity_From_Date;
                        inb_Voucher.Expiry_Date = result.Expiry_Date;
                        inb_Voucher.Processing_Type = result.Processing_Type;
                        inb_Voucher.Status = result.Status;
                        inb_Voucher.Site = result.Site;
                        inb_Voucher.Article_No = result.Article_No;
                        inb_Voucher.Bonus_Buy = result.Bonus_Buy;
                        inb_Voucher.POSNo = result.POSNo;
                        inb_Voucher.ReceiptNo = result.ReceiptNo;
                        inb_Voucher.TranDate = result.TranDate;
                        inb_Voucher.TranTime = result.TranTime;
                        string POSTerminal = inb_Voucher.ReceiptNo.Substring(0, 6);
                        if (inb_Voucher.Status == "EXP")
                        {
                            var calResult = await inbVoucherSap1.CallApiSAPUpdate("VCM", inb_Voucher.SerialNo, inb_Voucher.Article_No, "ZVCN", inb_Voucher.Status, inb_Voucher.Site, POSTerminal);
                            if (calResult != null)
                            {
                                if (calResult == "200")
                                {
                                    INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP()
                                    {
                                        Voucher_Type = inb_Voucher.Voucher_Type,
                                        SerialNo = inb_Voucher.SerialNo,
                                        Voucher_Value = inb_Voucher.Voucher_Value,
                                        Voucher_Currency = inb_Voucher.Voucher_Currency,
                                        Validity_From_Date = inb_Voucher.Validity_From_Date,
                                        Expiry_Date = inb_Voucher.Expiry_Date,
                                        Processing_Type = inb_Voucher.Processing_Type,
                                        Status = inb_Voucher.Status,
                                        Site = inb_Voucher.Site,
                                        Article_No = inb_Voucher.Article_No,
                                        Bonus_Buy = inb_Voucher.Bonus_Buy,
                                        POSNo = inb_Voucher.POSNo,
                                        ReceiptNo = inb_Voucher.ReceiptNo,
                                        TranDate = inb_Voucher.TranDate,
                                        TranTime = inb_Voucher.TranTime,
                                        FileName = inb_Voucher.SerialNo
                                    };
                                    string insertSql = @"INSERT INTO INB_VoucherToSAP 
                                        (Voucher_Type, SerialNo, Voucher_Value, Voucher_Currency, Validity_From_Date, Expiry_Date, 
                                        Processing_Type, Status, Site, Article_No, Bonus_Buy, POSNo, ReceiptNo, TranDate, TranTime, FileName)
                                        VALUES
                                        (@Voucher_Type, @SerialNo, @Voucher_Value, @Voucher_Currency, @Validity_From_Date, @Expiry_Date,
                                        @Processing_Type, @Status, @Site, @Article_No, @Bonus_Buy, @POSNo, @ReceiptNo, @TranDate, @TranTime, @FileName)";
                                    int rowsAffected = connection.Execute(insertSql, inbVoucherSap);
                                    if (rowsAffected > 0)
                                    {
                                        _logger.Information("Data inserted successfully!" + rowsAffected + " Row");
                                    }
                                    string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                    int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inbVoucherSap.SerialNo });

                                    if (rowsAffectedupdate > 0)
                                    {
                                        _logger.Information("Data Updated successfully!" + rowsAffectedupdate + " Row");
                                    }
                                }
                                else
                                {
                                    _logger.Information("Lỗi : " + calResult);
                                }
                            }
                            else
                            {
                                _logger.Information("Không có Data ");
                            }
                        }
                        if (inb_Voucher.Status == "SOLD")
                        {
                            var calResult = await inbVoucherSap1.CallApiSAPCreate(inb_Voucher.SerialNo, inb_Voucher.Voucher_Value,
                                             inb_Voucher.Validity_From_Date, inb_Voucher.Expiry_Date, inb_Voucher.Site,
                                             inb_Voucher.Bonus_Buy, inb_Voucher.Article_No, POSTerminal);
                            if (calResult != null)
                            {
                                if (calResult == "200")
                                {
                                    INB_VoucherToSAP inbVoucherSap = new INB_VoucherToSAP()
                                    {
                                        Voucher_Type = inb_Voucher.Voucher_Type,
                                        SerialNo = inb_Voucher.SerialNo,
                                        Voucher_Value = inb_Voucher.Voucher_Value,
                                        Voucher_Currency = inb_Voucher.Voucher_Currency,
                                        Validity_From_Date = inb_Voucher.Validity_From_Date,
                                        Expiry_Date = inb_Voucher.Expiry_Date,
                                        Processing_Type = inb_Voucher.Processing_Type,
                                        Status = inb_Voucher.Status,
                                        Site = inb_Voucher.Site,
                                        Article_No = inb_Voucher.Article_No,
                                        Bonus_Buy = inb_Voucher.Bonus_Buy,
                                        POSNo = inb_Voucher.POSNo,
                                        ReceiptNo = inb_Voucher.ReceiptNo,
                                        TranDate = inb_Voucher.TranDate,
                                        TranTime = inb_Voucher.TranTime,
                                        FileName = inb_Voucher.SerialNo
                                    };
                                    string insertSql = @"INSERT INTO INB_VoucherToSAP 
                                        (Voucher_Type, SerialNo, Voucher_Value, Voucher_Currency, Validity_From_Date, Expiry_Date, 
                                        Processing_Type, Status, Site, Article_No, Bonus_Buy, POSNo, ReceiptNo, TranDate, TranTime, FileName)
                                        VALUES
                                        (@Voucher_Type, @SerialNo, @Voucher_Value, @Voucher_Currency, @Validity_From_Date, @Expiry_Date,
                                        @Processing_Type, @Status, @Site, @Article_No, @Bonus_Buy, @POSNo, @ReceiptNo, @TranDate, @TranTime, @FileName)";
                                    int rowsAffected = connection.Execute(insertSql, inbVoucherSap);
                                    if (rowsAffected > 0)
                                    {
                                        _logger.Information("Data inserted successfully!" + rowsAffected + "Row");
                                    }
                                    string updatetSql = @"UPDATE TransCpnVchIssue SET IsSend = 1 WHERE SerialNo = @SerialNo ";
                                    int rowsAffectedupdate = connection.Execute(updatetSql, new { SerialNo = inbVoucherSap.SerialNo });

                                    if (rowsAffectedupdate > 0)
                                    {
                                        _logger.Information("Data Updated successfully!" + rowsAffectedupdate + " Row");
                                    }
                                }
                                else
                                {
                                    _logger.Information("Lỗi : " + calResult);
                                }
                            }
                            else
                            {
                                _logger.Information("Không có Data ");
                            }
                        }
                    }
                }
            }
        }catch (Exception e)
        {
            _logger.Error("Loi: ", e);
        }

        //------------------------------------------End Voucher SAP-----------------------------------------------------------
        SendEmailExample sendEmailExample = new SendEmailExample(_logger);
        ReadFile readfilSAP = new ReadFile(_logger);
        string IpSftp = configuration["IpSftp"];
        int Host = 22;
        string username = configuration["username"];
        string password = configuration["password"];
        string pathRemoteCar = configuration["pathRemoteCar"];
        string pathLocalCar = configuration["pathLocalCar"];
        string processedFoldercar = configuration["processedFoldercar"];
        SftpHelper sftpHelper = new SftpHelper(IpSftp, Host, username, password, _logger);
        _logger.Information("Bắt đầu tải CARStockBalance : Call sftpHelper.DownloadAuthen ");
        sftpHelper.DownloadAuthen(pathRemoteCar, pathLocalCar);
        _logger.Information("Bắt đầu đọc CARStockBalance: " + pathLocalCar);
        if (Directory.Exists(pathLocalCar))
        {
            string[] filteredStrings = Directory.GetFiles(pathLocalCar, "*.CSV");
            var count = 0;
            int countsl = filteredStrings.Length;
            _logger.Information("Tổng File :" + countsl);
            if (countsl > 0)
            {
                foreach (string xmlFile1 in filteredStrings)
                {
                    readfilSAP.ProcessCSV_CARStockBalance(xmlFile1, processedFoldercar);
                    count++;
                }
                sendEmailExample.ConfigMail("(Job CARStockBalance)- Số File : " + count.ToString() + " Insert thành công");
                _logger.Information("Process : " + count.ToString() + " file CARStockBalance");
                string[] getfile = Directory.GetFiles(pathLocalCar, "*.CSV");
                int fileLoi = getfile.Length;
                if(fileLoi > 0)
                {
                    sendEmailExample.ConfigMail("(Job CARStockBalance) Có : " + fileLoi + " file Lỗi Vui Lòng Kiểm tra ở Folder " + pathLocalCar);
                }
                DataSqlProcedure dataSql = new DataSqlProcedure(_logger);
                if (fileLoi == 0)
                {
                    List<string> siteList = dataSql.DataStoreXml();
                    foreach (var site in siteList)
                    {
                        string querry = $"SELECT '800_B' as Mandt ,site as WarehouseCode, '20' as MerchantCode, [ArticleNumber] as MerchantSku, " +
                            $"[UnreUseQty] as Quantity, '' as MovementType, '' as PostingDate," +
                            $"CASE WHEN TransitQty = '' THEN 0 ELSE CAST(TransitQty AS DECIMAL(18, 3)) END AS TransitQty," +
                            $"[BaseUoM] as UOM FROM [Inventory].[dbo].[CARStockBalances] where site = {site} and Status='0'";
                        string currentTimestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string xmldata =  dataSql.ConvertSQLtoXML(querry);
                       string outputPath = $"D:\\XmlData\\PRD_WINMART_STOCKBALANCE_{site}_{currentTimestamp}.xml";
                        if (!File.Exists(outputPath))
                        {
                            using (File.CreateText(outputPath)) { }
                        }
                        using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                        {
                      
                            writer.WriteLine(xmldata);
                        }

                    }
                    dataSql.Insert_TK_CarStockBalance();
                }
                else
                {
                    _logger.Information("Còn " + fileLoi + " Chưa đọc hết chưa run được procedure");
                }
            }
            else
            {
                _logger.Information("Không Có Data");
            }
        }
        else
        {
            _logger.Information("File Not Found");
            Directory.CreateDirectory(pathLocalCar);
        }
    }
}

