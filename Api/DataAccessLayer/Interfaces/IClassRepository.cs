using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IClassRepository
    {
        Task<IEnumerable<IndividualClass>> GetIndividualClasses();
        Task<IndividualClass> GetIndividualClass(Guid classId);
        Task<IndividualClass> GetLatestIndividualClassByCoach(Guid userId);
        Task<IEnumerable<IndividualClassDetails>> GetLatestIndividualClassDetailsByCoach(Guid userId);
        Task<IndividualClassByFilterViewModel> GetIndividualClass_UserView(Guid classId);
        Task<IEnumerable<IndividualClassByFilterViewModel>> GetIndividualClassesByFilter(IEnumerable<FilterViewModel> filter);
        Task<IEnumerable<IndividualClassDetails>> GetIndividualClassDetails(Guid individualClass);
        Task<Guid> InsertIndividualClass(Guid userId, IndividualClass individualClass);
        Task InsertIndividualClassDetails(IEnumerable<IndividualClassDetails> details);
        Task DisableLatestIndividualClass(Guid userId,
            IndividualClass individualClass);
        Task DisableLatestIndividualClassDetails(Guid userId,
            IEnumerable<IndividualClassDetails> individualClassDetails);
        Task<GroupClass> GetGroupClass(Guid classId);
        Task<IEnumerable<GroupClass>> GetGroupClassesPerCoach(Guid userId);
        Task<GroupClassByFilterViewModel> GetGroupClass_UserView(Guid classId, Guid userID);
        Task<IEnumerable<GroupClassByFilterViewModel>> GetGroupClassByFilter(IEnumerable<FilterViewModel> filter,Guid userID);
        Task InsertUpdateGroupClass(Guid userId, GroupClass groupClass);
        Task<Filters> GetFilters();
        Task<IEnumerable<ClassRenderViewModel>> GetAllGroupClasses();
        Task CreateUpdateIndividualClass(Guid adminId, ClassRenderViewModel individualClass);
        Task<ClassRenderViewModel> GetCoachingClass(Guid classId);
        Task DeleteClass(ChangeStatus classId);
        Task<IEnumerable<IndividualClassDetails>> GetIndividualClassDetailsByCoachForBetweenDate(Guid userId, DateTime startDate, DateTime EndDate, Guid ClassId);

        Task<IEnumerable<GroupClass>> GetGroupClassDetailsByCoachForBetweenDate(Guid userId, DateTime startDate, DateTime EndDate,Guid GroupClassId);
    }
}
