using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Booking;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IBookingRepository
    {
        Task<IEnumerable<CoachNotAvailableScheduleViewModel>> GetCoachNotAvailableSched(Guid coachId);
        Task<IEnumerable<IndividualBookingViewModel>> GetIndividualBookingsPerCoach(Guid userId, bool getLatest);
        Task<IEnumerable<IndividualBookingViewModel>> GetIndividualBookingsPerParticipant(Guid userId, bool getLatest);

        Task<IEnumerable<IndividualBookingViewModel>> GetAllBookingsPerParticipant(Guid userId);

        Task<IEnumerable<IndividualBookingViewModel>> GetAllBookings();

        Task<IndividualBooking> GetIndividualBooking(Guid bookingId);
        Task<IEnumerable<CancelPlayBookingViewModel>> GetBookingReferences(Guid facilityPitchTimingId);

        Task<IEnumerable<CancelPlayBookingViewModel>> GetBookingReferencesByBookingId(Guid bookingId);

        Task<IndividualBooking> GetIndividualBookingByTelRRefNo(string telRRefNo);
        Task<int> GetCount_IndividualBookingByDate(DateTime date);
        Task<Guid> InsertUpdateIndividualBooking(Guid userId,
            IndividualBooking booking);
        Task UpdateIndividualBooking(Guid userId,
            IndividualBooking booking);
        Task<IEnumerable<GroupBookingViewModel>> GetGroupBookingsByUser(Guid userId, bool getLatest);
        Task<IEnumerable<GroupBookingViewModel>> GetGroupBookingsByCoach(Guid userId, bool getLatest);
        Task<GroupBookingViewModel> GetGroupBooking(Guid bookingId);
        Task<GroupBookingViewModel> GetGroupBookingByTelRRefNo(string telRRefNo);
        Task<int> GetCount_GroupBookingByDate(DateTime date);
        Task<IEnumerable<GroupBooking>> GetGroupBookingPerGroupClass(Guid groupClassId);
        Task<GroupBooking> GetGroupBookingDb(Guid bookingId);

        Task UpdateGroupBooking(Guid userId,
            GroupBooking booking);
        Task<Guid> InsertGroupBooking(Guid userId,GroupBooking booking);

        Task<IEnumerable<BookingViewModel>> GetAllBookingBeforeSetTimeOfAppointment(DateTime dateTime);

        Task<IEnumerable<BookingViewModel>> GetAllBookingBefore48HoursSetTimeOfAppointment(DateTime dateTime);

        Task<List<TrainingListForPushNotificationViewModel>> GetIndividualClassListForPushNotification(DateTime startDate, DateTime endDate);

        Task<List<TrainingListForPushNotificationViewModel>> GetGroupClassListForPushNotification(DateTime startDate, DateTime endDate);
        Task<bool> DeleteBookingId(CancelPlayBookingViewModel bookingSlot);
    }
}
