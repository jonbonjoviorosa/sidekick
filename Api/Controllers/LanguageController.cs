using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Model;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageRepository languageRepository;

        public LanguageController(ILanguageRepository languageRepository)
        {
            this.languageRepository = languageRepository;
        }
        
        [HttpGet("Get")]
        public async Task<ActionResult<APIResponse<IEnumerable<Language>>>> GetLanguages()
        {
            return Ok(await languageRepository.GetLanguages());
        }
    }
}
