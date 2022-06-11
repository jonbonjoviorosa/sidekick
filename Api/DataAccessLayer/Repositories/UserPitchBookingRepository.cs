using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class UserPitchBookingRepository : IUserPitchBookingRepository
    {
        private readonly APIDBContext context;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        public UserPitchBookingRepository(APIDBContext context, ILoggerManager _logManager)
        {
            this.context = context;
            LogManager = _logManager;
        }

        public async Task<List<MatchListForPushNotificationViewModel>> GetMatchListForPushNotification(DateTime startDate, DateTime endDate)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookings for push notification between--" + startDate.ToString() + "-" + endDate.ToString());

            var group = await (from x in context.UserPitchBookings
                               join y in context.Facilities
                                on x.FacilityId equals y.FacilityId
                               where x.PitchStart >= startDate && x.PitchStart <= endDate
                               && x.IsCancelled == false && x.IsPaid == true
                               select new MatchListForPushNotificationViewModel()
                               {
                                   BookingId = x.BookingId,
                                   BookingDate = x.PitchStart,
                                   FacilityName = y.Name,
                                   UserId = x.UserId,
                                   FacilityId = x.FacilityId
                               }).ToListAsync();
            return group;
        }

        public async Task<UserPitchBooking> getUserPitchBooking(Guid BookingId)
        {
            return await context.UserPitchBookings.FirstOrDefaultAsync(x => x.BookingId == BookingId);
        }
    }
}
