using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IFacilityPitchTimingRepository
    {
        /// <summary>
        /// Get Facility Pitch Timings
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<FacilityPitchTiming>>> GetFacilityPitchTimings(Guid facilityPitchId);
        Task<APIResponse> GetAll(Guid facilityId);
        Task<APIResponse> GetAllWithBooking(Guid facilityId);
        Task<APIResponse> GetTiming(Guid facilityPitchTimingId);

        Task<APIResponse> GetBookingDetails(Guid facilityPitchTimingId);
    }
}
