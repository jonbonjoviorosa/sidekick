using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Play;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IPlayHandler
    {
        Task<APIResponse<IEnumerable<PlayFacilitiesModel>>> FilterPlayFacilities(IEnumerable<PlayFilterViewModel> filters, Guid _sportId, string facilityName);
        Task<APIResponse<PlayFacilitiesViewModel>> GetPlayFacilities(Guid _facilityId, DateTime bookDate,Guid _sportId);
        APIResponse<IEnumerable<PlayFacilitiesModel>> SearchFacilityByName(string facilityName);
        Task<APIResponse> OrganizeFreeGame(FreeGame freeGame);
        Task<APIResponse<IEnumerable<PlayRequestViewModel>>> GetPlayRequest(Guid bookingId);
        
        Task<APIResponse<InviteRequestResponseModel>> GetInviteRequest(Guid bookingId, string search);
        Task<APIResponse> SentInviteRequest(List<string> userSids,Guid bookingId);
        Task<APIResponse> GetFacilityPitchBookingsDate(string dateFrom, string dateTo);
        Task<APIResponse> PlayRequest(PlayRequest playRequest, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        Task<APIResponse> PitchBooking(UserPitchBookingModel pitchBooking, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        Task<APIResponse> GetPitchBookings(Guid facilityPitchId);
        Task<APIResponse<Filters>> GetAllFilters();
        Task<APIResponse> GetPitchBookingBySportId(IEnumerable<PlayFilterViewModel> filters, Guid sportId, string facilityName);
        Task<APIResponse> GetAllPitchBookings();
        Task<APIResponse> GetPitchBooking(Guid pitchBookingId);
        Task<APIResponse> GetPitchBookingByTelRRefNo(string TelRRefNo);
        Task<APIResponse> UpdateGameplayerToPaid(Guid playerId);
        Task<APIResponse> UpdateGamePlayer(FacilityPlayer facilityPlayer);
        Task<APIResponse<TelRResponseViewModel>> Payment(Guid facilityId);
        Task<APIResponse> CapturePayment(bool forDeposit, Guid pitchBookingId);
        Task<APIResponse> PaymentAuthProcess(Guid pitchBookingId);
        Task<APIResponse> PaymentCaptureProcess(Guid pitchBookingId);
        Task<APIResponse<TelRPaymentReponseViewModel>> RefundPayment(Guid bookingId);
        Task<APIResponse<TelRResponseViewModel>> RemainingPayment(Guid bookingId);
        Task<APIResponse<IEnumerable<UserPitchBooking>>> GetAllPitchBookingPriorToStartDate();
        Task<APIResponse<IEnumerable<UserPitchBooking>>> GetAllPitchBookingPrior48hrsToStartDate();
        Task<APIResponse> MyPlayBooking();
        Task<APIResponse> FacilitySendContactMessageToPlayer(FacilitySendContactMessageToPlayerRequestModel messageToPlayerRequestModel);
    }
}
