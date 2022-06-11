using Sidekick.Model;
using Sidekick.Model.Nationality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface INationalityRepository
    {
        Task<APIResponse<List<Nationality>>> GetNationalities();
        Task<Nationality> GetNationality(Guid nationalityId);
    }
}
