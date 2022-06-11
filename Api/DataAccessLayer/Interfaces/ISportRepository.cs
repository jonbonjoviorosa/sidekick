
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ISportRepository
    {
        Task<APIResponse<List<Sport>>> GetSports();
        APIResponse AddSport(string _auth, SportDto _sport);
        APIResponse EditSport(string _auth, SportDto _sport);
        APIResponse GetSport(int _sportId);
        APIResponse DeleteSport(string _auth, Guid sportId);

        Task<Sport> GetSport(Guid sportId);
    }
}
