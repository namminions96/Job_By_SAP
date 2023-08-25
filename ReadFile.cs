using CsvHelper;
using BluePosVoucher.Data;
using BluePosVoucher.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Read_xml
{
    public class ReadFile
    {
        private readonly ILogger _logger;
        public ReadFile(ILogger logger)
        {
            _logger = logger;
        }
        
        public void ProcessCSV_CARStockBalance(string csvFile, string processedFolderPathter)
        {
            try
            {
                using (var dbContext = new DbStaging_Inventory())
                {
                    using (var reader = new StreamReader(csvFile))
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = "|", 
                        HasHeaderRecord = true, 
                    }))

                    {
                        string[] lines = File.ReadAllLines(csvFile);
                        CARStockBalance cARStock    = new CARStockBalance();
                        int count = 0;
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            string[] data = line.Split('|');
                            cARStock.Id = new Guid();
                            cARStock.Status = 0;
                            cARStock.TimeStamp = data[0];
                            cARStock.Site = data[1];
                            cARStock.ArticleNumber = data[2];
                            cARStock.MCH5 = data[3];
                            cARStock.BaseUoM = data[4];
                            cARStock.UnreUseQty = data[5];
                            cARStock.UnreConsQty =data[6];
                            cARStock.TransitQty = data[7];
                            cARStock.UnprSaleQty = RemoveCommas(data[8]);

                            //if (string.IsNullOrEmpty(cARStock.UnreUseQty))
                            //{
                            //    cARStock.UnreUseQty = "0";
                            //}

                            //if (string.IsNullOrEmpty(cARStock.UnreConsQty))
                            //{
                            //    cARStock.UnreConsQty = "0";
                            //}

                            //if (string.IsNullOrEmpty(cARStock.TransitQty))
                            //{
                            //    cARStock.TransitQty = "0";
                            //}

                            //if (string.IsNullOrEmpty(cARStock.UnprSaleQty))
                            //{
                            //    cARStock.UnprSaleQty = "0";
                            //}

                            dbContext.CARStockBalances.Add(cARStock);
                            dbContext.SaveChanges();
                            count++;
                        }
                        _logger.Information("File:  "+ Path.GetFileName(csvFile) + " " +"Count Data Inserted : " + count + " Row");
                    }
                }
                if (Directory.Exists(processedFolderPathter))
                {
                    string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    File.Move(csvFile, destinationPath);
                }
                else
                {
                    Directory.CreateDirectory(processedFolderPathter);
                    string destinationPath = Path.Combine(processedFolderPathter, Path.GetFileName(csvFile));
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }
                    File.Move(csvFile, destinationPath);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Lỗi ProcessCSV_CARStockBalance");
            }
        }
        public string RemoveCommas(string input)
        {
            while (input.EndsWith(",,") || input.EndsWith(","))
            {
                input = input.Substring(0, input.Length - 1);
            }

            return input;
        }
    }
}

