using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IUserDevicesRepository
    {
        Task<UserDevice> GetLatestDeviceFcmToken(Guid userId);
    }
}
