using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public class SendEmailHelper: ISendEmailHelper
    {
        private readonly APIConfigurationManager aPIConfigurationManager;
        private readonly ILoggerManager loggerManager;

        public SendEmailHelper(APIConfigurationManager aPIConfigurationManager,
            ILoggerManager loggerManager)
        {
            this.aPIConfigurationManager = aPIConfigurationManager;
            this.loggerManager = loggerManager;
        }

        public void SendEmail(EmailViewModel email)
        {
            try
            {
                var mailConfig = aPIConfigurationManager.MailConfig;
                using (MailMessage mm = new MailMessage(mailConfig.Username, email.SendTo))
                {
                    mm.Subject = email.Subject;
                    mm.Body = email.Body;
                    mm.IsBodyHtml = false;
                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = mailConfig.Server;
                        smtp.EnableSsl = mailConfig.EnableSSL;
                        NetworkCredential NetworkCred = new NetworkCredential(mailConfig.Username, mailConfig.Password);
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = mailConfig.Port;
                        smtp.Send(mm);
                    }
                }
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SENDEMAIL);
                loggerManager.LogError(ex.Message);
                loggerManager.LogError(ex.StackTrace);
                throw new Exception(EResponseAction.SendEmailFailed.ToString());
            }
        }
    }
}
