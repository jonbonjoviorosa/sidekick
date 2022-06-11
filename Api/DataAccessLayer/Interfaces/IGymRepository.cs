using Sidekick.Model;
using Sidekick.Model.Gym;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IGymRepository
    {
        /// <summary>
        /// Gets All Gym
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<Gym>>> GetGyms();
        /// <summary>
        /// Adds or Edit a Gym
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="GYM"></param>
        /// <returns></returns>
        APIResponse AddOrEditGym(string _auth, Gym gym);
        /// <summary>
        /// Set the record IsEnabled = false for that gymId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="gymId"></param>
        /// <returns></returns>
        APIResponse DeleteGym(string _auth, Guid gymId);

        /// <summary>
        /// Get gym details
        /// </summary>
        /// <param name="GymId"></param>
        /// <returns></returns>
        Task<Gym> GetGym(Guid GymId);
    }
}
