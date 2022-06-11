using Sidekick.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace Sidekick.Api.Helpers
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder, string _hostUrl)
        {
            DateTime CreatedDate = DateTime.Now;
            var DestinationFolderName = Path.Combine("Resources", "Icons");
            string hostURL = _hostUrl;
            var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);
            string ShopCatDefaultLogo = hostURL + "resources/Icons/" + "Shop_Category_Default_Logo.jpg";

            var adminId = 1;
            var adminGuid = Guid.NewGuid();

            #region Sidekick Superadmin
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = adminId,
                    AdminId = adminGuid,
                    AccountType = EAccountType.SUPERADMINUSER,
                    AdminType = EAdminType.SUPERADMIN,
                    Email = "superadmin@sidekick.com",
                    MobileNumber = "0123456789",
                    Password = "IrmilSvThJwq6JxntQsPBxHHbOr6YIhOkxAQu1RrNo4=",
                    FirstName = "Super Admin",
                    LastName = "Administrator",
                    LastEditedBy = adminGuid,
                    LastEditedDate = CreatedDate,
                    CreatedBy = adminGuid,
                    CreatedDate = CreatedDate,
                    IsEnabledBy = adminGuid,
                    IsEnabled = true,
                    DateEnabled = CreatedDate,
                    IsLocked = false,
                    LockedDateTime = null
                }
            );
            #endregion

            #region Facility Superadmin
            modelBuilder.Entity<FacilityUser>().HasData(
                new FacilityUser
                {
                    Id = adminId,
                    FacilityUserId = adminGuid,
                    FacilityId = adminGuid,
                    FacilityAccountType = EAccountType.FACILITYUSER,
                    FacilityUserType = EFacilityUserType.ADMIN,
                    Email = "facilityadmin@sidekick.com",
                    MobileNumber = "0123456789",
                    Password = "IrmilSvThJwq6JxntQsPBxHHbOr6YIhOkxAQu1RrNo4=",
                    FirstName = "Facility Admin",
                    LastName = "Administrator",
                    DevicePlatform = EDevicePlatform.Web,
                    LastEditedBy = adminGuid,
                    LastEditedDate = CreatedDate,
                    CreatedBy = adminGuid,
                    CreatedDate = CreatedDate,
                    IsEnabledBy = adminGuid,
                    IsEnabled = true,
                    DateEnabled = CreatedDate,
                    IsLocked = false,
                    LockedDateTime = null
                }
            );
            #endregion

        }
    }
}

