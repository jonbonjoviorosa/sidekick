using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface ISendEmailHandler
    {
        Task<APIResponse> SendEmailToAdmin(string body);
    }
}
