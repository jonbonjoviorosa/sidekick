using Sidekick.Model;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface ILanguageRepository
    {
        Task<APIResponse<IEnumerable<Language>>> GetLanguages();
    }
}
