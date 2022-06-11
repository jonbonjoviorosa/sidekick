using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class LanguageRepository: ILanguageRepository
    {
        private readonly APIDBContext context;
        private readonly ILoggerManager loggerManager;

        public LanguageRepository(APIDBContext context,
            ILoggerManager loggerManager)
        {
            this.context = context;
            this.loggerManager = loggerManager;
        }

        public async Task<APIResponse<IEnumerable<Language>>> GetLanguages()
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var languages = await context.Languages.ToListAsync();
                return new APIResponse<IEnumerable<Language>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = languages
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<Language>>.ReturnAPIResponse(EResponseAction.Unauthorized);
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<IEnumerable<Language>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
    }
}
