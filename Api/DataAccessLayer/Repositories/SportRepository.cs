
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class SportRepository : APIBaseRepo, ISportRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public SportRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public async Task<APIResponse<List<Sport>>> GetSports()
        {
            var apiResp = new APIResponse<List<Sport>>();

            LogManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {
                var sports = await DbContext.Sports.Where(s => s.IsEnabled == true).ToListAsync();
                return apiResp = new APIResponse<List<Sport>>
                {
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = sports
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse AddSport(string _auth, SportDto _sport)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::SportRepository::AddSport --");
            LogManager.LogDebugObject(_sport);

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

                Sport sport = DbContext.Sports.Where(u => u.Name.ToLower() == _sport.Name.ToLower() && u.IsEnabled == true).FirstOrDefault();
                if (sport == null)
                {
                    Guid GuidId = Guid.NewGuid();
                    DateTime TodaysDate = DateTime.Now;

                    Sport newSport = new Sport
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
                        MaxPlayers = _sport.MaxPlayers,
                        MaxPrice = _sport.MaxPrice,

                        SportId = GuidId,
                        Name = _sport.Name,
                        Icon = _sport.Icon,
                        Description = _sport.Description
                    };

                    DbContext.Sports.Add(newSport);
                    DbContext.SaveChanges();

                    return apiResp = new APIResponse
                    {
                        Message = "New sport added successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Creation Failed. Duplicate sport name found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::SportRepository::AddSport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse EditSport(string _auth, SportDto _sport)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::SportRepository::EditSport --");
            LogManager.LogDebugObject(_sport);

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

                var sports = DbContext.Sports.Where(s => s.IsEnabled == true);
                var existingSport = sports.Where(u => u.SportId == _sport.SportId).FirstOrDefault();
                if (existingSport != null)
                {
                    //validate if sport has the same name
                    if (sports.Where(x => x.Name.ToLower() == _sport.Name.ToLower()).Any() && _sport.SportId != existingSport.SportId)
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Processing Failed. Duplicate record found.",
                            Status = "Failed!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    else if (existingSport.SportId == _sport.SportId)
                    {
                        if (sports.Where(s => s.Name.ToLower() == _sport.Name.ToLower() && _sport.SportId != s.SportId && s.IsEnabled == true).Any())
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "Processing Failed. Duplicate record found.",
                                Status = "Failed!",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                        DateTime DateUpdated = DateTime.Now;

                        existingSport.LastEditedBy = IsUserLoggedIn.AdminId;
                        existingSport.LastEditedDate = DateUpdated;
                        existingSport.IsEnabledBy = IsUserLoggedIn.AdminId;
                        existingSport.DateEnabled = DateUpdated;

                        existingSport.Name = _sport.Name;
                        existingSport.MaxPrice = _sport.MaxPrice;
                        existingSport.MaxPlayers = _sport.MaxPlayers;
                        existingSport.Description = _sport.Description;
                        existingSport.Icon = _sport.Icon;

                        DbContext.Sports.Update(existingSport);
                        DbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "Record updated successfully.",
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
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Processing Failed. Record not found.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (SqlException ex)
            {
                LogManager.LogInfo("-- Error::SportRepository::EditSport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something Went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse GetSport(int _sportId)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::SportRepository::GetSport --");
            LogManager.LogDebugObject(_sportId);

            try
            {
                if (_sportId == 0)
                {
                    IEnumerable<SportDto> sports = null;
                    sports = DbContext.Sports.Where(s => s.IsEnabled == true).AsNoTracking()
                        .Select(i => new SportDto
                        {
                            SportId = i.SportId,
                            Name = i.Name,
                            Description = i.Description,
                            Icon = i.Icon,
                            IsEnabled = i.IsEnabled
                        }).ToList();

                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = sports,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return aResp = new APIResponse
                    {
                        Message = "Record found.",
                        Status = "Success!",
                        Payload = DbContext.Sports.AsNoTracking().FirstOrDefault(u => u.Id == _sportId),
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::SportRepository::GetSport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }


        public APIResponse DeleteSport(string _auth, Guid sportId)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::SportRepository::DeleteSport --");
            LogManager.LogDebugObject(sportId);

            try
            {
                var IsUserLoggedIn = DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return aResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }
                var sport = DbContext.Sports.Where(s => s.SportId == sportId).FirstOrDefault();
                if (sport != null)
                {
                    sport.LastEditedBy = IsUserLoggedIn.AdminId;
                    sport.LastEditedDate = DateTime.Now;
                    sport.IsEnabled = false;

                    DbContext.Sports.Update(sport);
                    DbContext.SaveChanges();

                    return aResp = new APIResponse
                    {
                        Message = $"Successfully deleted {sport.Name} sport.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                return aResp = new APIResponse
                {
                    Message = "Error Deleting Sport.",
                    Status = "Failed!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::SportRepository::DeleteSport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<Sport> GetSport(Guid sportId)
        {
            return await DbContext.Sports.FirstOrDefaultAsync(x => x.SportId == sportId);
        }

    }
}
