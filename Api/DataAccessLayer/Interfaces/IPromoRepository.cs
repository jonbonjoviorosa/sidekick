using Sidekick.Model;
using Sidekick.Model.Promo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IPromoRepository
    {
        /// <summary>
        /// Gets All Promos
        /// </summary>
        /// <returns></returns>
        Task<APIResponse<IEnumerable<Promo>>> GetPromos();
        /// <summary>
        /// Adds or Edits a Promo
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="promo"></param>
        /// <returns></returns>
        Task<APIResponse> AddOrEditPromo(string _auth, Promo promo);
        // <summary>
        /// Set the record IsEnabled = false for that promoId
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="promoId"></param>
        /// <returns></returns>
        APIResponse DeletePromo(string _auth, Guid promoId);
        /// <summary>
        /// Get Promo by Id
        /// </summary>
        /// <param name="_auth"></param>
        /// <param name="promoId"></param>
        /// <returns></returns>
        APIResponse GetPromoById( Guid promoId);
    }
}
