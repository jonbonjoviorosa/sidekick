using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;

namespace Sidekick.Api.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/FAQs")]
    public class FAQsController : APIBaseController
    {
        IFAQsRepository FAQsRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public FAQsController(IFAQsRepository _fRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            FAQsRepo = _fRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("Add")]
        public IActionResult Add([FromHeader] string Authorization, [FromBody] FAQsDto _fAQ)
        {
            if (ModelState.IsValid)
            {
                return Ok(FAQsRepo.Add(Authorization.Split(' ')[1], _fAQ));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [HttpPost("Edit")]
        public IActionResult Edit([FromHeader] string Authorization, [FromBody] FAQsDto _fAQ)
        {

            if (ModelState.IsValid)
            {
                return Ok(FAQsRepo.Edit(Authorization.Split(' ')[1], _fAQ));
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("List")]
        public IActionResult List(Guid? _fAQsId = null)
        {
            APIResponse apiResp = FAQsRepo.List(_fAQsId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("Status")]
        public IActionResult Status([FromHeader] string Authorization, [FromBody] FAQStatus _fAQ)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = FAQsRepo.Status(Authorization.Split(' ')[1], _fAQ);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Model Objet Invalid",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("LegalDocument/{_type}")]
        public IActionResult ViewLegalDoc(ELegalDocType _type)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = FAQsRepo.ViewLegalDoc(_type);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Model Object Invalid",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("AddEditLegalDocument")]
        public IActionResult EditLegaDocument([FromHeader] string Authorization, [FromBody] LegalDocumentDto _legal)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = FAQsRepo.AddEditLegalDoc(Authorization.Split(' ')[1], _legal);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Model Objet Invalid",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

    }
}
