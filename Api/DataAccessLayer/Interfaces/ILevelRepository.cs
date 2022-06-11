using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Level;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ILevelRepository
    {
        /// <summary>
        ///  Gets All Location
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<Level>>> GetLevels();
        /// <summary>
        ///  Adds or Edits a Location
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        APIResponse AddOrEditLevel(string _auth, Level level);
        /// <summary>
        /// Set the record IsEnabled = false for that levelId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="levelId"></param>
        /// <returns></returns>
        APIResponse DeleteLevel(string _auth, Guid levelId);
    }
}
