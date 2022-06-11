using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.Specialty;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Specialty")]
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyRepository specialtyRepository;

        public SpecialtyController(ISpecialtyRepository specialtyRepository)
        {
            this.specialtyRepository = specialtyRepository;
        }

        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Specialty>>>> GetSpecialties()
        {
            return Ok(await specialtyRepository.GetSpecialties());
        }

        [HttpPost("AddOrEdit")]
        public IActionResult AddOrEditSpecialty([FromHeader] string Authorization, [FromBody] Specialty specialty)
        {
            if (ModelState.IsValid)
            {
                return Ok(specialtyRepository.AddOrEditSpecialty(Authorization.Split(' ')[1], specialty));
            }

            return BadRequest(new APIResponse
            {
                Message = "Invalid Model Object",
                ModelError = ModelState.Errors(),
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });

        }

        [HttpPost("Delete/{specialtyId}")]
        public IActionResult DeleteSurface([FromHeader] string Authorization, [FromBody] Guid specialtyId)
        {
            if (ModelState.IsValid)
            {
                return Ok(specialtyRepository.DeleteSpecialty(Authorization.Split(' ')[1], specialtyId));
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
