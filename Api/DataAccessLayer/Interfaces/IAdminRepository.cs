
using System;
using Sidekick.Api.Helpers;
using Sidekick.Model;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IAdminRepository
    {
        APIResponse RegisterAdmin(AdminProfile _admin);
        APIResponse AdminLogin(LoginCredentials _admin);
        APIResponse ReGenerateTokens(AdminLoginTransaction _admin);

        APIResponse GetAllAdmins(int _admin);
        APIResponse EditAdminProfile(AdminProfile _admin);
        APIResponse ChangeAdminStatus(ChangeRecordStatus _admin,string auth);

        APIResponse ForgotPassword(AdminForgotPassword _admin, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
    }
}
