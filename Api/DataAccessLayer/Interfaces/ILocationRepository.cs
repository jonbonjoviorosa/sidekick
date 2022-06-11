using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ILocationRepository
    {
        /// <summary>
        ///  Gets All Location
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<Location>>> GetLocations();
        /// <summary>
        ///  Adds or Edits a Location
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        APIResponse AddOrEditLocation(string _auth, Location location);
        /// <summary>
        /// Set the record IsEnabled = false for that locationId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        APIResponse DeleteLocation(string _auth, Guid locationId);

        /// <summary>
        /// Get Location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        Task<Location> GetLocation(Guid locationId);
    }
}
