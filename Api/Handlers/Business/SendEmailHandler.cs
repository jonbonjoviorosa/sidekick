using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class SendEmailHandler: ISendEmailHandler
    {
        private readonly ISendEmailHelper sendEmailHelper;
        private readonly APIConfigurationManager aPIConfigurationManager;
        private readonly ILoggerManager loggerManager;
        private readonly IUserRepository userRepository;
        private readonly IUserHelper userHelper;

        public SendEmailHandler(ISendEmailHelper sendEmailHelper,
            APIConfigurationManager aPIConfigurationManager,
            ILoggerManager loggerManager,
            IUserRepository userRepository,
            IUserHelper userHelper)
        {
            this.sendEmailHelper = sendEmailHelper;
            this.aPIConfigurationManager = aPIConfigurationManager;
            this.loggerManager = loggerManager;
            this.userRepository = userRepository;
            this.userHelper = userHelper;
        }

        public async Task<APIResponse> SendEmailToAdmin(string body)
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getuser = await userRepository.GetUser(currentLogin);
                body += Environment.NewLine;
                body += Environment.NewLine;
                body += "FROM:";
                body += Environment.NewLine;
                body += $"{getuser.FirstName} {getuser.LastName}";
                body += Environment.NewLine;
                body += getuser.Email;

                EmailViewModel email = new EmailViewModel()
                {
                    Subject = aPIConfigurationManager.SideKickAdminEmailConfig.Subject,
                    SendTo = aPIConfigurationManager.SideKickAdminEmailConfig.Email,
                    Body = body
                };

                sendEmailHelper.SendEmail(email);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.SendEmailSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                if (ex.Message == EResponseAction.SendEmailFailed.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.SendEmailFailed);


                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
           

        }
    }
}
