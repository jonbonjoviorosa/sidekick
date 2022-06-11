using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public static class EmailHelper
    {
        public static int SendEmailByEmailAddress(IEnumerable<string> _recipients, SMTPConfig _smtpConfig, ILoggerManager _logMgr)
        {
            _logMgr.LogInfo("SendEmailByEmail Start");
            _logMgr.LogDebugObject(_recipients);
            _logMgr.LogDebugObject(_smtpConfig);

            var retStatus = 0;

            try
            {
                // initialize email client
                SmtpClient client = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);

                // initialize message
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_smtpConfig.Username);
                mailMessage.IsBodyHtml = true;

                // one recipient
                if (_recipients.Count() < 2)
                {
                    mailMessage.To.Add(_recipients.First());
                }
                else // bulk
                {
                    // set this because email sending does not seem to work without a 'to'
                    mailMessage.To.Add(_smtpConfig.Username);

                    // set recipients to bcc
                    foreach (var toAddress in _recipients)
                    {
                        mailMessage.Bcc.Add(toAddress);
                    }
                }

                // body and subject
                mailMessage.Body = _smtpConfig.Body;
                mailMessage.Subject = _smtpConfig.Subject;

                client.Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password);
                client.EnableSsl = _smtpConfig.EnableSSL;
                client.UseDefaultCredentials = false;
                // Note : Must set https://www.google.com/settings/security/lesssecureapps to allow mail sending
                client.Send(mailMessage);
            }
            catch (Exception e)
            {
                retStatus = -1;
                _logMgr.LogInfo("Something went wrong in SendEmailByEmail");
                _logMgr.LogDebugObject(e);
            }

            _logMgr.LogInfo("SendEmailByEmail End");

            return retStatus;
        }

        public static async Task<int> SendEmailByEmailAddressAsync(IEnumerable<string> _recipients, SMTPConfig _smtpConfig, ILoggerManager _logMgr)
        {
            _logMgr.LogInfo("SendEmailByEmail Start");
            _logMgr.LogDebugObject(_recipients);
            _logMgr.LogDebugObject(_smtpConfig);

            var retStatus = 0;

            try
            {
                // initialize email client
                SmtpClient client = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);

                // initialize message
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_smtpConfig.Username);
                mailMessage.IsBodyHtml = true;

                // one recipient
                if (_recipients.Count() < 2)
                {
                    mailMessage.To.Add(_recipients.First());
                }
                else // bulk
                {
                    // set this because email sending does not seem to work without a 'to'
                    mailMessage.To.Add(_smtpConfig.Username);

                    // set recipients to bcc
                    foreach (var toAddress in _recipients)
                    {
                        mailMessage.Bcc.Add(toAddress);
                    }
                }

                // body and subject
                mailMessage.Body = _smtpConfig.Body;
                mailMessage.Subject = _smtpConfig.Subject;

                client.Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password);
                client.EnableSsl = _smtpConfig.EnableSSL;
                client.UseDefaultCredentials = false;
                // Note : Must set https://www.google.com/settings/security/lesssecureapps to allow mail sending
               await client.SendMailAsync(mailMessage, CancellationToken.None);
            }
            catch (Exception e)
            {
                retStatus = -1;
                _logMgr.LogInfo("Something went wrong in SendEmailByEmail");
                _logMgr.LogDebugObject(e);
            }

            _logMgr.LogInfo("SendEmailByEmail End");

            return retStatus;
        }
    }
}
