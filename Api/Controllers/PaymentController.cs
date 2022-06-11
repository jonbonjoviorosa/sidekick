using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using Sidekick.Model.Payment;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Payment")]
    public class PaymentController : APIBaseController
    {
        private readonly IPaymentHandler paymentHandler;
        IPaymentRepository PayRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public PaymentController(IPaymentRepository _aRepo, IPaymentHandler paymentHandler, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            PayRepo = _aRepo;
            this.paymentHandler = paymentHandler;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        //[AllowAnonymous]
        //[HttpGet("PaymentResponse/{orderRef}")]
        //public IActionResult PaymentResponse(string orderRef)
        //{
        //    //var requestContent = HttpContext.Request;
        //    //string jsonContent = requestContent.BodyReader.ReadAsync().Result.ToString();
        //    //var respContent = HttpContext.Response;
        //    //return Ok();
        //}


        [AllowAnonymous]
        [HttpGet("PaymentResponse/{orderRef}")]
        public async Task<ActionResult<APIResponse<TelRCheckPaymentResponseViewModel>>> PaymentResponse(string orderRef)
        {
            var apiResp = await paymentHandler.CheckStatusPayment(orderRef);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        //[AllowAnonymous]
        //[HttpGet("PaymentResponseCan")]
        //public IActionResult PaymentResponseCan()
        //{
        //    var requestContent = HttpContext.Request;
        //    string jsonContent = requestContent.BodyReader.ReadAsync().Result.ToString();
        //    var respContent = HttpContext.Response;
        //    return Ok();
        //}
        //[AllowAnonymous]
        //[HttpGet("PaymentResponseDecl")]
        //public IActionResult PaymentResponseDecl()
        //{
        //    var requestContent = HttpContext.Request;
        //    string jsonContent = requestContent.BodyReader.ReadAsync().Result.ToString();
        //    var respContent = HttpContext.Response;
        //    return Ok();
        //}
        //[AllowAnonymous]
        //[HttpPost("PaymentResponse")]
        //public IActionResult PaymentResponse()
        //{
        //    var requestContent = HttpContext.Request;
        //    string jsonContent = requestContent.BodyReader.ReadAsync().Result.ToString();
        //    return Ok();
        //}

        [HttpPost("InitiatePayment")]
        public IActionResult InitiatePayment([FromHeader] string Authorization,[FromBody] PaymentCheckout CheckOutData)
        {
            APIResponse resp = PayRepo.InitiatePayment(Authorization.Split(' ')[1], CheckOutData);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(resp);
            }
            else
            {
                return BadRequest(resp);
            }
        }

        [HttpGet("PaymentSummaries")]
        public async Task<IActionResult> PaymentSummaries()
        {
            var response = await PayRepo.PaymentSummaries();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetPaymentSummaries")]
        public async Task<IActionResult> GetPaymentSummaries()
        {
            var response = await PayRepo.GetPaymentSummaries();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("GetPlayPaymentHistory")]
        public async Task<IActionResult> GetPlayPaymentHistory()
        {
            var response = await PayRepo.GetPlayPaymentHistory();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
