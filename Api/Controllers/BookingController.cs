using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingHandler handler;
        private APIConfigurationManager MConf { get; }

        public BookingController(IBookingHandler handler,APIConfigurationManager _conf)
        {
            this.handler = handler;
            this.MConf = _conf;
        }

        [HttpGet("Individual/User/{getLatest}")]
        public async Task<ActionResult<APIResponse<IEnumerable<IndividualBookingViewModel>>>> GetIndivdualBookingsPerUser(bool getLatest)
        {
            return Ok(await handler.GetIndivdualBookingsPerUser(getLatest));
        }

        [HttpGet("GetAllBookings")]
        public async Task<ActionResult<APIResponse<IEnumerable<IndividualBookingViewModel>>>> GetAllBookingsBookingsPerUser()
        {
            return Ok(await handler.GetAllBookingsBookingsPerUser());
        }


        [HttpGet("GetAllTrainBookings")]
        public async Task<ActionResult<APIResponse<IEnumerable<IndividualBookingViewModel>>>> GetAllTrainBookings()
        {
            return Ok(await handler.GetAllBookings());
        }

        [HttpGet("Individual/Coach/{getLatest}")]
        public async Task<ActionResult<APIResponse<IEnumerable<IndividualBookingViewModel>>>> GetIndivdualBookingsPerCoach(bool getLatest)
        {
            return Ok(await handler.GetIndivdualBookingsPerCoach(getLatest));
        }

        [HttpPost("Individual/AddOrEdit")]
        public async Task<ActionResult<APIResponse<string>>> InsertUpdateIndividualBooking([FromBody] IndividualBooking_SaveViewModel booking)
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
                var apiResp = await handler.InsertUpdateIndividualBooking(booking);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
        }

        [HttpGet("Individual/Confirm/{bookingId}")]
        public async Task<ActionResult<APIResponse<IndividualConfirmBookingViewModel>>> ConfirmIndivualBooking(Guid bookingId)
        {
            var apiResp = await handler.ConfirmIndividualBooking(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("Individual/ChangeStatus/{bookingId}/{status}")]
        public async Task<ActionResult<APIResponse>> ChangeStatusIndivdualBooking(Guid bookingId, EBookingStatus status)
        {
            APIResponse apiResp = await handler.ChangeStatusIndivdualBooking(bookingId, status);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Individual/Payment/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRResponseViewModel>>> IndidualBookingPayment(Guid bookingId)
        {
            APIResponse<TelRResponseViewModel> apiResp = await handler.IndividualBookingPayment(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Individual/PaymentAuthProcess/{bookingId}")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> IndidualBookingAuthProcessPayment(Guid bookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.IndividualBookingPaymentAuthProcess(bookingId,MConf);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Individual/PaymentProcess/{bookingId}")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> IndidualBookingProcessPayment(Guid bookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.IndividualBookingPaymentProcess(bookingId, MConf);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Individual/RejectProcess/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> IndidualBookingRejectProcess(Guid bookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.IndividualBookingPaymentCancel(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("CancelSlotBooking/{facilityPitchTimingId}")]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> CancelSlotBooking(Guid facilityPitchTimingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.CancelSlotBooking(facilityPitchTimingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }


        [HttpGet("CancelBooking/{BookingId}")]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> CancelBooking(Guid BookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.CancelBooking(BookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Group/RejectProcess/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> ClassBookingRejectProcess(Guid bookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.ClassBookingPaymentCancel(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Group/User/{getLatest}")]
        public async Task<ActionResult<APIResponse<IEnumerable<GroupBookingViewModel>>>> GetGroupBookingsPerUser(bool getLatest)
        {
            return Ok(await handler.GetGroupBookingsPerUser(getLatest));
        }

        [HttpGet("Group/Coach/{getLatest}")]
        public async Task<ActionResult<APIResponse<IEnumerable<GroupBookingViewModel>>>> GetGroupBookingsPerCoach(bool getLatest)
        {
            return Ok(await handler.GetGroupBookingsPerCoach(getLatest));
        }

        [HttpPost("Group/JoinGroup/{groupClassId}")]
        public async Task<ActionResult<APIResponse<string>>> JoinGroupClass(Guid groupClassId)
        {
            var apiResp = await handler.JoinGroupClass(groupClassId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpGet("Group/Confirm/{bookingId}")]
        public async Task<ActionResult<APIResponse<IndividualConfirmBookingViewModel>>> ConfirmGroupBooking(Guid bookingId)
        {
            var apiResp = await handler.ConfirmGroupBooking(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("Group/ChangeStatus")]
        public async Task<ActionResult<APIResponse>> ChangeStatusGroupBooking([FromBody] GroupBooking_UpdateStatusViewModel booking)
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
                APIResponse apiResp = await handler.ChangeStatusGroupBooking(booking);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }

        }

        [HttpGet("Group/Payment/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRResponseViewModel>>> GroupBookingPayment(Guid bookingId)
        {
            APIResponse<TelRResponseViewModel> apiResp = await handler.GroupBookingPayment(bookingId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [AllowAnonymous]
        [HttpGet("Group/PaymentAuthProcess/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> GroupBookingAuthProcessPayment(Guid bookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.GroupBookingAuthPaymentProcess(bookingId, MConf);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [AllowAnonymous]
        [HttpGet("Group/PaymentProcess/{bookingId}")]
        public async Task<ActionResult<APIResponse<TelRPaymentReponseViewModel>>> GroupBookingProcessPayment(Guid bookingId)
        {
            APIResponse<TelRPaymentReponseViewModel> apiResp = await handler.GroupBookingPaymentProcess(bookingId,MConf);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [AllowAnonymous]
        [HttpGet("AllBookingBefore48HoursSetTimeOfAppointment")]
        public async Task<ActionResult<APIResponse<IEnumerable<BookingViewModel>>>> GetAllBookingBefore48HoursSetTimeOfAppointment()
        {
            return Ok(await handler.GetAllBookingBefore48HoursSetTimeOfAppointment());
        }

        [AllowAnonymous]
        [HttpGet("AllBookingBeforeTimeOfAppointment")]
        public async Task<ActionResult<APIResponse<IEnumerable<BookingViewModel>>>> GetAllBookingBeforeSetTimeOfAppointment()
        {
            return Ok(await handler.GetAllBookingBeforeSetTimeOfAppointment());
        }

        [AllowAnonymous]
        [HttpGet("UpdateBookingsToValidated")]
        public async Task<ActionResult<APIResponse>> UpdateBookingsToValidated(EBookingType bookingType, 
            Guid bookingId, 
            bool isPaymentValidated)
        {
            if (EBookingType.Individual == bookingType)
            {
                return Ok(await handler.UpdateIndividualBookingPaymentValidation(bookingId, isPaymentValidated));
            }
            return Ok(await handler.UpdateGroupBookingPaymentValidation(bookingId, isPaymentValidated));
        }

        [AllowAnonymous]
        [HttpGet("UpdateBookingsToComplete")]
        public async Task<ActionResult<APIResponse>> UpdateBookingsToComplete(EBookingType bookingType,
            Guid bookingId,
            bool isPaymentValidated)
        {
            if (EBookingType.Individual == bookingType)
            {
                return Ok(await handler.UpdateIndividualBookingPaymentValidation(bookingId, isPaymentValidated));
            }
            return Ok(await handler.UpdateGroupBookingPaymentValidation(bookingId, isPaymentValidated));
        }

        [HttpGet("UpdateBookingsToPaid")]
        public async Task<ActionResult<APIResponse>> UpdateBookingsToPaid(EBookingType bookingType,
            Guid bookingId
            )
        {
            if (EBookingType.Individual == bookingType)
            {
                return Ok(await handler.UpdateIndividualBookingPaymentPaid(bookingId));
            }
            return Ok(await handler.UpdateGroupBookingPaymentPaid(bookingId));
        }
    }
}
