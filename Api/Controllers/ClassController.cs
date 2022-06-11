using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassHandler classHandler;

        public ClassController(IClassHandler classHandler)
        {
            this.classHandler = classHandler;
        }

        [HttpGet("Individual")]
        public async Task<ActionResult<APIResponse<IEnumerable<IndividualClassViewModel>>>> GetIndividualClasses()
        {
            return Ok(await classHandler.GetIndividualClasses());
        }

        [HttpGet("Individual/{classId}")]
        public async Task<ActionResult<APIResponse<IndividualClassViewModel>>> GetIndividualClass(Guid classId)
        {
            return Ok(await classHandler.GetIndividualClass(classId));
        }

        [HttpGet("GetCoachingClass/{classId}")]
        public async Task<ActionResult<APIResponse>> GetCoachingClass(Guid classId)
        {
            var response = await classHandler.GetCoachingClass(classId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("Individual/{userId}/Coach")]
        public async Task<ActionResult<APIResponse<IndividualClassViewModel>>> GetCoachIndividualClass(Guid userId)
        {
            return Ok(await classHandler.GetCoachIndividualClass(userId));
        }

        [HttpPost("Individual/ByFilter")]
        public async Task<ActionResult<APIResponse<IEnumerable<IndividualClassByFilterViewModel>>>> GetIndividualClassesByFilter([FromBody]IEnumerable<FilterViewModel> filter)
        {
            return Ok(await classHandler.GetIndividualClassesByFilter(filter));
        }

        [HttpGet("Individual/UserView/{classId}/BookDate/{bookDate}")]
        public async Task<ActionResult<APIResponse<IndividualClassByFilterViewModel>>> GetIndividualClassByFilter(Guid classId,DateTime bookDate)
        {
            return Ok(await classHandler.GetIndividualClass_UserView(classId,bookDate));
        }

        [HttpPost("Individual")]
        public async Task<ActionResult<APIResponse>> InsertIndividualClass([FromBody] IndividualClassViewModel individualClass)
        {
            return Ok(await classHandler.InsertIndividualClass(individualClass));
        }

        [HttpPost("InsertOrUpdateIndividual")]
        public async Task<ActionResult<APIResponse>> CreateUpdateIndividualClass([FromHeader] string Authorization, [FromBody] ClassRenderViewModel individualClass)
        {
            return Ok(await classHandler.CreateUpdateIndividualClass(Authorization.Split(" ")[1], individualClass));
        }

        [HttpGet("Group/{classId}")]
        public async Task<ActionResult<APIResponse<GroupClassViewModel>>> GetGroupClass(Guid classId)
        {
            return Ok(await classHandler.GetGroupClass(classId));
        }

        [HttpGet("Group/{userId}/Coach")]
        public async Task<ActionResult<APIResponse<IEnumerable<GroupClassViewModel>>>> GetGroupClassesPerCoach(Guid userId)
        {
            return Ok(await classHandler.GetGroupClassesPerCoach(userId));
        }

        [HttpGet("GetAllGroupClasses")]
        public async Task<ActionResult> GetAllGroupClasses()
        {
            return Ok(await classHandler.GetAllGroupClasses());
        }

        [HttpPost("Group")]
        public async Task<ActionResult> InsertUpdateGroupClass([FromBody] GroupClassViewModel groupClass)
        {
            return Ok(await classHandler.InsertUpdateGroupClass(groupClass));
        }

        [HttpPost("Group/ByFilter")]
        public async Task<ActionResult<APIResponse<IEnumerable<GroupClassByFilterViewModel>>>> GetGroupClassesByFilter([FromBody]IEnumerable<FilterViewModel> filter)
        {
            return Ok(await classHandler.GetGroupClassByFilter(filter));
        }

        [HttpGet("Group/UserView/{classId}")]
        public async Task<ActionResult<APIResponse<GroupClassByFilterViewModel>>> GetGroupClassByFilter(Guid classId)
        {
            return Ok(await classHandler.GetGroupClass_UserView(classId));
        }

        [HttpGet("AllFilters")]
        public async Task<APIResponse<Filters>> GetAllFilters()
        {
            return await classHandler.GetAllFilters();
        }


        [HttpPost("DeleteClass")]
        public async Task<ActionResult<APIResponse>> DeleteClass([FromBody]ChangeStatus classId)
        {
            var response = await classHandler.DeleteClass(classId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
