using Sidekick.Model;
using Sidekick.Model.Specialty;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ISpecialtyRepository
    {
        /// <summary>
        /// Gets All Specialties
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<Specialty>>> GetSpecialties();
        /// <summary>
        /// Adds or Edits a Specialty
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="specialty"></param>
        /// <returns></returns>
        APIResponse AddOrEditSpecialty(string _auth, Specialty specialty);
        /// <summary>
        /// Set the record IsEnabled = false for that specialtyId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="specialtyId"></param>
        /// <returns></returns>
        APIResponse DeleteSpecialty(string _auth, Guid specialtyId);
    }
}
