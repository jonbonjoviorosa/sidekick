
using Sidekick.Model;
using System;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IFacilityPitchRepository
    {
        Task<APIResponse> AddFacilityPitch(string _auth, FacilityPitchDto _pitch);
        Task<APIResponse> GetFacilityPitch(Guid _guid);
        Task<APIResponse> GetFacilityBookingPitch(Guid _guid);
        Task<APIResponse> GetFacilityPitchSports(Guid _facilityId);
        Task<APIResponse> GetFacilityPitchByFacilityPitchId(Guid _guid, Guid _facilityPitchId, Guid sportId);
        Task<APIResponse> GetFacilityPitchByFacilityPitchIdWithTiming(Guid _guid, Guid _facilityPitchId, Guid sportId,Guid facilityPitchTimingId,Guid bookingId);
        Task<APIResponse> AddBlockSlot(string _auth, UnavailableSlot blockSlot);
        Task<APIResponse> GetBlockedSlots(Guid _facilityId);
        Task<APIResponse> CreateSlot(string auth, FacilityPitchVM pitch);
        Task<APIResponse> GetAllFacilityPitchTiming();
        Task<APIResponse> GetAllFacilityPitchBooking();
        Task<APIResponse> GetSlot(Guid facilityPitchTimingId);
        Task<APIResponse> UpdateSlot(string auth, EditSlotViewModel viewModel);
        Task<APIResponse> DeleteSlot(ChangeStatus facilityPitchTimingId);
        Task<APIResponse> GetUnavailableSlot(Guid unavailableSlotId);
        Task<APIResponse> DeleteUnavailableSlot(ChangeStatus status);
        Task<APIResponse> GetFacilityPitchesByFacilityId(Guid facilityId, Guid facilityPitchId);
    }
}
