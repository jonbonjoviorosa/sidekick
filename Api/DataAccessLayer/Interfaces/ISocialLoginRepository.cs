using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Goals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ISocialLoginRepository
    {
        APIResponse AppleSignIn(LoginAppleCredentials _user);
        APIResponse ValidateAppleToken(string _refreshToken);
        APIResponse GetAppleClientSecret();
    }
}
