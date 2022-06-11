
using System;
using System.Threading.Tasks;
using Sidekick.Model;
using Sidekick.Model.Player;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IFacilityPlayerRepository
    {
        APIResponse GetFacilityPlayers(Guid _facilityId);
        Task<APIResponse> DeleteFacilityPlayer(Guid _facilityId, string userNo);
        Task<APIResponse> GetAllFacilityPlayers();
        Task<APIResponse> GetAllFacilityPlayersDates(string dateFrom, string dateTo);
        Task<APIResponse> AddOrEditPlayer(string auth, PlayerViewModel player);
        Task<APIResponse> GetPlayer(Guid userId);
        Task<APIResponse> ChangeStatus(string auth, ChangeStatus user);
    }
}
