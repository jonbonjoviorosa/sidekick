using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly INotificationRepository notificationRepository;
        private readonly IMapper mapper;
        private readonly ILoggerManager loggerManager;

        public NotificationHandler(INotificationRepository notificationRepository,
            IMapper mapper,
            ILoggerManager loggerManager)
        {
            this.notificationRepository = notificationRepository;
            this.mapper = mapper;
            this.loggerManager = loggerManager;
        }

        public async Task<APIResponse<NotificationViewModel>> GetNotifcation()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await notificationRepository.GetNotification();
                if (response == null)
                    response = new Notification();

                var mappedResponse = mapper.Map<NotificationViewModel>(response);
                return new APIResponse<NotificationViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<NotificationViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper<NotificationViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> InsertUpdateNotification(NotificationViewModel notification)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(notification);
                var mappedResponse = mapper.Map<Notification>(notification);
                await notificationRepository.InsertUpdateNotification(mappedResponse);
                return new APIResponse
                {
                    Status = Status.Success,
                    Message = Messages.NoticationUpdateSuccess,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
    }
}
