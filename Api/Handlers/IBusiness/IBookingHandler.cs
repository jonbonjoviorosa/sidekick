using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IBookingHandler
    {
        Task<APIResponse<MyBookingViewModel>> GetIndivdualBookingsPerUser(bool getLatest);
        Task<APIResponse<IEnumerable<IndividualBookingViewModel>>> GetAllBookingsBookingsPerUser();
        Task<APIResponse<IEnumerable<IndividualBookingViewModel>>> GetAllBookings();
        Task<APIResponse<IEnumerable<IndividualBookingViewModel>>> GetIndivdualBookingsPerCoach(bool getLatest);
        Task<APIResponse<IndividualBooking>> InsertUpdateIndividualBooking(IndividualBooking_SaveViewModel booking);
        Task<APIResponse> ChangeStatusIndivdualBooking(Guid bookingId, EBookingStatus status);
        Task<APIResponse<IndividualConfirmBookingViewModel>> ConfirmIndividualBooking(Guid bookingId);
        Task<APIResponse<GroupBookingViewModel>> ConfirmGroupBooking(Guid groupClassId);
        Task<APIResponse<TelRResponseViewModel>> IndividualBookingPayment(Guid bookingId);
        Task<APIResponse<TelRPaymentReponseViewModel>> IndividualBookingPaymentProcess(Guid bookingId, APIConfigurationManager _conf = null);
        Task<APIResponse<TelRPaymentReponseViewModel>> IndividualBookingPaymentAuthProcess(Guid bookingId, APIConfigurationManager _conf = null);
        Task<APIResponse<TelRPaymentReponseViewModel>> IndividualBookingPaymentCancel(Guid bookingId);
        Task<APIResponse<TelRPaymentReponseViewModel>> ClassBookingPaymentCancel(Guid bookingId);
        Task<APIResponse> UpdateIndividualBookingPaymentValidation(Guid bookingId, bool isPaymentValidated);
        Task<APIResponse> UpdateIndividualBookingPaymentComplete(Guid bookingId, bool isPaymentValidated);
        Task<APIResponse> UpdateIndividualBookingPaymentPaid(Guid bookingId);
        Task<APIResponse<IndividualBooking>> GetIndivdualBookingsByTelRRefNo(string TelRRefNo);
        Task<APIResponse<IEnumerable<GroupBookingViewModel>>> GetGroupBookingsPerUser(bool getLatest);
        Task<APIResponse<IEnumerable<GroupBookingViewModel>>> GetGroupBookingsPerCoach(bool getLatest);
        Task<APIResponse<GroupBookingViewModel>> JoinGroupClass(Guid groupClassId);
        Task<APIResponse> ChangeStatusGroupBooking(GroupBooking_UpdateStatusViewModel booking);
        Task<APIResponse<TelRResponseViewModel>> GroupBookingPayment(Guid bookingId);
        Task<APIResponse<TelRPaymentReponseViewModel>> GroupBookingPaymentProcess(Guid bookingId, APIConfigurationManager _conf = null);

        Task<APIResponse<TelRPaymentReponseViewModel>> GroupBookingAuthPaymentProcess(Guid bookingId, APIConfigurationManager _conf = null);

        Task<APIResponse<IEnumerable<BookingViewModel>>> GetAllBookingBeforeSetTimeOfAppointment();

        Task<APIResponse<IEnumerable<BookingViewModel>>> GetAllBookingBefore48HoursSetTimeOfAppointment();

        Task<APIResponse> UpdateGroupBookingPaymentValidation(Guid bookingId, bool isPaymentValidated);
        Task<APIResponse> UpdateGroupBookingPaymentPaid(Guid bookingId);
        Task<APIResponse<GroupBookingViewModel>> GetGroupBookingsByTelRRefNo(string TelRRefNo);
        Task<APIResponse<TelRPaymentReponseViewModel>> CancelSlotBooking(Guid facilityPitchTimingId);
        Task<APIResponse<TelRPaymentReponseViewModel>> CancelBooking(Guid bookingID);


    }
}
