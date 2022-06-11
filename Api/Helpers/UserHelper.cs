using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using System;
using System.Linq;

namespace Sidekick.Api.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly APIDBContext context;

        public UserHelper(IHttpContextAccessor httpContextAccessor,
            APIDBContext context)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.context = context;
        }

        public Guid GetCurrentUserGuidLogin()
        {
            // copied from APIBaseRepo
            // still need to reconstruct this one
            // there's a better implementation about this
            // we can get it from claims
            // so that we don't need to connect in the database
            var auth = httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (auth != null)
            {
                var token = auth.Split(' ')[1];
                // we need to reconstruct this one!!!
                Guid updater = Guid.Empty;
                UserLoginTransaction UserUpdatingTheProfile = context.UserLoginTransactions.FirstOrDefault(u => u.Token == token);
                if (UserUpdatingTheProfile != null)
                {
                    return UserUpdatingTheProfile.UserId;
                };
                AdminLoginTransaction AdminUpdatingTheProfile = context.AdminLoginTransactions.FirstOrDefault(a => a.Token == token);
                if (AdminUpdatingTheProfile != null)
                {
                    return AdminUpdatingTheProfile.AdminId;
                }

                if (updater == Guid.Empty)
                {
                    throw new Exception("Unauthorized");
                }
                return updater;
            }
            throw new Exception("Unauthorized");
        }


        // need better implementation about this!!!
        // we should get it from claims
        public int GetCurrentUserIdLogin()
        {
            var auth = httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (auth != null)
            {
                var token = auth.Split(' ')[1];
                // we need to reconstruct this one!!!
                int updater = 0;
                UserLoginTransaction UserUpdatingTheProfile = context.UserLoginTransactions.FirstOrDefault(u => u.Token == token);
                if (UserUpdatingTheProfile != null) { return UserUpdatingTheProfile.Id; };
                AdminLoginTransaction AdminUpdatingTheProfile = context.AdminLoginTransactions.FirstOrDefault(a => a.Token == token);
                if (AdminUpdatingTheProfile != null) { return AdminUpdatingTheProfile.Id; }
                return updater;
            }
            throw new Exception("Unauthorized!");
        }
    }
}
