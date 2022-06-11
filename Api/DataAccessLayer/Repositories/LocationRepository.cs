using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class LocationRepository : APIBaseRepo, ILocationRepository
    {
        private readonly APIDBContext _dbContext;
        ILoggerManager _logManager { get; }
        public LocationRepository(APIDBContext dbContext, 
            ILoggerManager logManager)
        {
            _dbContext = dbContext;
            _logManager = logManager;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<Location>>> GetLocations()
        {
            var response = new APIResponse<IEnumerable<Location>>();

            _logManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var location = await _dbContext.Locations.Where(l => l.IsEnabled == true).ToListAsync();
                return response = new APIResponse<IEnumerable<Location>>
                {
                    Status = "Success!",
                    StatusCode = HttpStatusCode.OK,
                    Payload = location
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
        public APIResponse AddOrEditLocation(string _auth, Location location)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::LocationRepository::AddOrEditLocation --");
            _logManager.LogDebugObject(location);

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

                var locations = _dbContext.Locations;
                var isLocationExisting = locations.Where(e => e.LocationId == location.LocationId && e.IsEnabled == true).FirstOrDefault();
                var GuidId = Guid.NewGuid();
                var TodaysDate = DateTime.Now;
                if (isLocationExisting == null)
                {
                    if (!locations.Where(e => e.Name.ToLower() == location.Name.ToLower() && e.IsEnabled == true).Any())
                    {
                        var newLocation = new Location
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
                            LocationId = GuidId,
                            Name = location.Name,
                        };

                        _dbContext.Locations.Add(newLocation);
                        _dbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "New location  added successfully.",
                            Status = "Success!",
                            StatusCode = HttpStatusCode.OK
                        };
                    }
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate location name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                else if (!string.IsNullOrWhiteSpace(isLocationExisting.LocationId.ToString()))
                {
                    if(locations.Where(l => l.Name.ToLower() == location.Name.ToLower() && l.IsEnabled == true).Any())
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Creation Failed. Duplicate location name found.",
                            Status = "Failed!",
                            StatusCode = HttpStatusCode.BadRequest
                        };
                    }
                    isLocationExisting.LastEditedBy = IsUserLoggedIn.AdminId;
                    isLocationExisting.LastEditedDate = TodaysDate;
                    isLocationExisting.CreatedBy = IsUserLoggedIn.AdminId;
                    isLocationExisting.CreatedDate = TodaysDate;
                    isLocationExisting.IsEnabled = true;
                    isLocationExisting.IsEnabledBy = IsUserLoggedIn.AdminId;
                    isLocationExisting.DateEnabled = TodaysDate;
                    isLocationExisting.IsLocked = false;
                    isLocationExisting.LockedDateTime = TodaysDate;
                    isLocationExisting.Name = location.Name;

                    _dbContext.Locations.Update(isLocationExisting);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "Updated location successfully.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate surface name found.",
                        Status = "Failed!",
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::LocationRepository::AddOrEditLocation--");
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
        public APIResponse DeleteLocation(string _auth, Guid locationId)
        {
            var apiResp = new APIResponse();

            _logManager.LogInfo("-- Run::LocationRepository::DeleteLocation --");
            _logManager.LogDebugObject(locationId);

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

                var location = _dbContext.Locations.Where(s => s.LocationId == locationId).FirstOrDefault();
                if (location != null)
                {
                    location.LastEditedBy = IsUserLoggedIn.AdminId;
                    location.LastEditedDate = DateTime.Now;
                    location.IsEnabled = false;

                    _dbContext.Locations.Update(location);
                    _dbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = $"Successfully deleted {location.Name} Location.",
                        Status = "Success!",
                        StatusCode = HttpStatusCode.OK
                    };
                }

                return apiResp = new APIResponse
                {
                    Message = "Error Deleting Location Record",
                    Status = "Failed!",
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                _logManager.LogInfo("-- Error::LocationRepository::DeleteLocation--");
                _logManager.LogError(ex.InnerException.Message);
                _logManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }
        public async Task<Location> GetLocation(Guid locationId)
        {
            return await _dbContext.Locations
                .FirstOrDefaultAsync(x => x.LocationId == locationId);
        }
    }
}
