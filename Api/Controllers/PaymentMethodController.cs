using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Model;
using Sidekick.Model.PaymentMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodHandler paymentMethodHandler;

        public PaymentMethodController(IPaymentMethodHandler paymentMethodHandler)
        {
            this.paymentMethodHandler = paymentMethodHandler;
        }

        [HttpGet("Cards")]
        public async Task<ActionResult<APIResponse<IEnumerable<PaymentMethod_CardViewModel>>>> GetCards()
        {
            return Ok(await paymentMethodHandler.GetPaymentMethodCards());
        }

        [HttpGet("Card/{cardId}")]
        public async Task<ActionResult<APIResponse<PaymentMethod_CardViewModel>>> GetCard(Guid cardId)
        {
            return Ok(await paymentMethodHandler.GetPaymentMethodCard(cardId));
        }

        [HttpPost("Card")]
        public async Task<ActionResult<APIResponse>> InsertUpdateCard([FromBody] PaymentMethod_CardViewModel card)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = Messages.InvalidModelObject,
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                var response = await paymentMethodHandler.InsertUpdatePaymentMethodCard(card);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
        }

        [HttpPost("InitiatePayment")]
        public async Task<ActionResult<APIResponse>> InitiatePaymentMethod()
        {
            var response = await paymentMethodHandler.InitiatePaymentMethod();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }


    }
}
