using Microsoft.AspNetCore.Http;
using Sidekick.Admin.DataLayer;
using Sidekick.Admin.Helpers;
using Sidekick.Model;

namespace Sidekick.Admin.Controllers
{
    public class UsersController : BaseController
    {
        private IMainHttpClient UsersHttpClient { get; }
        public AdminUserContext AdminUCtxt { get; set; }
        private ConfigMaster MConf { get; }
        private readonly IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public UsersController(IMainHttpClient _mhttpc, IHttpContextAccessor httpContextAccessor, ConfigMaster _conf)
        {
            UsersHttpClient = _mhttpc;
            _httpCtxtAcc = httpContextAccessor;
            MConf = _conf;
            AdminUCtxt = _session.GetObjectFromJson<AdminUserContext>("adminUserContext");
        }
    }
}
