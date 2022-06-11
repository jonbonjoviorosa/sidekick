using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface ICoachHandler
    {
        Task<APIResponse<IEnumerable<CoachViewModel>>> GetCoaches();
        Task<APIResponse> BecomeACoach(CoachUpdateProfile updateProf);
        Task<APIResponse<CoachScheduleViewModel>> GetCoachSchedule(Guid userId);
        Task<APIResponse> UpdateCoachSchedule(CoachScheduleViewModel sched);
        Task<APIResponse> GetTrainingBookings();
        Task<APIResponse<CoachInfoViewModel>> GetCoache(Guid userId);
        APIResponse EditCoachProfile(CoachEditProfileFormModel userProfile);
        Task<APIResponse<CoachProfileViewModel>> GetCoacheProfile(Guid userId);
        Task<APIResponse<MyCoachingGroupListViewModel>> GetMyCoachingGroupList(Guid userId,string title);
        Task<APIResponse<MyIndividualGroupListViewModel>> GetMyIndividualGroupList(Guid userId, string name);
        Task<APIResponse<List<CoachAbsentSlotViewModel>>> GetCoachAbsentSlot(Guid userId);
        Task<APIResponse<GetCoachAbsentSlotDetailsViewModel>> GetCoachAbsentSlotDetails(int id);
        Task<APIResponse> UpdateCoachAbsentSlot(CoachCoachAbsentSlotViewModel absentSlot, int id);
        Task<APIResponse> DeleteCoachAbsentSlot(int id);
        Task<APIResponse<GetInsightActivitiesViewModel>> GetCoachInsightActivities(int searchType, Guid userId);
        Task<APIResponse<List<CoachCustomerListViewModel>>> GetCoachCustomer(Guid userId, string name);
    }
}
