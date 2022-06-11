using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Size;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ISizeRepository
    {
        /// <summary>
        /// Gets All Size
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<TeamSize>>> GetAllSizes();
        /// <summary>
        /// Adds a Size
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        APIResponse AddOrEditTeamSize(string _auth, TeamSize size);
        /// <summary>
        ///  Set the record IsEnabled = false for that sizeId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="sizeId"></param>
        /// <returns></returns>
        APIResponse DeleteSize(string _auth, Guid sizeId);
    }
}
