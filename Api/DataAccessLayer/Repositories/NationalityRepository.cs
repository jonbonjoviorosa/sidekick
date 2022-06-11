using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Nationality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class NationalityRepository: INationalityRepository
    {
        private readonly APIDBContext context;
        private readonly ILoggerManager loggerManager;

        public NationalityRepository(APIDBContext context, 
            ILoggerManager loggerManager)
        {
            this.context = context;
            this.loggerManager = loggerManager;
        }

        public async Task<APIResponse<List<Nationality>>> GetNationalities()
        {
            APIResponse<List<Nationality>> apiResp = new APIResponse<List<Nationality>>();

            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var nationalities = await context.Nationalities.ToListAsync();
                return apiResp = new APIResponse<List<Nationality>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = nationalities
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<List<Nationality>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<Nationality> GetNationality(Guid nationalityId)
        {
            return await context.Nationalities
                .FirstOrDefaultAsync(x => x.NationalityId == nationalityId);
        }
    }
}
