using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IClassHandler
    {
        Task<APIResponse<IEnumerable<IndividualClassViewModel>>> GetIndividualClasses();
        Task<APIResponse<IndividualClassViewModel>> GetIndividualClass(Guid classId);
        Task<APIResponse<IndividualClassViewModel>> GetCoachIndividualClass(Guid userId);
        Task<APIResponse<IndividualClassByFilterViewModel>> GetIndividualClass_UserView(Guid classId,DateTime bookDate);
        Task<APIResponse<IEnumerable<IndividualClassByFilterViewModel>>> GetIndividualClassesByFilter(IEnumerable<FilterViewModel> filter);
        Task<APIResponse<GroupClassByFilterViewModel>> GetGroupClass_UserView(Guid classId);
        Task<APIResponse> InsertIndividualClass(IndividualClassViewModel individualClass);
        Task<APIResponse<GroupClassViewModel>> GetGroupClass(Guid classId);
        Task<APIResponse<IEnumerable<GroupClassViewModel>>> GetGroupClassesPerCoach(Guid userId);
        Task<APIResponse<IEnumerable<GroupClassByFilterViewModel>>> GetGroupClassByFilter(IEnumerable<FilterViewModel> filter);
        Task<APIResponse> InsertUpdateGroupClass(GroupClassViewModel groupClass);
        Task<APIResponse<Filters>> GetAllFilters();
        Task<APIResponse> GetCoachingClass(Guid classId);
        Task<APIResponse> GetAllGroupClasses();
        Task<APIResponse> CreateUpdateIndividualClass(string auth, ClassRenderViewModel individualClass);
        Task<APIResponse> DeleteClass(ChangeStatus classId);
    }
}
