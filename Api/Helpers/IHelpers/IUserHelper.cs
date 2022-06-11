using System;

namespace Sidekick.Api.Helpers.IHelpers
{
    public interface IUserHelper
    {
        Guid GetCurrentUserGuidLogin();
        int GetCurrentUserIdLogin();
    }
}
