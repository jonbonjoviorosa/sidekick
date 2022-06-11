using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class UserDevicesRepository : IUserDevicesRepository
    {
        private readonly APIDBContext context;
        ILoggerManager LogManager { get; }
        public UserDevicesRepository(APIDBContext context, ILoggerManager _logManager)
        {
            this.context = context;
            LogManager = _logManager;
        }

        public async Task<UserDevice> GetLatestDeviceFcmToken(Guid userId)
        {
            return await context.UserDevices.OrderByDescending(s => s.CreatedDate).FirstOrDefaultAsync(s => s.IsEnabled == true && s.DeviceFCMToken != null && s.UserId == userId);
        }
    }
}
