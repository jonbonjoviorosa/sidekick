using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SendEmailController : ControllerBase
    {
        private readonly ISendEmailHandler sendEmailHandler;

        public SendEmailController(ISendEmailHandler sendEmailHandler)
        {
            this.sendEmailHandler = sendEmailHandler;
        }

        [HttpPost("ToAdmin")]
        public async Task<ActionResult<APIResponse>> SendEmailToAdmin([FromBody] EmailBodyViewModel body)
        {
            if (ModelState.IsValid)
            {
                return Ok(await sendEmailHandler.SendEmailToAdmin(body.Body));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

    }
}
