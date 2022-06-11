using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.Promo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Promo")]
    public class PromoController : APIBaseController
    {
        private IPromoRepository _promoRepository { get; }
        public PromoController(IPromoRepository promoRepository)
        {
            _promoRepository = promoRepository;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Promo>>>> GetPromos()
        {
            return Ok(await _promoRepository.GetPromos());
        }

        [HttpGet("Promo/{promoId}")]
        public IActionResult GetPromoById(Guid promoId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_promoRepository.GetPromoById(promoId));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }

        [HttpPost("AddOrEdit")]
        public async Task<IActionResult> AddOrEditPromo([FromHeader] string Authorization, [FromBody] Promo promo)
        {
            if (ModelState.IsValid)
            {
                return Ok(await _promoRepository.AddOrEditPromo(Authorization.Split(' ')[1], promo));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }

        [HttpPost("Delete/{promoId}")]
        public IActionResult DeletePromo([FromHeader] string Authorization, [FromBody] Guid promoId)
        {
            if (ModelState.IsValid)
            {
                return Ok(_promoRepository.DeletePromo(Authorization.Split(' ')[1], promoId));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }
    }
}
