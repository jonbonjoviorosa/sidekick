using Sidekick.Model;
using Sidekick.Api.Helpers;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class APIBaseRepo
    {
        public Guid GetUpdaterId(string Auth,APIDBContext DbContext) 
        {
            // get user id by the person trying to update
            Guid updater = Guid.NewGuid();
            UserLoginTransaction UserUpdatingTheProfile = DbContext.UserLoginTransactions.FirstOrDefault(u => u.Token == Auth);
            if (UserUpdatingTheProfile != null) { return UserUpdatingTheProfile.UserId; };
            AdminLoginTransaction AdminUpdatingTheProfile = DbContext.AdminLoginTransactions.FirstOrDefault(a => a.Token == Auth);
            if (AdminUpdatingTheProfile != null) { return AdminUpdatingTheProfile.AdminId; }
            return updater;
        }

        public string GenerateUniqueCode(int _requiredLength, bool _requireDigit, bool _requireUppercase)
        {

            //int requiredUniqueChars = -1;
            //bool requireDigit = true;
            //bool requireNonAlphanumeric = false;
            //bool requireUppercase = true;
            //bool requireLowercase = false;

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "0123456789"                 // digits
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (_requireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (_requireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            var theBit = _requireUppercase ? 0 : 1;
            for (int i = chars.Count; i < _requiredLength; i++)
            {
                string rcs = randomChars[rand.Next(theBit, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public string CreateJWTToken(User _logindetails, DateTime _notBefore, APIConfigurationManager _apiConf)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, _logindetails.Email),
                new Claim(JwtRegisteredClaimNames.Sub, _logindetails.UserType.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _apiConf.TokenKeys.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, _apiConf.TokenKeys.Audience),
                new Claim(JwtRegisteredClaimNames.Exp, _notBefore.AddMinutes(_apiConf.TokenKeys.Exp).ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, _logindetails.Id.ToString())
            };
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_apiConf.TokenKeys.Key));
            var jwt = new JwtSecurityToken(
                issuer: _apiConf.TokenKeys.Issuer,
                audience: _apiConf.TokenKeys.Audience,
                claims: claims,
                notBefore: _notBefore,
                expires: _notBefore.AddMinutes(_apiConf.TokenKeys.Exp),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public string CreateJWTTokenForAdmin(Admin _logindetails, DateTime _notBefore, APIConfigurationManager _apiConf)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, _logindetails.Email),
                new Claim(JwtRegisteredClaimNames.Sub, _logindetails.AdminType.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _apiConf.TokenKeys.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, _apiConf.TokenKeys.Audience),
                new Claim(JwtRegisteredClaimNames.Exp, _notBefore.AddMinutes(_apiConf.TokenKeys.Exp).ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, _logindetails.Id.ToString())
            };
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_apiConf.TokenKeys.Key));
            var jwt = new JwtSecurityToken(
                issuer: _apiConf.TokenKeys.Issuer,
                audience: _apiConf.TokenKeys.Audience,
                claims: claims,
                notBefore: _notBefore,
                expires: _notBefore.AddMinutes(_apiConf.TokenKeys.Exp),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public string CreateJWTTokenForFacilityAdmin(FacilityUser _logindetails, DateTime _notBefore, APIConfigurationManager _apiConf)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, _logindetails.Email),
                new Claim(JwtRegisteredClaimNames.Iss, _apiConf.TokenKeys.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, _apiConf.TokenKeys.Audience),
                new Claim(JwtRegisteredClaimNames.Exp, _notBefore.AddMinutes(_apiConf.TokenKeys.Exp).ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, _logindetails.Id.ToString())
            };
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_apiConf.TokenKeys.Key));
            var jwt = new JwtSecurityToken(
                issuer: _apiConf.TokenKeys.Issuer,
                audience: _apiConf.TokenKeys.Audience,
                claims: claims,
                notBefore: _notBefore,
                expires: _notBefore.AddMinutes(_apiConf.TokenKeys.Exp),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public IEnumerable<KeyValuePair<string, string[]>> GetStackError(Exception ex)
        {
            return new[] { new KeyValuePair<string, string[]>(ex.Message, SplitStackTrace(ex.StackTrace)) };
        }

        public string[] SplitStackTrace(string stackT)
        {
            string[] stackTraceTemp = stackT.Split("\r\n");
            List<string> returnStack = new List<string>();
            foreach (var stack in stackTraceTemp)
            {
                returnStack.Add(stack);
            }
            return returnStack.ToArray();
        }

        public async Task<string> SendSmsAsync(IMainHttpClient _httpc, SmsParameter _smsConfig)
        {
            var ParameterBuilder = "?action=" + _smsConfig.Action;
            ParameterBuilder += "&user=" + _smsConfig.User;
            ParameterBuilder += "&password=" + _smsConfig.Password;
            ParameterBuilder += "&from=" + _smsConfig.From;
            ParameterBuilder += "&to=" + _smsConfig.To;
            ParameterBuilder += "&text=" + Uri.EscapeUriString(_smsConfig.Text);
            var apiCall = await _httpc.SendSmsHttpClientRequestAsync(ParameterBuilder);
            return "Sending SMS Asynchronously.. to:" + _smsConfig.To + " msg: " + _smsConfig.Text;

        }

        public int SendEmailByEmailAddress(IEnumerable<string> _recipients, SMTPConfig _smtpConfig, ILoggerManager _logMgr)
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
    }
}
