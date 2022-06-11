using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class FacilityPitchTimingRepository : APIBaseRepo, IFacilityPitchTimingRepository
    {
        private readonly APIDBContext _dbContext;
        ILoggerManager _logManager { get; }

        public FacilityPitchTimingRepository(APIDBContext dbContext,
             ILoggerManager LogManager)
        {
            _dbContext = dbContext;
            _logManager = LogManager;
        }

        public async Task<APIResponse<IEnumerable<FacilityPitchTiming>>> GetFacilityPitchTimings(Guid facilityPitchId)
        {
            var response = new APIResponse<IEnumerable<FacilityPitchTiming>>();
            _logManager.LogInfo("-- Run::FacilityPitchTimingRepository::GetFacilityPitchTimings --");
            _logManager.LogDebugObject(facilityPitchId);
            

            try
            {
                var facilityPitchTimings = await _dbContext.FacilityPitchTimings.Where(f => f.IsEnabled == true).ToListAsync();
                return response = new APIResponse<IEnumerable<FacilityPitchTiming>>
                {
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = facilityPitchTimings
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

        public async Task<APIResponse> GetAllWithBooking(Guid facilityId)
        {
            var response = new APIResponse();
            _logManager.LogInfo("-- Run::FacilityPitchTimingRepository::GetAll --");
            _logManager.LogDebugObject(facilityId);


            try
            {
                var facilityPitches = await _dbContext.FacilityPitches.Where(f => f.FacilityId == facilityId && f.IsEnabled == true).ToListAsync();
                var facilityPitchTimings = await _dbContext.FacilityPitchTimings.ToListAsync();
                var timingCalendarList = new List<TimingCalendarViewModel>();
                var facilities = await _dbContext.Facilities.Include(a => a.Area).Where(f => f.IsEnabled == true).ToListAsync();
                var sports = await _dbContext.Sports.ToListAsync();
                var areas = await _dbContext.Areas.Where(f => f.IsEnabled == true).ToListAsync();
                if (facilityPitchTimings.Any())
                {
                    foreach (var facilityPitch in facilityPitches)
                    {
                        if (facilityPitch.FacilityPitchTimingIds != null && !string.IsNullOrWhiteSpace(facilityPitch.FacilityPitchTimingIds))
                        {
                            var facilityTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                            foreach (var facilityTimingId in facilityTimingIds)
                            {
                                if (!string.IsNullOrWhiteSpace(facilityTimingId))
                                {
                                    var timing = facilityPitchTimings.Where(f => f.FacilityPitchTimingId == new Guid(facilityTimingId)).FirstOrDefault();
                                    var booking = (from x in facilityPitches
                                                  join y in _dbContext.UserPitchBookings
                                                    on x.FacilityPitchId equals y.FacilityPitchId
                                                  select new UserPitchBooking()
                                                  {
                                                      BookingId = y.BookingId,
                                                  }).FirstOrDefault();
                                    if (timing != null)
                                    {
                                        var timingCalendar = new TimingCalendarViewModel();
                                        var playerCount = timing.PlayerIds != null ? timing.PlayerIds.Split(";") : Array.Empty<string>();
                                        timingCalendar.FacilityPitchId = (Guid)facilityPitch.FacilityPitchId;
                                        timingCalendar.FacilityPitchName = facilityPitch.Name;
                                        timingCalendar.Description = facilityPitch.Description;
                                        timingCalendar.PlayerCount = $"{playerCount.Length} / {facilityPitch.MaxPlayers}";
                                        timingCalendar.FacilityPitchTimingId = timing.FacilityPitchTimingId;
                                        var facility = facilities.Where(f => f.FacilityId == facilityPitch.FacilityId).FirstOrDefault();
                                        if (facility != null)
                                        {
                                            var area = areas.Where(a => a.AreaId == facility.Area.AreaId).FirstOrDefault();
                                            timingCalendar.FacilityName = facility != null ? facility.Name : string.Empty;
                                            timingCalendar.AreaName = area != null ? area.AreaName : string.Empty;
                                        }

                                        var sport = sports.Where(s => s.SportId == facilityPitch.SportId).FirstOrDefault();
                                        if (sport != null)
                                        {
                                            timingCalendar.SportName = sport.Name;
                                        }
                                        timingCalendar.SportId = facilityPitch.SportId;

                                        timingCalendar.TimeStart = timing.TimeStart;
                                        timingCalendar.TimeEnd = timing.TimeEnd;
                                        timingCalendar.CustomPrice = timing.CustomPrice;
                                        timingCalendar.IsRepeatEveryWeek = timing.IsRepeatEveryWeek;
                                        timingCalendar.Day = timing.Day;
                                        timingCalendar.BookingId = booking.BookingId;
                                        timingCalendarList.Add(timingCalendar);
                                    }
                                }
                            }
                        }

                    }
                }

                return new APIResponse
                {
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = timingCalendarList
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

        public async Task<APIResponse> GetAll(Guid facilityId)
        {
            var response = new APIResponse();
            _logManager.LogInfo("-- Run::FacilityPitchTimingRepository::GetAll --");
            _logManager.LogDebugObject(facilityId);


            try
            {
                var facilityPitches = await _dbContext.FacilityPitches.Where(f => f.FacilityId == facilityId && f.IsEnabled == true).ToListAsync();
                var facilityPitchTimings = await _dbContext.FacilityPitchTimings.ToListAsync();
                var timingCalendarList = new List<TimingCalendarViewModel>();
                var facilities = await _dbContext.Facilities.Include(a => a.Area).Where(f => f.IsEnabled == true).ToListAsync();
                var sports = await _dbContext.Sports.ToListAsync();
                var areas = await _dbContext.Areas.Where(f => f.IsEnabled == true).ToListAsync();
                if (facilityPitchTimings.Any())
                {
                    foreach (var facilityPitch in facilityPitches)
                    {
                        if (facilityPitch.FacilityPitchTimingIds != null && !string.IsNullOrWhiteSpace(facilityPitch.FacilityPitchTimingIds))
                        {
                            var facilityTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                            foreach (var facilityTimingId in facilityTimingIds)
                            {
                                if (!string.IsNullOrWhiteSpace(facilityTimingId))
                                {
                                    var timing = facilityPitchTimings.Where(f => f.FacilityPitchTimingId == new Guid(facilityTimingId)).FirstOrDefault();
                                    if (timing != null)
                                    {
                                        var timingCalendar = new TimingCalendarViewModel();
                                        var playerCount = timing.PlayerIds != null ? timing.PlayerIds.Split(";") : Array.Empty<string>();
                                        timingCalendar.FacilityPitchId = (Guid)facilityPitch.FacilityPitchId;
                                        timingCalendar.FacilityPitchName = facilityPitch.Name;
                                        timingCalendar.Description = facilityPitch.Description;
                                        timingCalendar.PlayerCount = $"{playerCount.Length} / {facilityPitch.MaxPlayers}";
                                        timingCalendar.FacilityPitchTimingId = timing.FacilityPitchTimingId;
                                        var facility = facilities.Where(f => f.FacilityId == facilityPitch.FacilityId).FirstOrDefault();
                                        if (facility != null)
                                        {
                                            var area = areas.Where(a => a.AreaId == facility.Area.AreaId).FirstOrDefault();
                                            timingCalendar.FacilityName = facility != null ? facility.Name : string.Empty;
                                            timingCalendar.AreaName = area != null ? area.AreaName : string.Empty;
                                        }

                                        var sport = sports.Where(s => s.SportId == facilityPitch.SportId).FirstOrDefault();
                                        if (sport != null)
                                        {
                                            timingCalendar.SportName = sport.Name;
                                        }
                                        timingCalendar.SportId = facilityPitch.SportId;
        
                                        timingCalendar.TimeStart = timing.TimeStart;
                                        timingCalendar.TimeEnd = timing.TimeEnd;
                                        timingCalendar.CustomPrice = timing.CustomPrice;
                                        timingCalendar.IsRepeatEveryWeek = timing.IsRepeatEveryWeek;
                                        timingCalendar.Day = timing.Day;

                                        timingCalendarList.Add(timingCalendar);
                                    }
                                }                           
                            }
                        }
                                       
                    }
                }
               
                return new APIResponse
                {
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = timingCalendarList
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

        public async Task<APIResponse> GetTiming(Guid facilityPitchTimingId)
        {
            var response = new APIResponse();
            _logManager.LogInfo("-- Run::FacilityPitchTimingRepository::GetAll --");
            _logManager.LogDebugObject(facilityPitchTimingId);

            try
            {
                var facilityPitchTiming = await _dbContext.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId).FirstOrDefaultAsync();
                var users = await _dbContext.Users.Where(u => u.IsEnabled == true).ToListAsync();
                var userPitchBookings = await _dbContext.UserPitchBookings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId).ToListAsync();
                if(facilityPitchTiming == null)
                {
                    return new APIResponse
                    {
                        Message = "Timing does not exist",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                var players = new List<PlayerPitchViewModel>();
                var pitchDetailList = new DisplayPitch();
                var Ids = facilityPitchTiming.PlayerIds.Split(";");
                if (Ids.Any())
                {
                    foreach (var id in Ids)
                    {
                        var user = users.Where(u => u.UserId.ToString() == id).FirstOrDefault();
                        if(user != null)
                        {
                            players.Add(new PlayerPitchViewModel
                            {
                                Email = user.Email,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                ProfileImgUrl = user.ImageUrl,
                                IsPaid = userPitchBookings.Where(u => u.UserId.ToString() == id).Any() ? userPitchBookings.Where(u => u.UserId.ToString() == id).FirstOrDefault().IsPaid : false,
                            });
                        }
                    }
                }

                pitchDetailList.Players = players;
                pitchDetailList.Timing = facilityPitchTiming;

                facilityPitchTiming.Date = GetDate(facilityPitchTiming.Day, DateTime.Now);

                return new APIResponse
                {
                    Message = "Successful Retrieved",
                    Payload = pitchDetailList,
                    StatusCode = System.Net.HttpStatusCode.OK
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


        public async Task<APIResponse> GetBookingDetails(Guid facilityPitchTimingId)
        {
            var response = new APIResponse();
            _logManager.LogInfo("-- Run::FacilityPitchTimingRepository::GetAll --");
            _logManager.LogDebugObject(facilityPitchTimingId);

            try
            {
                var userPitchBookings = await _dbContext.UserPitchBookings.AsNoTracking().Where(f => f.BookingId == facilityPitchTimingId).FirstOrDefaultAsync();
                var facilityPitchTiming = await _dbContext.FacilityPitchTimings.AsNoTracking().Where(f => f.FacilityPitchTimingId == userPitchBookings.FacilityPitchTimingId).FirstOrDefaultAsync();
                var users = await _dbContext.Users.Where(u => u.IsEnabled == true).ToListAsync();
                
                if (userPitchBookings == null)
                {
                    return new APIResponse
                    {
                        Message = "booking does not exist",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }

                var players = new List<PlayerPitchViewModel>();
                var pitchDetailList = new DisplayPitch();
                if (userPitchBookings != null)
                {
                    var facilityplayers = await _dbContext.FacilityPlayers.Where(a => a.BookingId == facilityPitchTimingId).ToListAsync();
                    foreach (var itemPlayer in facilityplayers)
                    {
                        var user = users.Where(u => u.UserId == itemPlayer.UserId).FirstOrDefault();
                        if (user != null)
                        {
                            players.Add(new PlayerPitchViewModel
                            {
                                Email = user.Email,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                ProfileImgUrl = user.ImageUrl,
                                IsPaid = userPitchBookings.IsPaid,
                            });
                        }
                    }    
                   
                }

                pitchDetailList.Players = players;

                facilityPitchTiming.TimeStart = userPitchBookings.PitchStart;
                facilityPitchTiming.TimeEnd = userPitchBookings.PitchEnd;
                facilityPitchTiming.Date = userPitchBookings.Date;
                facilityPitchTiming.CustomPrice = userPitchBookings.PricePerUser.GetValueOrDefault();
                pitchDetailList.Timing = facilityPitchTiming;

                return new APIResponse
                {
                    Message = "Successful Retrieved",
                    Payload = pitchDetailList,
                    StatusCode = System.Net.HttpStatusCode.OK
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

        private DateTime GetDate(DayOfWeek day, DateTime now)
        {
            if (day == now.DayOfWeek)
            {
                return now;
            }

            DateTime result = DateTime.Now.AddDays(1);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }
    }
}
