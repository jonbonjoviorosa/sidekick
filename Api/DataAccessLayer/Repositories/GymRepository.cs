using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.Gym;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class GymRepository: APIBaseRepo, IGymRepository
    {
        private readonly APIDBContext _dbContext;
        private readonly ILoggerManager _logManager;

        public GymRepository(APIDBContext dbContext,
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        ///<inheritdoc/>
        public async Task<APIResponse<IEnumerable<Gym>>> GetGyms()
        {
            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var gyms = await _dbContext.Gyms.Where(g => g.IsEnabled == true).ToListAsync();
                return new APIResponse<IEnumerable<Gym>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = gyms
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<Gym>>.ReturnAPIResponse(EResponseAction.Unauthorized);
                _logManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                return APIResponseHelper<IEnumerable<Gym>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        ///<inheritdoc/>
        public APIResponse AddOrEditGym(string _auth, Gym gym)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::GymRepository::AddOrEditGym --");
            _logManager.LogDebugObject(gym);

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

                var gyms = _dbContext.Gyms.Where(g => g.IsEnabled == true);
                var isGymExisting = gyms.Where(e => e.GymId == gym.GymId).FirstOrDefault();
                var GuidId = Guid.NewGuid();
                var TodaysDate = DateTime.Now;
                if (isGymExisting == null)
                {
                    if (!gyms.Where(e => e.GymName.ToLower() == gym.GymName.ToLower()).Any())
                    {
                        var newGym = new Gym
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
                            GymId = GuidId,
                            GymName = gym.GymName,
                            Icon = gym.Icon,
                            GymAddress = gym.GymAddress,
                            GymLat = gym.GymLat,
                            GymLong = gym.GymLong,
                            AreaId = gym.AreaId
                        };

                        _dbContext.Gyms.Add(newGym);
                        _dbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New gym added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate gym name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isGymExisting.GymId.ToString()))
                {
                    if(gyms.Where(g => g.GymName.ToLower() == gym.GymName.ToLower() && g.GymId != gym.GymId).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate gym name found.",
                            Status = "Failed!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    isGymExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isGymExisting.LastEditedDate = TodaysDate;
                    isGymExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isGymExisting.CreatedDate = TodaysDate;
                    isGymExisting.IsEnabled = true;
                    isGymExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isGymExisting.DateEnabled = TodaysDate;
                    isGymExisting.IsLocked = false;
                    isGymExisting.LockedDateTime = TodaysDate;
                    isGymExisting.GymName = gym.GymName;
                    isGymExisting.Icon = gym.Icon;
                    isGymExisting.GymAddress = gym.GymAddress;
                    isGymExisting.GymLat = gym.GymLat;
                    isGymExisting.GymLong = gym.GymLong;
                    isGymExisting.AreaId = gym.AreaId;

                    _dbContext.Gyms.Update(isGymExisting);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated gym successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate gym name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::GymRepository::AddOrEditGym--");
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
        public APIResponse DeleteGym(string _auth, Guid gymId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::GymRepository::DeleteGym --");
            _logManager.LogDebugObject(gymId);

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

                var gym = _dbContext.Gyms.Where(s => s.GymId == gymId).FirstOrDefault();
                if (gym != null)
                {
                    gym.LastEditedBy = IsUserLoggedIn.AdminId;
                    gym.LastEditedDate = DateTime.Now;
                    gym.IsEnabled = false;

                    _dbContext.Gyms.Update(gym);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {gym.GymName} Gym.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Gym Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::GymRepository::DeleteGym--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }
        public async Task<Gym> GetGym(Guid GymId)
        {
            return await _dbContext.Gyms
                .FirstOrDefaultAsync(x => x.GymId == GymId);
        }
    }
}
