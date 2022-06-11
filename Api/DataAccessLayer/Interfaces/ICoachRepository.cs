using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ICoachRepository
    {
        APIResponse GetPlayers(int UserId);
        Task<IEnumerable<CoachViewModel>> GetCoaches();
        Task<CoachInfoViewModel> GetCoache(Guid coachId);
        APIResponse SetPlayerToCoach(string auth, int id);
        APIResponse BecomeACoach(CoachUpdateProfile CProfile);
        CoachProfile GetCoachProfile(Guid CoachId);
        Task<APIResponse> GetCoachProfileView(Guid CoachId);
        Task<Coach> GetCoach(Guid coachId);
        Task UpdateUserFromCoachProfile(Guid userId,
            DateTime dateOfBirth,
            string description,
            Genders gender);

        Task InsertUpdateCoach(Guid userId, CoachUpdateProfile coachProfile);
        Task<IEnumerable<CoachSpecialty>> GetSpecialties(Guid userId);
        Task InsertSpecialties(Guid userId,
            IEnumerable<Guid> specialties);
        Task DeleteSpecialties(IEnumerable<CoachSpecialty> specialties);
        Task<IEnumerable<CoachLanguage>> GetLanguages(Guid userId);
        Task InsertLanguages(Guid userId,
            IEnumerable<Guid> languages);
        Task DeleteLanguages(IEnumerable<CoachLanguage> languages);
        Task<IEnumerable<CoachGymAccess>> GetGymsAccess(Guid userId);
        Task InsertGymsAccess(Guid userId,
            IEnumerable<Guid> coachGymsAccess);
        Task DeleteGymsAccess(IEnumerable<CoachGymAccess> coachGymsAccess);

        Task<CoachEverydaySchedule> GetEverydaySchedule(Guid userId);
        Task InsertEverydaySchedule(Guid userId,
            CoachEverydayScheduleViewModel schedule);
        Task DeleteEverydaySchedule(CoachEverydaySchedule schedule);

        Task<IEnumerable<CoachCustomSchedule>> GetCustomSchedule(Guid userId);
        Task InsertCustomSchedule(Guid userId,
           IEnumerable<CoachCustomScheduleViewModel> schedules);
        Task DeleteCustomSchedule(IEnumerable<CoachCustomSchedule> schedules);
        Task<IEnumerable<TrainingBooking>> GetTrainingBookings();
        Task<CoachScheduleViewModel> GetCoachSchedule(Guid userId);

        Task<CoachProfileViewModel> GetCoacheProfile(Guid userId);
        APIResponse EditCoachProfile(CoachEditProfileFormModel _banner);
        Task<MyCoachingGroupListViewModel> GetMyCoachingGroupList(Guid userId, string title);
        Task<MyIndividualGroupListViewModel> GetMyIndividualGroupList(Guid userId, string name);
        Task<List<CoachAbsentSlotViewModel>> GetCoachAbsentSlot(Guid userId);
        Task<GetCoachAbsentSlotDetailsViewModel> GetCoachAbsentSlotDetails(int userId);
        Task UpdateCoachAbsentSlot(Guid userId, CoachCoachAbsentSlotViewModel schedules,int id);
        Task DeleteCoachAbsentSlot(int id);
        Task<GetInsightActivitiesViewModel> GetCoachInsightActivities(int searchTypr, Guid userId);
        Task<List<CoachCustomerListViewModel>> GetCoachCustomer(Guid userId, string name);
        Task<APIResponse> GetCoachConfirmation(ReviewType type, int id);
        Task<APIResponse> CancelCoachConfirmation(ReviewType type, int id);

        Task<APIResponse> GetCoachHome(string dateFrom, string dateTo);
        Task<APIResponse> GetCoachHomeDetail(ReviewType Type, Guid BookingId);

    }
}
