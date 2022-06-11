using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.SetupConfiguration.Level;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class LevelRepository : APIBaseRepo, ILevelRepository
    {
        private readonly APIDBContext _dbContext;
        private ILoggerManager _logManager;

        public LevelRepository(APIDBContext dbContext,
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<Level>>> GetLevels()
        {
            var response = new APIResponse<IEnumerable<Level>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var level = await _dbContext.Levels.Where(l => l.IsEnabled == true).ToListAsync();
                return response = new APIResponse<IEnumerable<Level>>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = level
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                response.Message = "Something went wrong!";
                response.Status = "Internal Server Error";
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ModelError = GetStackError(ex.InnerException);
            }

            return response;
        }

        ///<inheritdoc/>
        public APIResponse AddOrEditLevel(string _auth, Level level)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::LevelRepository::AddOrEditLevel --");
            _logManager.LogDebugObject(level);

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

                var levels = _dbContext.Levels.Where(l => l.IsEnabled == true);
                var isLevelExisting = levels.Where(e => e.LevelId == level.LevelId).FirstOrDefault();
                var GuidId = Guid.NewGuid();
                var TodaysDate = DateTime.Now;
                if (isLevelExisting == null)
                {
                    if (!levels.Where(e => e.Name == level.Name).Any())
                    {
                        var newLevel = new Level
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
                            LevelId = GuidId,
                            Name = level.Name,
                        };

                        _dbContext.Levels.Add(newLevel);
                        _dbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New level added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate level name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isLevelExisting.LevelId.ToString()))
                {
                    if(levels.Where(l => l.Name == level.Name).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate level name found.",
                            Status = "Failed!",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                    isLevelExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isLevelExisting.LastEditedDate = TodaysDate;
                    isLevelExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isLevelExisting.CreatedDate = TodaysDate;
                    isLevelExisting.IsEnabled = true;
                    isLevelExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isLevelExisting.DateEnabled = TodaysDate;
                    isLevelExisting.IsLocked = false;
                    isLevelExisting.LockedDateTime = TodaysDate;
                    isLevelExisting.Name = level.Name;

                    _dbContext.Levels.Update(isLevelExisting);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated level successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate level name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::LevelRepository::AddOrEditLevel--");
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
        public APIResponse DeleteLevel(string _auth, Guid levelId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::LevelRepository::DeleteLevel --");
            _logManager.LogDebugObject(levelId);

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

                var level = _dbContext.Levels.Where(s => s.LevelId == levelId).FirstOrDefault();
                if (level != null)
                {
                    level.LastEditedBy = IsUserLoggedIn.AdminId;
                    level.LastEditedDate = DateTime.Now;
                    level.IsEnabled = false;

                    _dbContext.Levels.Update(level);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {level.Name} Gym.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Level Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::LevelRepository::DeleteLevel--");
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
