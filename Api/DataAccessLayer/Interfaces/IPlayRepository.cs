using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Play;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IPlayRepository
    {
        Task<IEnumerable<PlayFacilitiesModel>> FilterFacility(IEnumerable<PlayFilterViewModel> filters, Guid _sportId, string facilityName);
        Task<PlayFacilitiesViewModel> GetFacility(Guid _facilityId, DateTime bookDate, Guid _sportId);
        IEnumerable<PlayFacilitiesModel> SearchFacilityByName(string facilityName);
        Task<APIResponse> SaveFreeGame(FreeGame freeGame);
        Task<APIResponse> SubmitPlayRequest(PlayRequest playRequest, Guid UserID, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        Task<APIResponse> PitchBooking(UserPitchBookingModel pitchBooking,Guid UserID, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        Task<IEnumerable<UserPitchBooking>> GetPitchBookings(Guid facilityId);
        Task<IEnumerable<UserPitchBooking>> GetAllPitchBookings5MinsPriorToStartDate();

        Task<IEnumerable<UserPitchBooking>> GetAllPitchBookingPrior48hrsToStartDate();
        Task<IEnumerable<PlayBookingModel>> GetFacilityPitchBookingsDate(string dateFrom, string dateTo);
        Task<Filters> GetFilters();
        Task<IEnumerable<UserPitchBookingResponseModel>> GetPitchBookingBySportId(IEnumerable<PlayFilterViewModel> filters, Guid sportId, Guid userId,string facilityName);
        Task DeleteGamePlayers(Guid bookingId, Guid userId);
        Task AddEditGamePlayers(IEnumerable<FacilityPlayerModel> gamePlayers, Guid bookingId, Guid userId);
        Task<IEnumerable<PlayBookingModel>> GetAllPitchBookings();
        Task<PlayBookingModel> GetPitchBookingsDetails(Guid bookingId, Guid userId);
        Task<PlayBookingModel> GetPitchBookingsDetailsByTelRRefNo(string TelRRefNo);
        Task<FacilityPlayer> GetGamePlayer(Guid bookingId, Guid userId);
        Task<IEnumerable<FacilityPlayer>> GetGamePlayers(Guid bookingId);
        Task UpdateGamePlayer(FacilityPlayer gamePlayer);
        Task UpdateGameplayerToPaid(Guid gameplayId);
        Task<UserPitchBooking> GetPitchBooking(Guid bookingId);
        Task<IEnumerable<PlayRequestViewModel>> GetPlayRequestByBooking(Guid bookingId);
        Task<InviteRequestResponseModel> GetInviteRequestByBooking(Guid bookingId,string search);
        Task<APIResponse> SentInviteRequest(Guid bookingId, List<string> userSids, Guid userID);
        Task<MyPlayBookingResponseModel> MyPlayBooking(Guid userId);
        Task PitchBookingConfirmationToFacility(Guid bookingId, Guid userId);

        Task<APIResponse> FacilitySendContactMessageToPlayer(FacilitySendContactMessageToPlayerRequestModel messageToPlayerRequestModel);
        Task SendPlayPaymentFailNotification(Guid bookingId, Guid userId);
    }
}
