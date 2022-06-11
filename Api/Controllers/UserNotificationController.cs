using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Model;
using Sidekick.Model.UserNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserNotificationController : ControllerBase
    {
        private readonly IUserNotificationHandler notificationHandler;

        public UserNotificationController(IUserNotificationHandler notificationHandler)
        {
            this.notificationHandler = notificationHandler;
        }

        [HttpGet]
        public async Task<ActionResult<APIResponse>> GetUserNotication()
        {
            return Ok(await notificationHandler.GetUserNotifcation());
        }

        [HttpPost]
        public async Task<ActionResult<APIResponse>> InsertUpdateNotification([FromBody] UserNotificationViewModel notification)
        {
            return Ok(await notificationHandler.InsertUpdateNotification(notification));
        }

        [HttpGet("GetNotifications/{facilityId}")]
        public async Task<IActionResult> GetNotifications(Guid facilityId)
        {
            return Ok(await notificationHandler.GetNotifications(facilityId));
        }
    }
}
