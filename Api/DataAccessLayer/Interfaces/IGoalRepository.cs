using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Goals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IGoalRepository
    {
        /// <summary>
        ///  Gets All Goals
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<Goal>>> GetGoals();
        /// <summary>
        ///  Adds or Edits a Goal
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        APIResponse AddOrEditGoal(string _auth, Goal goal);
        /// <summary>
        /// Set the record IsEnabled = false for that gymId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="goalId"></param>
        /// <returns></returns>
        APIResponse DeleteGoal(string _auth, Guid goalId);
    }
}
