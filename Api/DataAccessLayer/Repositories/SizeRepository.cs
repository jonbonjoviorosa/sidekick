using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Size;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class SizeRepository : APIBaseRepo, ISizeRepository
    {
        readonly APIDBContext _dbContext;
        ILoggerManager _logManager { get; }

        public SizeRepository(APIDBContext dbContext,
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        ///<inheritdoc/>
        public async Task<APIResponse<IEnumerable<TeamSize>>> GetAllSizes()
        {
            var response = new APIResponse<IEnumerable<TeamSize>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);

            try
            {
                var size = await _dbContext.Sizes.Where(s => s.IsEnabled == true).ToListAsync();
                return response = new APIResponse<IEnumerable<TeamSize>>
                {
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = size
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
        public APIResponse AddOrEditTeamSize(string _auth, TeamSize size)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::SizeRepository::AddOrEditTeamSize --");
            _logManager.LogDebugObject(size);

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
                
                var teamSizeList = _dbContext.Sizes.Where(s => s.IsEnabled == true);
                var isTeamSizeExisting = teamSizeList.Where(e => e.SizeId == size.SizeId).FirstOrDefault();
                var todaysDate = DateTime.Now;
                if (isTeamSizeExisting == null)
                {
                    if (!teamSizeList.Where(e => e.SizeName.ToLower() == size.SizeName.ToLower()).Any())
                    {
                        var newTeamSize = new TeamSize
                        {
                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = todaysDate,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = todaysDate,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = todaysDate,
                            IsLocked = false,
                            LockedDateTime = todaysDate,
                            SizeId = Guid.NewGuid(),
                            SizeName = size.SizeName,
                        };

                        _dbContext.Sizes.Add(newTeamSize);
                        _dbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New Team Size added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate team size found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isTeamSizeExisting.SizeId.ToString()))
                {
                    if(teamSizeList.Where(s => s.SizeName.ToLower() == size.SizeName.ToLower()).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate team size found.",
                            Status = "Failed!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    isTeamSizeExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isTeamSizeExisting.LastEditedDate = todaysDate;
                    isTeamSizeExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isTeamSizeExisting.CreatedDate = todaysDate;
                    isTeamSizeExisting.IsEnabled = true;
                    isTeamSizeExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isTeamSizeExisting.DateEnabled = todaysDate;
                    isTeamSizeExisting.IsLocked = false;
                    isTeamSizeExisting.LockedDateTime = todaysDate;
                    isTeamSizeExisting.SizeName = size.SizeName;


                    _dbContext.Sizes.Update(isTeamSizeExisting);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated Team Size successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate Team size found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::SizeRepository::AddOrEditTeamSize--");
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
        public APIResponse DeleteSize(string _auth, Guid sizeId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::SurfaceRepository::DeleteSize --");
            _logManager.LogDebugObject(sizeId);

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

                var size = _dbContext.Sizes.Where(s => s.SizeId == sizeId).FirstOrDefault();
                if (size != null)
                {
                    size.LastEditedBy = IsUserLoggedIn.AdminId;
                    size.LastEditedDate = DateTime.Now;
                    size.IsEnabled = false;

                    _dbContext.Sizes.Update(size);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {size.SizeName} Team Size.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Team Size Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::SurfaceRepository::DeleteSize--");
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
