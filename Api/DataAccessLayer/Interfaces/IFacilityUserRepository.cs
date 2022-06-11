
using System;
using System.Threading.Tasks;
using Sidekick.Api.Helpers;
using Sidekick.Model;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IFacilityUserRepository
    {
        APIResponse LoginFacilityUser(LoginCredentials _user);
        APIResponse AddFacilityUser(string _auth, FacilityUserProfile _user);
        APIResponse EditFacilityUser(string _auth, FacilityUserProfile _user);
        APIResponse GetFacilityUser(Guid _guid, bool _takeAll);
        APIResponse FacilityUserChangePassword(string _auth, FacilityUserChangePassword _user);
        APIResponse FacilityUserForgotPassword(FacilityUserForgotPassword _user, APIConfigurationManager _conf = null);
        APIResponse FacilityUserChangeStatus(string _auth, ChangeStatus _user);
        Task<APIResponse> GetUserRole(Guid facilityRoleId);
    }
}
