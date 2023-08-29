using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using BluePosVoucher.Data;
using Job_By_SAP.Data;
using Microsoft.EntityFrameworkCore;
using Job_By_SAP.Models;
using BluePosVoucher.Models;

namespace Job_By_SAP
{

    public class SendEmailExample
    {
        private readonly ILogger _logger;
        public SendEmailExample(ILogger logger)
        {
            _logger = logger;
        }
        public void ConfigMail(string body)
        {
            using (var dbContext = new DbStaging_Inventory())
            {
                var result = dbContext.mailConfigs.FromSqlRaw("SP_Config_Mail").ToList();
                MailConfig mailConfig = new MailConfig();
                foreach (var config in result)
                {
                    mailConfig.smtpServer = config.smtpServer;
                    mailConfig.smtpPort = config.smtpPort;
                    mailConfig.smtpUsername = config.smtpUsername;
                    mailConfig.smtpPassword = config.smtpPassword;

                }
                // string smtpServer = "10.235.64.60";
                // int smtpPort = 25;
                //string smtpUsername = "vhud.system";
                // string smtpPassword = "r-Uj#8B\\<%<G";
                string recipient = "namnd4@crownx.masangroup.com";
                SendEmail(mailConfig.smtpServer, mailConfig.smtpPort, false, mailConfig.smtpUsername, mailConfig.smtpPassword, recipient, body);

            }
        }
        public void SendEmail(string mailServer, int port, bool enableSSL, string username, string password, string recipient,string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(mailServer, port))
                {
                    client.EnableSsl = enableSSL;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(username+"@winmart.masangroup.com");
                        mail.To.Add(recipient);
                        mail.Subject = "System Run Job Information";
                        mail.Body = $"TT: {body} -"+
                            "Time: " + DateTime.Now;
                            
                        client.Send(mail);
                    }

                    _logger.Information("Email sent successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending email: " + ex);
            }
        }
    }
}
