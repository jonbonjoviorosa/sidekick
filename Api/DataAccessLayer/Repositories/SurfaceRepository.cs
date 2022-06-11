using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class SurfaceRepository : APIBaseRepo, ISurfaceRepository
    {
        readonly APIDBContext _dbContext;
        private APIConfigurationManager _apiConfig { get; set; }
        ILoggerManager _logManager { get; }

        public SurfaceRepository(APIDBContext dbContext,
            ILoggerManager logManager,
            APIConfigurationManager apiConfig)
        {
            _dbContext = dbContext;
            _logManager = logManager;
            _apiConfig = apiConfig;
        }

        ///<inheritdoc/>
        public async Task<APIResponse<List<Surface>>> GetAllSurface()
        {
            var response = new APIResponse<List<Surface>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);

            try
            {
                var surfaces = await _dbContext.Surfaces.Where(s => s.IsEnabled == true).ToListAsync();
                return response = new APIResponse<List<Surface>>
                {
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = surfaces
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                response.Message = "Something went wrong!";
                response.Status = "Internal Server Error";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.ModelError = GetStackError(ex.InnerException);
            }

            return response;
        }

        ///<inheritdoc/>
        public APIResponse AddOrEditSurface(string _auth, Surface surface)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::SurfaceRepository::AddOrEditSurface --");
            _logManager.LogDebugObject(surface);

            try
            {
                var IsUserLoggedIn = _dbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                var surfaces = _dbContext.Surfaces.Where(s => s.IsEnabled == true);
                var isSurfaceExisting = surfaces.Where(e => e.SurfaceId == surface.SurfaceId).FirstOrDefault();
                var GuidId = Guid.NewGuid();
                var TodaysDate = DateTime.Now;
                if (isSurfaceExisting == null)
                {
                    if (!surfaces.Where(e => e.Name.ToLower() == surface.Name.ToLower()).Any())
                    {
                        var newSurface = new Surface
                        {
                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = TodaysDate,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = TodaysDate,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = TodaysDate,
                            IsLocked = false,
                            LockedDateTime = TodaysDate,
                            SurfaceId = GuidId,
                            Name = surface.Name,
                        };

                        _dbContext.Surfaces.Add(newSurface);
                        _dbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New surface added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }

                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate surface name found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isSurfaceExisting.SurfaceId.ToString()))
                {
                    if(surfaces.Where(s => s.Name.ToLower() == surface.Name.ToLower() && s.IsEnabled == true).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate surface name found.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    isSurfaceExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isSurfaceExisting.LastEditedDate = TodaysDate;
                    isSurfaceExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isSurfaceExisting.CreatedDate = TodaysDate;
                    isSurfaceExisting.IsEnabled = true;
                    isSurfaceExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isSurfaceExisting.DateEnabled = TodaysDate;
                    isSurfaceExisting.IsLocked = false;
                    isSurfaceExisting.LockedDateTime = TodaysDate;
                    isSurfaceExisting.Name = surface.Name;

                    _dbContext.Surfaces.Update(isSurfaceExisting);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated surface successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate surface name found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::SurfaceRepository::AddOrEditSurface--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        ///<inheritdoc/>
        public APIResponse DeleteSurface(string _auth, Guid surfaceId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::SurfaceRepository::DeleteSurface --");
            _logManager.LogDebugObject(surfaceId);

            try
            {
                var IsUserLoggedIn = _dbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.Unauthorized
                    };
                }

                var surface = _dbContext.Surfaces.Where(s => s.SurfaceId == surfaceId).FirstOrDefault();
                if (surface != null)
                {
                    surface.LastEditedBy = IsUserLoggedIn.AdminId;
                    surface.LastEditedDate = DateTime.Now;
                    surface.IsEnabled = false;

                    _dbContext.Surfaces.Update(surface);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {surface.Name} surface.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Surface Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::SurfaceRepository::DeleteSurface--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }


            return apiResp;
        }

    }
}
