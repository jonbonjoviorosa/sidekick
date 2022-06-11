using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ISurfaceRepository
    {
        /// <summary>
        /// Gets All Surfaces
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<List<Surface>>> GetAllSurface();
        /// <summary>
        /// Adds a Surface
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="_sport"></param>
        /// <returns></returns>
        APIResponse AddOrEditSurface(string _auth, Surface surface);
        /// <summary>
        /// Set the record IsEnabled = false for that surfaceId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="sportId"></param>
        /// <returns></returns>
        APIResponse DeleteSurface(string _auth, Guid surfaceId);
    }
}
