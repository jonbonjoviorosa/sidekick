using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Model;
using Sidekick.Model.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationHandler notificationHandler;
        private readonly IPushNotificationHandler pushNotificationHandler;

        public NotificationController(INotificationHandler notificationHandler, IPushNotificationHandler pushNotificationHandler)
        {
            this.notificationHandler = notificationHandler;
            this.pushNotificationHandler = pushNotificationHandler;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetNotication()
        {
            return Ok(await notificationHandler.GetNotifcation());
        }

        [HttpPost("InsertNotification")]
        public async Task<ActionResult<APIResponse>> InsertUpdateNotification([FromBody]NotificationViewModel notification)
        {
            return Ok(await notificationHandler.InsertUpdateNotification(notification));
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("SendBookingStartingPlay")]
        public async Task<ActionResult<APIResponse>> SendBookingStartingPlay()
        {
            await pushNotificationHandler.SendBookingStartingPlayNotificationBefore24Hours();
            return Ok();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("SendIndividualClassStarting")]
        public async Task<ActionResult<APIResponse>> SendBookingStartingIndividualClass()
        {
            await pushNotificationHandler.SendBookingStartingIndividualClassNotificationBefore24Hours();
            return Ok();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        [HttpGet("SendGroupClassStarting")]
        public async Task<ActionResult<APIResponse>> SendBookingStartingGroupClass()
        {
            await pushNotificationHandler.SendBookingStartingGroupClassNotificationBefore24Hours();
            return Ok();
        }
    }
}
