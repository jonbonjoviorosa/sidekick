using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IUserPitchBookingRepository
    {
        Task<List<MatchListForPushNotificationViewModel>> GetMatchListForPushNotification(DateTime startDate, DateTime endDate);
        Task<UserPitchBooking> getUserPitchBooking(Guid BookingId);
    }
}
