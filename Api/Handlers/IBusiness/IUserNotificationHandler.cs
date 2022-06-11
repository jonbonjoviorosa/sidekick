using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.UserNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IUserNotificationHandler
    {
        Task<APIResponse<UserNotificationViewModel>> GetNotifcation(Guid usernofication);
        Task<APIResponse> InsertUpdateNotification(UserNotificationViewModel notification);
        Task<APIResponse<List<UserNotificationViewModel>>> GetUserNotifcation();

        #region InApp Notification Templates
        Task PitchBookingCancellationFromCaptainMorethan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate);
        Task PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate);
        Task PitchBookingCancellationFromPlayerMorethan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromPlayerMorethan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromCaptainLessthan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate);
        Task PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate);
        Task<APIResponse> GetNotifications(Guid facilityId);
        Task PitchBookingCancellationFromPlayerLessthan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromPlayerLessthan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromFacilityMorethan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromFacilityLessthan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate);

        Task IndividualCoachingBookingConfirmationToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task GroupCoachingBookingConfirmationToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task CancellationCoachingFromUserMorethan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task CancellationCoachingFromCoachMorethan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate);
        Task CancellationCoachingFromUserLessthan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task CancellationCoachingFromCoachLessthan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingConfirmationToCaptain(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingConfirmationToPlayer(BookingNotificationCommonTemplate commonTemplate);
        Task IndividualCoachingRequestToCoach(BookingNotificationCommonTemplate commonTemplate);

        Task GroupCoachingBookingConfirmationToCoach(BookingNotificationCommonTemplate commonTemplate);

        Task IndividualCoachingCancellationLessthan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate);

        Task IndividualCoachingCancellationMorethan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate);

        Task GroupCoachingCancellationLessthan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate);

        Task GroupCoachingCancellationMorethan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate);

        Task ShareEventToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task CaptainAcceptsTheRequestToPlayer(BookingNotificationCommonTemplate commonTemplate);

        Task OneSpotIsFreeFromWaitingListToPlayer(BookingNotificationCommonTemplate commonTemplate);
        Task PitchBookingConfirmationToFacility(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromCaptainMorethan24HoursToFacility(BookingNotificationCommonTemplate commonTemplate);

        Task PitchBookingCancellationFromPlayerMorethan24HoursToFacility(BookingNotificationCommonTemplate commonTemplate);
        #endregion

    }
}
