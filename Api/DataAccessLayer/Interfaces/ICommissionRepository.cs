using Sidekick.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ICommissionRepository
    {
        /// <summary>
        ///  Gets All Comission Plays
        /// </summary>
        /// <returns>ComissionPlay</returns>
        Task<APIResponse<IEnumerable<CommissionPlay>>> ComissionPlays();
        /// <summary>
        ///  Gets All Comission Trains
        /// </summary>
        /// <returns>ComissionTrain</returns>
        Task<APIResponse<CommissionTrain>> ComissionTrains();
        /// <summary>
        ///  Adds or Edits a Comission Play
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="play"></param>
        /// <returns></returns>
        Task<APIResponse> AddOrEditComissionPlay(string _auth, List<CommissionPlaySportViewModel> plays);
        /// <summary>
        ///  Adds or Edits a Comission Train
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="train"></param>
        /// <returns></returns>
        APIResponse AddOrEditComissionTrain(string _auth, CommissionTrain train);

        Task<APIResponse<IEnumerable<CommisionReport>>> GetComissionReport();
    }
}
