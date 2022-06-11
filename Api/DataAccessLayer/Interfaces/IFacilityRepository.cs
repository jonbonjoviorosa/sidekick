
using System;
using System.Threading.Tasks;
using Sidekick.Api.Helpers;
using Sidekick.Model;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IFacilityRepository
    {
        Task<APIResponse> AddFacility(string _auth, FacilityProfile _facility, IMainHttpClient _mhttpc = null, APIConfigurationManager _conf = null);
        APIResponse GetAllFacilities();
        APIResponse GetFacilityProfile(Guid _facilityId);
        APIResponse EditFacilityProfile(FacilityProfile _facility);
        Task<APIResponse> FacilityChangeStatus(string _auth, ChangeStatus _facility);
        APIResponse UpdateFacilityProfile(FacilityProfile _facility);
        APIResponse GetFacilityUserTypes();
        APIResponse GetAreas();
        APIResponse AddEditFacilityUserType(FacilityUserType _facilityUserType);
        APIResponse DeleteFacilityUserType(FacilityUserType _facilityUserType);
        Task<Facility> GetFacility(Guid FacilityId);
    }
}
