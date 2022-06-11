using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class BannerRepository : APIBaseRepo, IBannerRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public BannerRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse Add(string _auth, BannerDto _banner)
        {
            APIResponse apiResp = new APIResponse();
            DateTime TodaysDate = DateTime.Now;
            DateTime Date = DateTime.Now.Date;
            LogManager.LogInfo("-- Run::BannerRepository::Add --");
            LogManager.LogDebugObject(_banner);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                Banner hasActive = DbContext.Banners.AsNoTracking().Where(c => c.FacilityId == _banner.FacilityId && c.IsEnabled == true &&
                c.StartDate >= Date && c.EndDate >= Date).FirstOrDefault();

                if (hasActive != null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Existing Banner found. Please Deactivate the current banner to proceed.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Conflict
                    };
                }

                Banner hasDuplicate = DbContext.Banners.AsNoTracking().Where(c => c.BannerId == _banner.BannerId).FirstOrDefault();
                if (hasDuplicate == null) //add new
                {
                    Guid GuidId = Guid.NewGuid();

                    //DateTime StartDate = new DateTime(_banner.StartDate.Year, _banner.StartDate.Month, _banner.StartDate.Day, 00, 00, 00);
                    //DateTime EndDate = new DateTime(_banner.EndDate.Year, _banner.EndDate.Month, _banner.EndDate.Day, 23, 59, 59);

                    Banner banners = new()
                    {
                        LastEditedBy = IsUserLoggedIn.AdminId,
                        LastEditedDate = TodaysDate,
                        CreatedBy = IsUserLoggedIn.AdminId,
                        CreatedDate = TodaysDate,
                        IsEnabled = _banner.IsActive,
                        IsEnabledBy = IsUserLoggedIn.AdminId,
                        DateEnabled = TodaysDate,
                        IsLocked = false,
                        LockedDateTime = TodaysDate,

                        BannerId = GuidId,
                        FacilityId = _banner.FacilityId,
                        Title = _banner.Title,
                        StartDate = _banner.StartDate,
                        EndDate = _banner.EndDate,
                        ImageUrl = _banner.ImageUrl
                    };

                    DbContext.Banners.Add(banners);
                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "New Banner added successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Duplicate record found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::BannerRepository::Add --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;

        }

        public APIResponse Edit(string _auth, BannerDto _banner)
        {
            APIResponse apiResp = new APIResponse();
            DateTime TodaysDate = DateTime.Now;
            LogManager.LogInfo("-- Run::BannerRepository::Add --");
            LogManager.LogDebugObject(_banner);

            try
            {
                AdminLoginTransaction IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                Banner foundRecord = DbContext.Banners.AsNoTracking().FirstOrDefault(x => x.BannerId == _banner.BannerId);
                if (foundRecord != null)
                {
                    foundRecord.LastEditedBy = IsUserLoggedIn.AdminId;
                    foundRecord.LastEditedDate = TodaysDate;
                    foundRecord.IsEnabled = _banner.IsActive;
                    foundRecord.IsEnabledBy = IsUserLoggedIn.AdminId;
                    foundRecord.Title = _banner.Title;
                    foundRecord.StartDate = _banner.StartDate;
                    foundRecord.EndDate = _banner.EndDate;
                    foundRecord.ImageUrl = _banner.ImageUrl;
                    foundRecord.FacilityId = _banner.FacilityId;

                    DbContext.Banners.Update(foundRecord);
                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Banner updated successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::BannerRepository::Edit --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }
        
        public APIResponse List()
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::BannerRepository::List --");

            try
            {
                IEnumerable<BannerList> bannerList = null;
                bannerList = DbContext.Facilities.AsNoTracking()
                              .Join(DbContext.Banners.AsNoTracking(), f => f.FacilityId, b => b.FacilityId, (f, b) => new { f, b })
                              .Select(i => new BannerList
                                     {
                                         BannerId = i.b.BannerId,
                                         FacilityId = i.b.FacilityId,
                                         FacilityName = i.f.Name,
                                         Title = i.b.Title,
                                         StartDate = i.b.StartDate,
                                         EndDate = i.b.EndDate,
                                         ImageUrl = i.b.ImageUrl,
                                         IsActive = i.b.IsEnabled ?? false
                                     }).ToList();

                return apiResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = bannerList,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::BannerRepository::List --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse GetBanner(Guid _bannerID)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::BannerRepository::List --");

            try
            {
                Banner banner = DbContext.Banners.AsNoTracking().Where(x => x.BannerId == _bannerID).FirstOrDefault();
                if (banner != null)
                {
                    BannerDto details = new BannerDto
                    {
                        BannerId = banner.BannerId,
                        FacilityId = banner.FacilityId,
                        Title = banner.Title,
                        StartDate = banner.StartDate,
                        EndDate = banner.EndDate,
                        ImageUrl = banner.ImageUrl,
                        IsActive = banner.IsEnabled ?? false
                    };
                    return apiResp = new APIResponse
                    {
                        Message = "Record found.",
                        Status = "Success!",
                        Payload = details,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Record not found.",
                        Status = "Success!",
                        Payload = null,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::BannerRepository::FacilityBanner --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
            }

        public APIResponse FacilityBanner(Guid _facilityID)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::BannerRepository::FacilityBanner --" + _facilityID);

            try
            {
                IEnumerable<BannerDto> bannerList = null;
                bannerList = DbContext.Banners.AsNoTracking().Where(x => x.FacilityId == _facilityID).Select(i => new BannerDto
                              {
                                  BannerId = i.BannerId,
                                  Title = i.Title,
                                  StartDate = i.StartDate,
                                  EndDate = i.EndDate,
                                  ImageUrl = i.ImageUrl,
                                  IsActive = i.IsEnabled ?? false
                              }).ToList();

                if (bannerList.Any())
                {
                    return apiResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = bannerList,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "No records found.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::BannerRepository::FacilityBanner --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> Delete(Guid _bannerID)
        {
            APIResponse aResp = new();
            try
            {
                Banner banner = null;
                banner = DbContext.Banners.AsNoTracking().Where(w => w.BannerId == _bannerID).First();
                DbContext.Banners.RemoveRange(banner);
                await DbContext.SaveChangesAsync();
                return aResp = new APIResponse
                {
                    Message = "Banner Successfully Deleted",
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::BannerRepository::Delete --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
    }
}
