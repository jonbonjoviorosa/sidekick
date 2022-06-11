
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using AutoMapper;
using System.Threading.Tasks;
using System.Reflection;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class FacilityPitchRepository : APIBaseRepo, IFacilityPitchRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly IMapper _mapper;

        public FacilityPitchRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IMapper mapper)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            _mapper = mapper;
        }

        public async Task<APIResponse> AddFacilityPitch(string _auth, FacilityPitchDto _pitch)
        {
            APIResponse apiResp = new APIResponse();
            try
            {
                FacilityUserLoginTransaction IsUserLoggedIn = await DbContext.FacilityUserLoginTransactions.AsNoTracking().FirstOrDefaultAsync(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                FacilityPitch facilityPitch = await DbContext.FacilityPitches.Where(u => u.Id == _pitch.Id).FirstOrDefaultAsync();
                var unavailableSlots = await DbContext.UnavailableSlots.Where(f => f.FacilityId == _pitch.FacilityId).ToListAsync();
                List<FacilityTiming> facilityTimingList = await DbContext.FacilityTimings.Where(u => u.FacilityId == _pitch.FacilityId).ToListAsync();

                // check configure pith time is between facility start time and end time
                if (facilityTimingList != null && facilityTimingList.Any())
                {
                    foreach (var item in _pitch.FacilityPitchTimings)
                    {
                        var pitchStart = item.Date.Date.AddHours(item.TimeStart.Hour).AddMinutes(item.TimeStart.Minute);
                        var pitchEnd = item.Date.Date.AddHours(item.TimeEnd.Hour).AddMinutes(item.TimeEnd.Minute);

                        var facilityTiming = facilityTimingList.Where(s => (s.Day == pitchStart.DayOfWeek || s.IsEveryday) && s.TimeStart.TimeOfDay <= pitchStart.TimeOfDay && s.TimeEnd.TimeOfDay >= pitchEnd.TimeOfDay).ToList();
                        if (facilityTiming == null || facilityTiming != null && !facilityTiming.Any())
                        {
                            return new APIResponse
                            {
                                Message = "Timeslot should be within the Facility opening hours",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }
                else
                {
                    return new APIResponse
                    {
                        Message = "facility time Slot is not configure",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                //TODO CHECK OVERLAPPING
                foreach (var item in _pitch.FacilityPitchTimings)
                {
                    var pitchStart = item.Date.Date.AddHours(item.TimeStart.Hour).AddMinutes(item.TimeStart.Minute);
                    var pitchEnd = item.Date.Date.AddHours(item.TimeEnd.Hour).AddMinutes(item.TimeEnd.Minute);
                    foreach (var blockSlot in unavailableSlots.Where(f => f.FacilityPitchId == f.FacilityPitchId).ToList())
                    {
                        if (blockSlot.Starts.DayOfWeek == pitchStart.DayOfWeek && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && blockSlot.Ends.DayOfWeek == pitchEnd.DayOfWeek && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                        {
                            return new APIResponse
                            {
                                Message = "Slot is occupied",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }

                    foreach (var blockSlot in unavailableSlots.Where(u => u.AllPitches == true).ToList())
                    {
                        if (blockSlot.Starts.DayOfWeek == pitchStart.DayOfWeek && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && blockSlot.Ends.DayOfWeek == pitchEnd.DayOfWeek && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                        {
                            return new APIResponse
                            {
                                Message = "Slot is occupied",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }

                if (facilityPitch == null)
                {
                    LogManager.LogInfo("-- Run::FaciityPitchRepository::AddFacilityPitch --");
                    LogManager.LogDebugObject(_pitch);
                    Guid GuidId = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    FacilityPitch newPitch = new FacilityPitch
                    {
                        FacilityPitchId = GuidId,
                        FacilityId = _pitch.FacilityId,
                        SportId = _pitch.SportId,
                        Name = _pitch.Name,
                        Description = _pitch.Description,
                        ImageUrl = _pitch.ImageUrl,
                        MaxPlayers = _pitch.MaxPlayers,
                        IsFacilityTime = _pitch.IsFacilityTime,
                        IsFixedPrice = _pitch.IsFixedPrice,
                        FixedPrice = _pitch.FixedPrice,
                        Divisions = _pitch.Divisions,
                        SurfaceId = _pitch.SurfaceId,
                        TeamSize = _pitch.TeamSize,
                        LocationId = _pitch.LocationId,

                        LastEditedBy = IsUserLoggedIn.FacilityUserId,
                        LastEditedDate = RegistrationDate,
                        CreatedBy = IsUserLoggedIn.FacilityUserId,
                        CreatedDate = RegistrationDate,
                        IsEnabled = true,
                        IsEnabledBy = IsUserLoggedIn.FacilityUserId,
                        DateEnabled = RegistrationDate,
                        IsLocked = false,
                        LockedDateTime = RegistrationDate
                    };

                    var Ids = AddFacilityPitchTimings(_pitch.FacilityPitchTimings, newPitch.FacilityPitchId.Value, IsUserLoggedIn.FacilityUserId);
                    newPitch.FacilityPitchTimingIds = Ids;
                    DbContext.FacilityPitches.Add(newPitch);
                    await DbContext.SaveChangesAsync();

                    return apiResp = new APIResponse
                    {
                        Message = "New record added successfully.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                LogManager.LogInfo("-- Run::FaciityPitchRepository::EditFacilityPitch --");
                LogManager.LogDebugObject(_pitch);
                //facilityPitch.FacilityId = _pitch.FacilityId;
                //facilityPitch.FacilityPitchId = _pitch.FacilityPitchId;
                facilityPitch.SportId = _pitch.SportId;
                facilityPitch.Name = _pitch.Name;
                facilityPitch.Description = _pitch.Description;
                facilityPitch.ImageUrl = _pitch.ImageUrl;
                facilityPitch.MaxPlayers = _pitch.MaxPlayers;
                facilityPitch.IsFacilityTime = _pitch.IsFacilityTime;
                facilityPitch.IsFixedPrice = _pitch.IsFixedPrice;
                facilityPitch.FixedPrice = _pitch.FixedPrice;
                facilityPitch.Divisions = _pitch.Divisions;
                facilityPitch.SurfaceId = _pitch.SurfaceId;
                facilityPitch.TeamSize = _pitch.TeamSize;

                facilityPitch.LastEditedBy = IsUserLoggedIn.FacilityUserId;
                facilityPitch.CreatedBy = IsUserLoggedIn.FacilityUserId;
                facilityPitch.IsEnabled = true;
                facilityPitch.IsEnabledBy = IsUserLoggedIn.FacilityUserId;
                facilityPitch.IsLocked = false;

                var facilityPitchTimingIds = UpdateFacilityPitchTimings(_pitch, IsUserLoggedIn);
                facilityPitch.FacilityPitchTimingIds = facilityPitchTimingIds;
                DbContext.FacilityPitches.Update(facilityPitch);

                DbContext.Entry<FacilityPitch>(facilityPitch).State = EntityState.Modified;
                await DbContext.SaveChangesAsync();
                return apiResp = new APIResponse
                {
                    Message = "Record updated successfully",
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::AddFacilityPitch --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        private string UpdateFacilityPitchTimings(FacilityPitchDto pitch,
            FacilityUserLoginTransaction isUserLoggedIn)
        {
            var existingTimings = new List<FacilityPitchTiming>();
            var newPitchTiming = new List<FacilityPitchTiming>();
            var pitchTimingToRemove = new List<FacilityPitchTiming>();
            var timingGuidIds = new List<string>();
            var facilityPitchTimings = DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchId == pitch.FacilityPitchId).ToList();

            var facilityPitch = DbContext.FacilityPitches.Where(f => f.Id == pitch.Id).FirstOrDefault();

            foreach (var item in pitch.FacilityPitchTimings)
            {
                if (item.FacilityPitchTimingId == Guid.Empty)
                {
                    var GuidId = Guid.NewGuid();
                    var CreatedDate = DateTime.Now;

                    var newTiming = new FacilityPitchTiming
                    {
                        FacilityPitchTimingId = GuidId,
                        FacilityPitchId = pitch.FacilityPitchId.Value,
                        Day = item.Day,
                        Date = GetDate(item.Day, CreatedDate),
                        TimeStart = item.TimeStart,
                        TimeEnd = item.TimeEnd,
                        CustomPrice = item.CustomPrice,
                        IsRepeatEveryWeek = true,

                        LastEditedBy = isUserLoggedIn.FacilityUserId,
                        LastEditedDate = CreatedDate,
                        CreatedBy = isUserLoggedIn.FacilityUserId,
                        CreatedDate = CreatedDate,
                        IsEnabled = true,
                        IsEnabledBy = isUserLoggedIn.FacilityUserId,
                        DateEnabled = CreatedDate,
                        IsLocked = false,
                        LockedDateTime = CreatedDate
                    };

                    newPitchTiming.Add(newTiming);
                    timingGuidIds.Add(GuidId.ToString());
                    DbContext.FacilityPitchTimings.Add(newTiming);
                }
                else
                {
                    var timingModel = facilityPitchTimings.Where(f => f.FacilityPitchTimingId == item.FacilityPitchTimingId).FirstOrDefault();
                    if (timingModel != null)
                    {
                        timingModel.Date = GetDate(item.Day, DateTime.Now);
                        timingModel.Day = item.Day;
                        timingModel.TimeStart = item.TimeStart;
                        timingModel.TimeEnd = item.TimeEnd;
                        timingModel.CustomPrice = item.CustomPrice;
                        timingModel.IsRepeatEveryWeek = true;

                        timingModel.LastEditedBy = isUserLoggedIn.FacilityUserId;
                        timingModel.LastEditedDate = DateTime.Now;

                        existingTimings.Add(timingModel);
                        DbContext.FacilityPitchTimings.Update(timingModel);
                    }
                }
            }

            var facilityPitchTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
            foreach (var timingId in facilityPitchTimingIds)
            {
                var IdToAdd = pitch.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == timingId).FirstOrDefault();
                if (IdToAdd != null)
                {
                    timingGuidIds.Add(timingId);
                }
            }

            if (!string.IsNullOrWhiteSpace(pitch.TimingIdsToRemove))
            {
                var IdsToRemove = pitch.TimingIdsToRemove.Split(";");
                if (IdsToRemove.Any())
                {
                    foreach (var Id in IdsToRemove)
                    {
                        if (!string.IsNullOrWhiteSpace(Id))
                        {
                            var timingToRemove = DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == Id).FirstOrDefault();
                            if (timingToRemove != null)
                            {
                                pitchTimingToRemove.Add(timingToRemove);
                                DbContext.FacilityPitchTimings.Remove(timingToRemove);
                            }
                        }
                    }

                }
            }

            var Ids = timingGuidIds.Count != 0 ? string.Join(";", timingGuidIds) : string.Empty;
            return Ids;
        }

        private string AddFacilityPitchTimings(List<FacilityPitchTimings> _pitchTimings, Guid _newFacilityPitchId, Guid _userLoggedIn)
        {
            LogManager.LogInfo("-- Run::FaciityPitchRepository::AddFacilityPitchTimings --");

            var pitchTimings = new List<FacilityPitchTiming>();
            var timingGuidIds = new List<string>();

            foreach (var _timing in _pitchTimings)
            {
                var GuidId = Guid.NewGuid();
                var CreatedDate = DateTime.Now;

                FacilityPitchTiming newTiming = new FacilityPitchTiming
                {
                    FacilityPitchTimingId = GuidId,
                    FacilityPitchId = _newFacilityPitchId,
                    Day = _timing.Day,
                    Date = GetDate(_timing.Day, CreatedDate),
                    TimeStart = _timing.TimeStart,
                    TimeEnd = _timing.TimeEnd,
                    CustomPrice = _timing.CustomPrice,
                    IsRepeatEveryWeek = true,

                    LastEditedBy = _userLoggedIn,
                    LastEditedDate = CreatedDate,
                    CreatedBy = _userLoggedIn,
                    CreatedDate = CreatedDate,
                    IsEnabled = true,
                    IsEnabledBy = _userLoggedIn,
                    DateEnabled = CreatedDate,
                    IsLocked = false,
                    LockedDateTime = CreatedDate
                };

                pitchTimings.Add(newTiming);
                timingGuidIds.Add(newTiming.FacilityPitchTimingId.ToString());
            }

            var Ids = timingGuidIds.Count != 0 ? string.Join(";", timingGuidIds) : string.Empty;

            DbContext.FacilityPitchTimings.AddRange(pitchTimings);
            DbContext.SaveChanges();

            return Ids;
        }

        private DateTime GetDate(DayOfWeek day, DateTime createdDate)
        {
            if (day == createdDate.DayOfWeek)
            {
                return createdDate;
            }

            DateTime result = DateTime.Now.AddDays(1);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;

            //if(day < createdDate.DayOfWeek)
            //{
            //DateTime nextDate = DateTime.Today.AddDays(((int)DateTime.Today.DayOfWeek - (int)day) + 7);
            //    return nextDate;
            ////}


        }

        public async Task<APIResponse> GetFacilityPitch(Guid facilityId)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetFacilityPitch --");
            LogManager.LogDebugObject(facilityId);

            try
            {
                var facility = await DbContext.Facilities.Where(x => x.FacilityId == facilityId).Include(f => f.Area).FirstOrDefaultAsync();
                if (facility != null)
                {
                    var facilityTiming = await DbContext.FacilityTimings.Where(x => x.FacilityId == facilityId && x.IsEveryday == true).FirstOrDefaultAsync();

                    var location = facility.Street + ", " + facility.Area.AreaName;
                    var workingHours = facilityTiming != null ? facilityTiming.TimeStart.TimeOfDay + " - " + facilityTiming.TimeEnd.TimeOfDay : string.Empty;

                    IEnumerable<FacilityPitchList> facilityPitches = null;
                    facilityPitches = await DbContext.FacilityPitches.AsNoTracking()
                        .Where(w => w.FacilityId == facilityId)
                        .Select(i => new FacilityPitchList
                        {
                            FacilityId = i.FacilityId,
                            FacilityPitchId = i.FacilityPitchId,
                            SportId = i.SportId,
                            ImageUrl = facility.ImageUrl,
                            OpeningHours = workingHours,
                            Location = location,
                            CurrentBooking = 0, //TO DO
                            CreatedDate = i.CreatedDate,
                            Name = i.Name,
                            MaxPlayers = i.MaxPlayers,
                            FacilityPitchTimingIds = i.FacilityPitchTimingIds
                        }).ToListAsync();

                    var userPitchBookings = await DbContext.UserPitchBookings.Where(f => f.FacilityId == facilityId).ToListAsync();
                    var facilityPitchTimings = await DbContext.FacilityPitchTimings.ToListAsync();
                    var sports = await DbContext.Sports.ToListAsync();
                    var timingsThatHasBookings = new List<FacilityPitchTiming>();

                    foreach (var facilityPitch in facilityPitches)
                    {
                        facilityPitch.Sport = sports.Where(s => s.SportId == facilityPitch.SportId).FirstOrDefault().Name;
                        var timingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                        if (timingIds.Any())
                        {
                            foreach (var Id in timingIds)
                            {
                                var hasBookings = userPitchBookings.Where(u => u.FacilityPitchId == facilityPitch.FacilityPitchId && u.SportId == facilityPitch.SportId && u.FacilityPitchTimingId == Guid.Parse(Id)).ToList();
                                if (hasBookings.Any())
                                {
                                    var timingSlot = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == Id).FirstOrDefault();
                                    if (timingSlot != null)
                                    {
                                        timingSlot.Date = GetDate(timingSlot.Day, DateTime.Now);
                                        timingsThatHasBookings.Add(timingSlot);
                                        facilityPitch.Bookings = hasBookings;
                                    }
                                }
                            }
                        }

                        //facilityPitch.Bookings = hasBookings;
                        facilityPitch.FacilityPitchTimings = timingsThatHasBookings.Any() ? timingsThatHasBookings.OrderBy(f => f.TimeStart.TimeOfDay).ToList() : timingsThatHasBookings;
                        timingsThatHasBookings = new List<FacilityPitchTiming>();
                    }

                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = facilityPitches,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetFacilityPitch --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }


        public async Task<APIResponse> GetFacilityBookingPitch(Guid bookingID)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetFacilityBookingPitch --");
            LogManager.LogDebugObject(bookingID);

            try
            {
                LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookingsDetails --");
                var playBookings = new List<PlayBookingModel>();
                var bookingDetail = new PlayBookingModel();

                UserPitchBooking booking = await DbContext.UserPitchBookings.Where(u => u.IsEnabled == true && u.BookingId == bookingID).FirstOrDefaultAsync();
                if (booking != null)
                {
                    var locationName = "";
                    var facility = DbContext.Facilities.Where(u => u.FacilityId == booking.FacilityId).FirstOrDefault();
                    var FacilityPitches = DbContext.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId).FirstOrDefault();
                    if (facility != null)
                    {
                        locationName = facility.Street + " " + facility.Area + " " + facility.City + " " + facility.Country;
                    }

                    var TeamSizeName = FacilityPitches != null ? DbContext.Sizes.Where(u => u.SizeId == FacilityPitches.TeamSize && u.IsEnabled == true).Select(x => x.SizeName).FirstOrDefault() : "";

                    var SurfaceName = FacilityPitches != null ? DbContext.Surfaces.Where(u => u.SurfaceId == FacilityPitches.SurfaceId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault() : "";

                    var SportName = DbContext.Sports.Where(u => u.SportId == booking.SportId).Select(x => x.Name).FirstOrDefault();

                    bookingDetail = new PlayBookingModel
                    {
                        BookingId = booking.BookingId,
                        Name = booking.Name,
                        FacilityId = booking.FacilityId,
                        PitchStart = booking.PitchStart,
                        PitchEnd = booking.PitchEnd,
                        Date = Helper.DisplayDateTime(booking.Date),
                        IsPrivate = booking.IsPrivate,
                        PlayerCount = booking.PlayerCount,
                        PricePerPlayer = booking.PricePerUser.GetValueOrDefault(),
                        PriceIncludingVat = booking.PlayerCount * (booking.PricePerUser.GetValueOrDefault() + booking.PricePerUserVat.GetValueOrDefault()),
                        TotalPrice = (decimal)((booking.PlayerCount * (booking.PricePerUser.GetValueOrDefault() + booking.PricePerUserVat.GetValueOrDefault() + booking.Commission.GetValueOrDefault()))),
                        Commission = booking.Commission.GetValueOrDefault(),
                        Facility = facility,
                        FacilityPitch = FacilityPitches,
                        CreatedDate = booking.CreatedDate.Value,
                        UserName = null,
                        LocationName = locationName,
                        TeamSizeName = TeamSizeName,
                        SurfaceName = SurfaceName,
                        SportName = SportName,
                        IsPaid = booking.IsPaid,
                        WaitingPlayerCount = DbContext.FacilityPlayers.Count(a => a.BookingId == booking.BookingId && a.PlayerStatus == EGamePlayerStatus.Waiting),
                        RegisteredPlayers = (from gameplayer in DbContext.FacilityPlayers
                                             join user in DbContext.Users
                                             on gameplayer.UserId equals user.UserId
                                             where gameplayer.BookingId == booking.BookingId && gameplayer.IsEnabled == true && user.IsEnabled == true && (gameplayer.PlayerStatus == EGamePlayerStatus.Approved || gameplayer.PlayerStatus == EGamePlayerStatus.CanInvite)
                                             select new GamePlayerModel()
                                             {
                                                 BookingId = gameplayer.BookingId,
                                                 PlayerName = user.FirstName + " " + user.LastName,
                                                 UserId = gameplayer.UserId,
                                                 IsCaptain = gameplayer.IsCaptain,
                                                 FacilityPitchId = gameplayer.FacilityPitchId,
                                                 GameId = gameplayer.FacilityPlayerId,
                                                 SportId = gameplayer.SportId,
                                                 PlayerImage = user.ImageUrl,
                                                 PlayerStatus = gameplayer.PlayerStatus,
                                                 ParticipateDate = gameplayer.CreatedDate.GetValueOrDefault(),
                                                 SportName = SportName
                                             }).ToList(),
                        WaitingPlayers = (from gameplayer in DbContext.FacilityPlayers
                                          join user in DbContext.Users
                                          on gameplayer.UserId equals user.UserId
                                          where gameplayer.BookingId == booking.BookingId && (gameplayer.PlayerStatus == EGamePlayerStatus.Waiting)
                                          select new GamePlayerModel()
                                          {
                                              BookingId = gameplayer.BookingId,
                                              PlayerName = user.FirstName + " " + user.LastName,
                                              UserId = gameplayer.UserId,
                                              IsCaptain = gameplayer.IsCaptain,
                                              FacilityPitchId = gameplayer.FacilityPitchId,
                                              GameId = gameplayer.FacilityPlayerId,
                                              SportId = gameplayer.SportId,
                                              PlayerImage = user.ImageUrl,
                                              PlayerStatus = gameplayer.PlayerStatus,
                                              ParticipateDate = gameplayer.CreatedDate.GetValueOrDefault(),
                                              SportName = SportName
                                          }).ToList()
                    };

                }
                //return bookingDetail;

                return aResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = bookingDetail,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetFacilityPitch --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> GetFacilityPitchByFacilityPitchId(Guid _guid, Guid _facilityPitchId, Guid sportId)
        {
            APIResponse aResp = new();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetFacilityPitch --");
            LogManager.LogDebugObject(_guid);

            try
            {
                var facility = await DbContext.Facilities.Where(x => x.FacilityId == _guid).Include(x => x.Area).FirstOrDefaultAsync();
                if (facility != null)
                {
                    var facilityPitchTimings = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchId == _facilityPitchId && f.IsEnabled == true).ToListAsync();
                    //FacilityTiming facilityTiming = DbContext.FacilityTimings.Where(x => x.FacilityId == _guid && x.IsEveryday == true).FirstOrDefault();

                    var location = facility.Street + " " + facility.Area.AreaName;
                    //var workingHours = facilityTiming.TimeStart.TimeOfDay + " - " + facilityTiming.TimeEnd.TimeOfDay;

                    //var userPitchBookings = DbContext.UserPitchBookings.Where(f => f.FacilityPitchId == _facilityPitchId && f.IsEnabled == true).ToList();
                    //if (userPitchBookings != null)
                    //{
                    //    userPitchBookings = userPitchBookings.OrderBy(f => f.PitchStart).ToList();
                    //}

                    var slotList = new List<SlotRenderViewModel>();
                    var facilityPlayers = await DbContext.FacilityPlayers.Where(f => f.FacilityId == _guid && f.IsEnabled == true).ToListAsync();
                    var userPitchBookings = await DbContext.UserPitchBookings.Where(f => f.FacilityId == _guid && f.FacilityPitchId == _facilityPitchId).ToListAsync();

                    FacilityPitch facilityPitch = null;
                    facilityPitch = await DbContext.FacilityPitches.AsNoTracking()
                        .Where(w => w.FacilityId == _guid && w.FacilityPitchId == _facilityPitchId && w.SportId == sportId)
                        .Select(i => new FacilityPitch
                        {
                            FacilityId = i.FacilityId,
                            FacilityPitchId = i.FacilityPitchId,
                            SportId = i.SportId,
                            ImageUrl = facility.ImageUrl,
                            Description = facility.Description,
                            MaxPlayers = i.MaxPlayers,
                            IsFixedPrice = i.IsFixedPrice,
                            FixedPrice = i.FixedPrice,
                            CreatedDate = i.CreatedDate,
                            SurfaceId = i.SurfaceId,
                            TeamSize = i.TeamSize,
                            Name = i.Name,
                            LocationId = i.LocationId,
                            Id = i.Id,
                            FacilityPitchTimingIds = i.FacilityPitchTimingIds,
                        }).FirstOrDefaultAsync();

                    var mappedResponse = _mapper.Map<FacilityPitchVM>(facilityPitch);
                    mappedResponse.Location = location;
                    var timingsThatHasBookings = new List<FacilityPitchTiming>();
                    mappedResponse.FacilityPitchTimings = new List<FacilityPitchTiming>();

                    var timingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                    if (timingIds.Any())
                    {
                        foreach (var id in timingIds)
                        {
                            var timingModel = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == id).FirstOrDefault();
                            if (timingModel != null)
                            {
                                var hasBookings = userPitchBookings.Where(u => u.FacilityPitchTimingId == timingModel.FacilityPitchTimingId).ToList();
                                if (hasBookings.Any())
                                {
                                    var timingSlot = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == id).FirstOrDefault();
                                    if (timingSlot != null)
                                    {
                                        timingSlot.Date = GetDate(timingSlot.Day, DateTime.Now);
                                        timingsThatHasBookings.Add(timingSlot);
                                        mappedResponse.Bookings = hasBookings;
                                    }

                                }

                                mappedResponse.FacilityPitchTimings.Add(timingModel);
                            }


                        }
                    }



                    mappedResponse.Bookings = userPitchBookings.Where(u => u.FacilityPitchId == _facilityPitchId && u.SportId == sportId).ToList(); ;
                    mappedResponse.BookingPitchTimings = new List<FacilityPitchTiming>();
                    if (mappedResponse.Bookings.Count > 0)
                    {
                        var booking = mappedResponse.Bookings[0];


                        var timingModel = new FacilityPitchTiming();
                        timingModel.TimeStart = booking.PitchStart;
                        timingModel.TimeEnd = booking.PitchEnd;
                        timingModel.Date = booking.Date;
                        timingModel.CustomPrice = booking.PricePerUser.GetValueOrDefault();
                        mappedResponse.BookingPitchTimings.Add(timingModel);

                        var user = facilityPlayers.Where(f => f.BookingId == booking.BookingId);
                        var onLoadPlayerDetails = new List<PlayerPitchViewModel>();
                        foreach (var player in user)
                        {
                            onLoadPlayerDetails.Add(new PlayerPitchViewModel
                            {
                                FirstName = player.FirstName,
                                LastName = player.LastName,
                                Email = player.Email,
                                IsPaid = mappedResponse.Bookings[0].IsFree == true ? true : false,
                                ProfileImgUrl = player.ProfileImgUrl
                            });
                        }


                        mappedResponse.OnLoadPlayerDetails = onLoadPlayerDetails;

                        //mappedResponse.BookingPitchTimings = timingsThatHasBookings.Any() ? timingsThatHasBookings.OrderBy(f => f.TimeStart.TimeOfDay).ToList() : timingsThatHasBookings;
                        //if (mappedResponse.BookingPitchTimings.Any())
                        //{
                        //    //
                        //    //if (mappedResponse.BookingPitchTimings[0].PlayerIds != null)
                        //    //{
                        //    //    var playerIds = mappedResponse.BookingPitchTimings[0].PlayerIds.Split(";");
                        //    //    if (playerIds.Any())
                        //    //    {
                        //    //        foreach (var player in playerIds)
                        //    //        {
                        //    //            var user = facilityPlayers.Where(f => f.UserId.ToString() == player).FirstOrDefault();
                        //    //            onLoadPlayerDetails.Add(new PlayerPitchViewModel
                        //    //            {
                        //    //                FirstName = user.FirstName,
                        //    //                LastName = user.LastName,
                        //    //                Email = user.Email,
                        //    //                IsPaid = mappedResponse.BookingPitchTimings[0].IsFree == true ? true : false,
                        //    //                ProfileImgUrl = user.ProfileImgUrl
                        //    //            });
                        //    //        }
                        //    //    }
                        //    //}


                        //}


                        //foreach (var timing in mappedResponse.FacilityPitchTimings)
                        //{
                        //    var playerIds = timing.PlayerIds.Split(";");
                        //    if (playerIds.Any())
                        //    {
                        //        foreach (var player in playerIds)
                        //        {
                        //            var user = facilityPlayers.Where(f => f.UserId.ToString() == player).FirstOrDefault();
                        //            if (user != null)
                        //            {

                        //            }
                        //        }

                        //    }
                        //}

                        ////mappedResponse.Bookings = userPitchBookings ?? new List<UserPitchBooking>();
                        //var timingList = new List<FacilityPitchTiming>();
                        //var facilityPitchTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                        //if (facilityPitchTimingIds.Any())
                        //{
                        //    foreach (var timing in facilityPitchTimingIds)
                        //    {
                        //        var timingModel = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == timing).FirstOrDefault();
                        //        if (timingModel != null)
                        //        {
                        //            timingList.Add(timingModel);
                        //        }
                        //    }

                        //}
                        //mappedResponse.FacilityPitchTimings = timingList ?? new List<FacilityPitchTiming>();

                        //var x = userPitchBookings.GroupBy(f => new
                        //{
                        //    PitchStart = f.PitchStart.ToShortTimeString(),
                        //    PitchEnd = f.PitchEnd.ToShortTimeString(),
                        //    Date = f.Date,
                        //    Price = f.PricePerUser
                        //}).ToDictionary(f => f.Key, f => f.ToList()).ToList();

                        //var slotRender = new List<SlotRenderViewModel>();

                        //foreach (var item in x)
                        //{
                        //    var slot = new SlotRenderViewModel();
                        //    var a = PopulatePitchTimingRecord(item, slot, facilityPlayers);
                        //    slotRender.Add(a);
                        //}

                        //mappedResponse.MappedBookings = slotRender;
                    }
                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = mappedResponse,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetFacilityPitch --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> GetFacilityPitchByFacilityPitchIdWithTiming(Guid _guid, Guid _facilityPitchId, Guid sportId, Guid facilityPitchTimingId, Guid bookingID)
        {
            APIResponse aResp = new();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetFacilityPitch --");
            LogManager.LogDebugObject(_guid);

            try
            {
                var facility = await DbContext.Facilities.Where(x => x.FacilityId == _guid).Include(x => x.Area).FirstOrDefaultAsync();
                if (facility != null)
                {
                    var facilityPitchTimings = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchId == _facilityPitchId && f.FacilityPitchTimingId == facilityPitchTimingId && f.IsEnabled == true).ToListAsync();
                    //FacilityTiming facilityTiming = DbContext.FacilityTimings.Where(x => x.FacilityId == _guid && x.IsEveryday == true).FirstOrDefault();

                    var location = facility.Street + " " + facility.Area.AreaName;
                    //var workingHours = facilityTiming.TimeStart.TimeOfDay + " - " + facilityTiming.TimeEnd.TimeOfDay;

                    //var userPitchBookings = DbContext.UserPitchBookings.Where(f => f.FacilityPitchId == _facilityPitchId && f.IsEnabled == true).ToList();
                    //if (userPitchBookings != null)
                    //{
                    //    userPitchBookings = userPitchBookings.OrderBy(f => f.PitchStart).ToList();
                    //}

                    var slotList = new List<SlotRenderViewModel>();
                    var facilityPlayers = await DbContext.FacilityPlayers.Where(f => f.FacilityId == _guid && f.IsEnabled == true).ToListAsync();
                    var userPitchBookings = await DbContext.UserPitchBookings.Where(f => f.FacilityId == _guid && f.FacilityPitchId == _facilityPitchId).ToListAsync();

                    FacilityPitch facilityPitch = null;
                    facilityPitch = await DbContext.FacilityPitches.AsNoTracking()
                        .Where(w => w.FacilityId == _guid && w.FacilityPitchId == _facilityPitchId && w.SportId == sportId)
                        .Select(i => new FacilityPitch
                        {
                            FacilityId = i.FacilityId,
                            FacilityPitchId = i.FacilityPitchId,
                            SportId = i.SportId,
                            ImageUrl = facility.ImageUrl,
                            Description = facility.Description,
                            MaxPlayers = i.MaxPlayers,
                            IsFixedPrice = i.IsFixedPrice,
                            FixedPrice = i.FixedPrice,
                            CreatedDate = i.CreatedDate,
                            SurfaceId = i.SurfaceId,
                            TeamSize = i.TeamSize,
                            Name = i.Name,
                            LocationId = i.LocationId,
                            Id = i.Id,
                            FacilityPitchTimingIds = i.FacilityPitchTimingIds,
                        }).FirstOrDefaultAsync();

                    var mappedResponse = _mapper.Map<FacilityPitchVM>(facilityPitch);
                    mappedResponse.Location = location;
                    var timingsThatHasBookings = new List<FacilityPitchTiming>();
                    mappedResponse.FacilityPitchTimings = new List<FacilityPitchTiming>();


                    var timingModel = facilityPitchTimings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId).FirstOrDefault();
                    if (timingModel != null)
                    {
                        var hasBookings = new List<UserPitchBooking>();
                        if (bookingID != Guid.Empty)
                            hasBookings = userPitchBookings.Where(u => u.FacilityPitchTimingId == timingModel.FacilityPitchTimingId).ToList();
                        else
                            hasBookings = userPitchBookings.Where(u => u.FacilityPitchTimingId == timingModel.FacilityPitchTimingId && u.BookingId == bookingID).ToList();
                        if (hasBookings.Any())
                        {
                            var timingSlot = facilityPitchTimings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId).FirstOrDefault();
                            if (timingSlot != null)
                            {
                                timingSlot.Date = GetDate(timingSlot.Day, DateTime.Now);
                                timingsThatHasBookings.Add(timingSlot);
                                mappedResponse.Bookings = hasBookings;
                            }

                        }

                        mappedResponse.FacilityPitchTimings.Add(timingModel);
                    }
                    if (bookingID != Guid.Empty)
                        mappedResponse.Bookings = userPitchBookings.Where(u => u.FacilityPitchId == _facilityPitchId && u.SportId == sportId && u.FacilityPitchTimingId == facilityPitchTimingId && u.BookingId == bookingID).ToList();
                    else
                        mappedResponse.Bookings = userPitchBookings.Where(u => u.FacilityPitchId == _facilityPitchId && u.SportId == sportId && u.FacilityPitchTimingId == facilityPitchTimingId).ToList();
                    mappedResponse.BookingPitchTimings = new List<FacilityPitchTiming>();
                    if (mappedResponse.Bookings.Count > 0)
                    {

                        var booking = mappedResponse.Bookings[0];


                        var timingModel1 = new FacilityPitchTiming();
                        timingModel1.TimeStart = booking.PitchStart;
                        timingModel1.TimeEnd = booking.PitchEnd;
                        timingModel1.Date = booking.Date;
                        timingModel1.CustomPrice = booking.PricePerUser.GetValueOrDefault();
                        mappedResponse.BookingPitchTimings.Add(timingModel1);

                        var user = facilityPlayers.Where(f => f.BookingId == booking.BookingId);
                        var onLoadPlayerDetails = new List<PlayerPitchViewModel>();
                        foreach (var player in user)
                        {
                            onLoadPlayerDetails.Add(new PlayerPitchViewModel
                            {
                                FirstName = player.FirstName,
                                LastName = player.LastName,
                                Email = player.Email,
                                IsPaid = mappedResponse.Bookings[0].IsFree == true ? true : false,
                                ProfileImgUrl = player.ProfileImgUrl
                            });
                        }


                        mappedResponse.OnLoadPlayerDetails = onLoadPlayerDetails;


                    }

                    return aResp = new APIResponse
                    {
                        Message = "All records found.",
                        Status = "Success!",
                        Payload = mappedResponse,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetFacilityPitch --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> GetFacilityPitchSports(Guid _facilityId)
        {
            APIResponse aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetFacilityPitchSports --");
            LogManager.LogDebugObject(_facilityId);

            try
            {
                IEnumerable<SportDto> sports = null;
                sports = await DbContext.FacilitySports.AsNoTracking()
                    .Where(w => w.FacilityId == _facilityId)
                    .Select(i => new SportDto
                    {
                        SportId = i.SportId,
                        IsEnabled = i.IsEnabled,
                    }).ToListAsync();

                if (sports.Any())
                {
                    foreach (var item in sports)
                    {
                        Sport details = await DbContext.Sports.Where(x => x.SportId == item.SportId).FirstOrDefaultAsync();
                        item.Name = details.Name;
                        item.Description = details.Description;
                    }
                }

                return aResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = sports,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetFacilityPitchSports --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> AddBlockSlot(string _auth, UnavailableSlot blockSlot)
        {
            var response = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::BlockSlot --");
            LogManager.LogDebugObject(blockSlot);

            try
            {
                var IsUserLoggedIn = await DbContext.FacilityUserLoginTransactions.AsNoTracking().FirstOrDefaultAsync(ult => ult.Token == _auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return response = new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }
                var existingSlot = DbContext.UnavailableSlots.Where(b => b.UnavailableSlotId == blockSlot.UnavailableSlotId
                                                                    && b.FacilityId == blockSlot.FacilityId).FirstOrDefault();
                var facilityPitches = await DbContext.FacilityPitches.Where(f => f.FacilityId == blockSlot.FacilityId).ToListAsync();
                var unavailableSlots = await DbContext.UnavailableSlots.Where(u => u.FacilityId == blockSlot.FacilityId).ToListAsync();
                if (existingSlot == null)
                {
                    var facilityPitchTimings = await DbContext.FacilityPitchTimings.ToListAsync();
                    foreach (var pitch in facilityPitches)
                    {
                        var timingIds = pitch.FacilityPitchTimingIds.Split(";");
                        foreach (var timing in timingIds)
                        {
                            if (!string.IsNullOrWhiteSpace(timing))
                            {
                                var timingSlot = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == timing).FirstOrDefault();
                                if (timingSlot != null)
                                {
                                    if (timingSlot.Day == blockSlot.Starts.DayOfWeek && timingSlot.TimeStart.TimeOfDay < blockSlot.Ends.TimeOfDay
                                                && timingSlot.Day == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < timingSlot.TimeEnd.TimeOfDay)
                                    {
                                        return new APIResponse
                                        {
                                            Message = "There is an overlapping schedule with the Slot",
                                            StatusCode = System.Net.HttpStatusCode.BadRequest
                                        };
                                    }

                                }

                            }
                        }
                    }

                    if (blockSlot.AllPitches.Value)
                    {
                        foreach (var slot in unavailableSlots.Where(u => u.AllPitches == false && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }

                        foreach (var slot in unavailableSlots.Where(u => u.AllPitches == true && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var slot in unavailableSlots.Where(u => u.FacilityPitchId == blockSlot.FacilityPitchId && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }

                        foreach (var slot in unavailableSlots.Where(u => u.AllPitches == true && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }
                    }



                    blockSlot.UnavailableSlotId = Guid.NewGuid();

                    blockSlot.LastEditedBy = IsUserLoggedIn.FacilityUserId;
                    blockSlot.LastEditedDate = DateTime.Now;
                    blockSlot.CreatedBy = IsUserLoggedIn.FacilityUserId;
                    blockSlot.CreatedDate = DateTime.Now;
                    blockSlot.IsEnabled = true;
                    blockSlot.IsEnabledBy = IsUserLoggedIn.FacilityUserId;
                    blockSlot.DateEnabled = DateTime.Now;
                    blockSlot.IsLocked = false;
                    blockSlot.LockedDateTime = DateTime.Now;

                    DbContext.UnavailableSlots.Add(blockSlot);
                    DbContext.SaveChanges();

                    return response = new APIResponse
                    {
                        Message = "Successfully blocked slot",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = blockSlot.UnavailableSlotId
                    };
                }
                else
                {
                    var facilityPitchTimings = await DbContext.FacilityPitchTimings.ToListAsync();
                    foreach (var pitch in facilityPitches)
                    {
                        var timingIds = pitch.FacilityPitchTimingIds.Split(";");
                        foreach (var timing in timingIds)
                        {
                            if (!string.IsNullOrWhiteSpace(timing))
                            {
                                var timingSlot = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == timing).FirstOrDefault();
                                if (timingSlot != null)
                                {
                                    if (timingSlot.FacilityPitchId == blockSlot.FacilityPitchId)
                                    {
                                        if (timingSlot.TimeStart.DayOfWeek == blockSlot.Starts.DayOfWeek && timingSlot.TimeStart.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && timingSlot.TimeEnd.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < timingSlot.TimeEnd.TimeOfDay)
                                        {
                                            return new APIResponse
                                            {
                                                Message = "There is an overlapping schedule with the Slot",
                                                StatusCode = System.Net.HttpStatusCode.BadRequest
                                            };
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (blockSlot.AllPitches.Value)
                    {
                        foreach (var slot in unavailableSlots.Where(u => u.AllPitches == false && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }

                        foreach (var slot in unavailableSlots.Where(u => u.AllPitches == true && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts <= blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var slot in unavailableSlots.Where(u => u.FacilityPitchId == blockSlot.FacilityPitchId && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }

                        foreach (var slot in unavailableSlots.Where(u => u.AllPitches == true && u.UnavailableSlotId != blockSlot.UnavailableSlotId).ToList())
                        {
                            if (slot.Starts < blockSlot.Ends && blockSlot.Starts < slot.Ends)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is already occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            if (slot.RepeatEveryWeek.Value)
                            {
                                if (slot.Starts.DayOfWeek == blockSlot.Starts.DayOfWeek && slot.Ends.TimeOfDay < blockSlot.Ends.TimeOfDay
                                        && slot.Ends.DayOfWeek == blockSlot.Ends.DayOfWeek && blockSlot.Starts.TimeOfDay < slot.Ends.TimeOfDay)
                                {
                                    return new APIResponse
                                    {
                                        Message = "There is an overlapping schedule with the Slot",
                                        StatusCode = System.Net.HttpStatusCode.BadRequest
                                    };
                                }
                            }
                        }
                    }

                    existingSlot.AllDay = blockSlot.AllDay;
                    existingSlot.Starts = blockSlot.Starts;
                    existingSlot.Ends = blockSlot.Ends;
                    existingSlot.RepeatEveryWeek = blockSlot.RepeatEveryWeek;
                    existingSlot.During = blockSlot.During;
                    existingSlot.Title = blockSlot.Title;
                    existingSlot.Notes = blockSlot.Notes;
                    existingSlot.FacilityPitchId = blockSlot.FacilityPitchId;
                    existingSlot.AllPitches = blockSlot.AllPitches;

                    existingSlot.LastEditedBy = IsUserLoggedIn.FacilityUserId;
                    existingSlot.LastEditedDate = DateTime.Now;

                    DbContext.UnavailableSlots.Update(existingSlot);
                    DbContext.SaveChanges();

                    return response = new APIResponse
                    {
                        Message = "Successfully updated slot",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = blockSlot.UnavailableSlotId
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::BlockSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                response.Message = "Something Went wrong!";
                response.Status = "Internal Server Error";
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.ModelError = GetStackError(ex.InnerException);
            }

            return response;
        }

        public SlotRenderViewModel PopulatePitchTimingRecord<T>(KeyValuePair<T, List<UserPitchBooking>> item, SlotRenderViewModel model, List<FacilityPlayer> player)
        {
            var getType = item.Key.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(getType.GetProperties());
            foreach (var prop in props)
            {
                object property = prop.Name;
                object value = prop.GetValue(item.Key, null);
                if (property.ToString() == "PitchStart")
                    model.PitchStart = DateTime.Parse(value.ToString());
                if (property.ToString() == "PitchEnd")
                    model.PitchEnd = DateTime.Parse(value.ToString());
                if (property.ToString() == "Date")
                    model.PitchDate = (DateTime)value;
                if (property.ToString() == "Price")
                    model.Price = value != null ? decimal.Parse(value.ToString()) : default;
            }
            model.Players = new List<PlayerPitchViewModel>();

            foreach (var value in item.Value)
            {

                var user = player.Where(p => p.UserId == value.UserId).FirstOrDefault();
                if (user != null)
                {
                    model.Players.Add(new PlayerPitchViewModel
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IsPaid = user.IsPaid,
                        ProfileImgUrl = user.ProfileImgUrl
                    });
                }
            }
            model.GuidId = Guid.NewGuid();
            return model;
        }

        public async Task<APIResponse> GetBlockedSlots(Guid _facilityId)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetBlockedSlots --");
            LogManager.LogDebugObject(_facilityId);

            try
            {
                var blockedSlots = await DbContext.UnavailableSlots.Where(b => b.FacilityId == _facilityId && b.IsEnabled == true).ToListAsync();
                var vm = new List<UnavailableSlotViewModel>();
                var facilityPitches = await DbContext.FacilityPitches.Where(f => f.IsEnabled == true).ToListAsync();
                foreach (var blockedSlot in blockedSlots)
                {
                    var facilityPitch = facilityPitches.Where(f => f.FacilityPitchId == blockedSlot.FacilityPitchId).FirstOrDefault();
                    var name = string.Empty;
                    if (facilityPitch != null)
                    {
                        name = facilityPitch.Name;
                    }
                    var viewModel = new UnavailableSlotViewModel
                    {
                        AllDay = blockedSlot.AllDay,
                        AllPitches = blockedSlot.AllPitches,
                        During = blockedSlot.During,
                        Ends = blockedSlot.Ends,
                        FacilityId = blockedSlot.FacilityId,
                        FacilityPitchId = blockedSlot.FacilityPitchId,
                        FacilityPitchName = blockedSlot.AllPitches == true ? "All Pitches" : name,
                        Notes = blockedSlot.Notes,
                        RepeatEveryWeek = blockedSlot.RepeatEveryWeek,
                        Starts = blockedSlot.Starts,
                        Title = blockedSlot.Title,
                        UnavailableSlotId = blockedSlot.UnavailableSlotId
                    };

                    vm.Add(viewModel);
                }
                return new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = vm,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetBlockedSlots --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> CreateSlot(string auth, FacilityPitchVM pitch)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::CreateSlot --");
            LogManager.LogDebugObject(pitch);

            try
            {
                var IsUserLoggedIn = await DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefaultAsync(ult => ult.Token == auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                var facilityPitch = await DbContext.FacilityPitches.Where(f => f.FacilityId == pitch.FacilityId
                                                                        && f.FacilityPitchId == pitch.FacilityPitchId
                                                                        && f.SportId == pitch.SportId
                                                                        && f.IsEnabled == true).FirstOrDefaultAsync();

                var unavailableSlots = await DbContext.UnavailableSlots.Where(u => u.FacilityId == pitch.FacilityId).ToListAsync();
                var facilityPitchTimings = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchId == pitch.FacilityPitchId).ToListAsync();
                //var pitchStart = pitch.FacilityPitchTimings.FirstOrDefault().Date.AddHours(pitch.FacilityPitchTimings.FirstOrDefault().TimeStart.Hour).AddMinutes(pitch.FacilityPitchTimings.FirstOrDefault().TimeStart.Minute);
                //var pitchEnd = pitch.FacilityPitchTimings.FirstOrDefault().Date.AddHours(pitch.FacilityPitchTimings.FirstOrDefault().TimeEnd.Hour).AddMinutes(pitch.FacilityPitchTimings.FirstOrDefault().TimeEnd.Minute);
                var pitchTimings = new List<FacilityPitchTiming>();
                var date = GetDate(pitch.FacilityPitchTimings.FirstOrDefault().Day, DateTime.Now);

                var pitchStart = date.Date.AddHours(pitch.FacilityPitchTimings.FirstOrDefault().TimeStart.Hour).AddMinutes(pitch.FacilityPitchTimings.FirstOrDefault().TimeStart.Minute);
                var pitchEnd = date.Date.AddHours(pitch.FacilityPitchTimings.FirstOrDefault().TimeEnd.Hour).AddMinutes(pitch.FacilityPitchTimings.FirstOrDefault().TimeEnd.Minute);
                if (facilityPitch != null)
                {
                    var facilityPitchTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                    foreach (var timing in facilityPitchTimingIds)
                    {
                        var timingModel = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == timing).FirstOrDefault();
                        if (timingModel != null)
                        {
                            var pitchStartTiming = timingModel.Date.Date.AddHours(timingModel.TimeStart.Hour).AddMinutes(timingModel.TimeStart.Minute);
                            var pitchEndTiming = timingModel.Date.Date.AddHours(timingModel.TimeEnd.Hour).AddMinutes(timingModel.TimeEnd.Minute);
                            if (pitchStartTiming.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && pitchStartTiming.TimeOfDay < pitchEnd.TimeOfDay
                            && pitchEndTiming.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && pitchStart.TimeOfDay < pitchEndTiming.TimeOfDay)
                            {
                                return new APIResponse
                                {
                                    Message = "Slot is Occupied",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }
                        }
                    }

                    foreach (var blockSlot in unavailableSlots.Where(u => u.FacilityPitchId == pitch.FacilityPitchId).ToList())
                    {
                        if (blockSlot.Starts.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && blockSlot.Ends.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                        {
                            return new APIResponse
                            {
                                Message = "Slot is occupied",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }

                    foreach (var blockSlot in unavailableSlots.Where(u => u.AllPitches == true).ToList())
                    {
                        if (blockSlot.Starts.DayOfWeek == pitchStart.DayOfWeek && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && blockSlot.Ends.DayOfWeek == pitchEnd.DayOfWeek && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                        {
                            return new APIResponse
                            {
                                Message = "Slot is occupied",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }
                else
                {
                    foreach (var blockSlot in unavailableSlots.Where(u => u.FacilityPitchId == pitch.FacilityPitchId).ToList())
                    {
                        if (blockSlot.Starts.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && blockSlot.Ends.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                        {
                            return new APIResponse
                            {
                                Message = "Slot is occupied",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }

                    foreach (var blockSlot in unavailableSlots.Where(u => u.AllPitches == true).ToList())
                    {
                        if (blockSlot.Starts.DayOfWeek == pitchStart.DayOfWeek && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && blockSlot.Ends.DayOfWeek == pitchEnd.DayOfWeek && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                        {
                            return new APIResponse
                            {
                                Message = "Slot is occupied",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }

                var facility = await DbContext.Facilities.Where(f => f.FacilityId == pitch.FacilityId).Include(f => f.Area).FirstOrDefaultAsync();
                if (facilityPitch != null)
                {
                    facilityPitch.Description = pitch.Description;
                    var facilityPlayers = await DbContext.FacilityPlayers.ToListAsync();
                    foreach (var item in pitch.FacilityPitchTimings)
                    {
                        var newTimingId = Guid.NewGuid();
                        pitchTimings.Add(new FacilityPitchTiming
                        {
                            FacilityPitchTimingId = newTimingId,
                            FacilityPitchId = pitch.FacilityPitchId.Value,
                            Day = item.Day,
                            TimeStart = item.TimeStart,
                            TimeEnd = item.TimeEnd,
                            CustomPrice = item.CustomPrice,
                            Date = item.Date,
                            IsFree = item.IsFree,
                            IsRepeatEveryWeek = true,
                            PlayerIds = item.PlayerIds,

                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now
                        });

                        var playerIds = item.PlayerIds.Split(";");
                        foreach (var playerId in playerIds)
                        {
                            if (!string.IsNullOrWhiteSpace(playerId))
                            {
                                var existingPlayerFacility = facilityPlayers.Where(f => f.FacilityId == pitch.FacilityId && f.UserId == new Guid(playerId)).FirstOrDefault();
                                if (existingPlayerFacility == null)
                                {
                                    var playerInfo = facilityPlayers.Where(f => f.UserId == new Guid(playerId)).FirstOrDefault();
                                    if (playerInfo != null)
                                    {
                                        var bookingIdGuid = Guid.NewGuid();
                                        var facilityPlayer = new FacilityPlayer()
                                        {
                                            UserId = playerInfo.UserId,
                                            Name = playerInfo.Name,
                                            FirstName = playerInfo.FirstName,
                                            LastName = playerInfo.LastName,
                                            AreaId = playerInfo.AreaId,
                                            Email = playerInfo.Email,
                                            ProfileImgUrl = playerInfo.ProfileImgUrl,
                                            ContactNumber = playerInfo.ContactNumber,
                                            FacilityId = pitch.FacilityId.GetValueOrDefault(),
                                            BookingId = bookingIdGuid,

                                            LastEditedBy = IsUserLoggedIn.AdminId,
                                            LastEditedDate = DateTime.Now,
                                            DateCreated = DateTime.Now,
                                            CreatedBy = IsUserLoggedIn.AdminId,
                                            CreatedDate = DateTime.Now,
                                            IsEnabled = true,
                                            IsEnabledBy = IsUserLoggedIn.AdminId,
                                            DateEnabled = DateTime.Now,
                                            IsLocked = false,
                                            LockedDateTime = DateTime.Now,
                                        };

                                        DbContext.FacilityPlayers.Add(facilityPlayer);

                                        var newFreeBooking = new UserPitchBooking
                                        {
                                            BookingId = bookingIdGuid,
                                            FacilityId = pitch.FacilityId.GetValueOrDefault(),
                                            Name = facility.Name,
                                            Description = pitch.Description,
                                            FacilityPitchId = pitch.FacilityPitchId.GetValueOrDefault(),
                                            SportId = pitch.SportId,
                                            AreaId = facility.Area.AreaId,
                                            City = facility.City,
                                            SurfaceId = facilityPitch.SurfaceId,
                                            UserId = playerInfo.UserId,
                                            IsCaptain = false,
                                            IsPaid = true, //Free Game
                                            PricePerUser = default,
                                            IsEnabled = true,
                                            PitchStart = item.TimeStart,
                                            PitchEnd = item.TimeEnd,
                                            CreatedDate = DateTime.Now,
                                            Date = GetDate(item.Day, DateTime.Now),
                                            FacilityPitchTimingId = newTimingId,

                                            CreatedBy = IsUserLoggedIn.AdminId,
                                            IsFree = true
                                        };
                                        DbContext.UserPitchBookings.Add(newFreeBooking);

                                    }
                                }
                                else
                                {
                                    var newFreeBooking = new UserPitchBooking
                                    {
                                        BookingId = Guid.NewGuid(),
                                        FacilityId = pitch.FacilityId.GetValueOrDefault(),
                                        Name = facility.Name,
                                        Description = pitch.Description,
                                        FacilityPitchId = pitch.FacilityPitchId.GetValueOrDefault(),
                                        SportId = pitch.SportId,
                                        AreaId = facility.Area.AreaId,
                                        City = facility.City,
                                        SurfaceId = facilityPitch.SurfaceId,
                                        UserId = existingPlayerFacility.UserId,
                                        IsCaptain = false,
                                        IsPaid = true, //Free Game
                                        PricePerUser = default,
                                        IsEnabled = true,
                                        PitchStart = item.TimeStart,
                                        PitchEnd = item.TimeEnd,
                                        CreatedDate = DateTime.Now,
                                        Date = GetDate(item.Day, DateTime.Now),
                                        FacilityPitchTimingId = newTimingId,

                                        CreatedBy = IsUserLoggedIn.AdminId,
                                        IsFree = true
                                    };
                                    DbContext.UserPitchBookings.Add(newFreeBooking);

                                }
                            }
                        }
                    }

                    DbContext.FacilityPitchTimings.AddRange(pitchTimings);
                    facilityPitch.FacilityPitchTimingIds = string.IsNullOrWhiteSpace(facilityPitch.FacilityPitchTimingIds) ? pitchTimings[0].FacilityPitchTimingId.ToString() : facilityPitch.FacilityPitchTimingIds + ";" + pitchTimings[0].FacilityPitchTimingId.ToString();
                    DbContext.FacilityPitches.Update(facilityPitch);
                }
                else
                {
                    var getPitch = await DbContext.FacilityPitches.Where(f => f.FacilityPitchId == pitch.FacilityPitchId
                                                                        && f.FacilityId == pitch.FacilityId).FirstOrDefaultAsync();
                    var newPitch = new FacilityPitch
                    {
                        FacilityPitchId = pitch.FacilityPitchId,
                        FacilityId = pitch.FacilityId,
                        SportId = pitch.SportId,
                        Name = getPitch != null ? getPitch.Name : "Created through Superadmin",
                        Description = pitch.Description,
                        MaxPlayers = getPitch != null ? getPitch.MaxPlayers : 30,//default to Max Num Players

                        LastEditedBy = IsUserLoggedIn.AdminId,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = IsUserLoggedIn.AdminId,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsEnabledBy = IsUserLoggedIn.AdminId,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now
                    };

                    var checkFacilitySportExisting = await DbContext.FacilitySports.Where(f => f.IsEnabled == true && f.FacilityId == pitch.FacilityId && f.SportId == pitch.SportId).FirstOrDefaultAsync();
                    if (checkFacilitySportExisting == null)
                    {
                        var newSport = new FacilitySport
                        {
                            FacilityId = pitch.FacilityId.GetValueOrDefault(),
                            SportId = pitch.SportId,

                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now
                        };

                        DbContext.FacilitySports.Add(newSport);
                    }

                    var facilityPlayers = await DbContext.FacilityPlayers.ToListAsync();
                    var timings = new List<FacilityPitchTiming>();
                    foreach (var timing in pitch.FacilityPitchTimings)
                    {
                        var newTimingId = Guid.NewGuid();
                        timings.Add(new FacilityPitchTiming
                        {
                            FacilityPitchTimingId = newTimingId,
                            FacilityPitchId = pitch.FacilityPitchId.Value,
                            TimeStart = timing.TimeStart,
                            TimeEnd = timing.TimeEnd,
                            CustomPrice = timing.CustomPrice,
                            IsFree = timing.IsFree,
                            Date = timing.Date,
                            Day = timing.Day,
                            IsRepeatEveryWeek = true,
                            PlayerIds = timing.PlayerIds,

                            LastEditedBy = IsUserLoggedIn.AdminId,
                            LastEditedDate = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.AdminId,
                            CreatedDate = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.AdminId,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now
                        });

                        var playerIds = timing.PlayerIds.Split(";");
                        foreach (var playerId in playerIds)
                        {
                            if (!string.IsNullOrWhiteSpace(playerId))
                            {
                                var existingPlayerFacility = facilityPlayers.Where(f => f.FacilityId == pitch.FacilityId && f.UserId == new Guid(playerId)).FirstOrDefault();
                                if (existingPlayerFacility == null)
                                {
                                    var playerInfo = facilityPlayers.Where(f => f.UserId == new Guid(playerId)).FirstOrDefault();
                                    if (playerInfo != null)
                                    {
                                        var bookingIdGuid = Guid.NewGuid();
                                        var facilityPlayer = new FacilityPlayer()
                                        {
                                            UserId = playerInfo.UserId,
                                            Name = playerInfo.Name,
                                            FirstName = playerInfo.FirstName,
                                            LastName = playerInfo.LastName,
                                            AreaId = playerInfo.AreaId,
                                            Email = playerInfo.Email,
                                            ProfileImgUrl = playerInfo.ProfileImgUrl,
                                            ContactNumber = playerInfo.ContactNumber,
                                            FacilityId = pitch.FacilityId.GetValueOrDefault(),
                                            BookingId = bookingIdGuid,

                                            LastEditedBy = IsUserLoggedIn.AdminId,
                                            LastEditedDate = DateTime.Now,
                                            DateCreated = DateTime.Now,
                                            CreatedBy = IsUserLoggedIn.AdminId,
                                            CreatedDate = DateTime.Now,
                                            IsEnabled = true,
                                            IsEnabledBy = IsUserLoggedIn.AdminId,
                                            DateEnabled = DateTime.Now,
                                            IsLocked = false,
                                            LockedDateTime = DateTime.Now,
                                        };

                                        DbContext.FacilityPlayers.Add(facilityPlayer);

                                        var newFreeBooking = new UserPitchBooking
                                        {
                                            BookingId = bookingIdGuid,
                                            FacilityId = pitch.FacilityId.GetValueOrDefault(),
                                            Name = facility.Name,
                                            Description = pitch.Description,
                                            FacilityPitchId = pitch.FacilityPitchId.GetValueOrDefault(),
                                            SportId = pitch.SportId,
                                            AreaId = facility.Area.AreaId,
                                            City = facility.City,
                                            SurfaceId = facilityPitch.SurfaceId,
                                            UserId = playerInfo.UserId,
                                            IsCaptain = false,
                                            IsPaid = true, //Free Game
                                            PricePerUser = default,
                                            IsEnabled = true,
                                            PitchStart = timing.TimeStart,
                                            PitchEnd = timing.TimeEnd,
                                            CreatedDate = DateTime.Now,
                                            Date = GetDate(timing.Day, DateTime.Now),
                                            FacilityPitchTimingId = newTimingId,

                                            CreatedBy = IsUserLoggedIn.AdminId,
                                            IsFree = true
                                        };
                                        DbContext.UserPitchBookings.Add(newFreeBooking);
                                    }
                                }
                                else
                                {
                                    var newFreeBooking = new UserPitchBooking
                                    {
                                        BookingId = Guid.NewGuid(),
                                        FacilityId = pitch.FacilityId.GetValueOrDefault(),
                                        Name = facility.Name,
                                        Description = pitch.Description,
                                        FacilityPitchId = pitch.FacilityPitchId.GetValueOrDefault(),
                                        SportId = pitch.SportId,
                                        AreaId = facility.Area.AreaId,
                                        City = facility.City,
                                        //SurfaceId = facilityPitch.SurfaceId != null ? facilityPitch.SurfaceId : Guid.Empty,
                                        UserId = existingPlayerFacility.UserId,
                                        IsCaptain = false,
                                        IsPaid = true, //Free Game
                                        PricePerUser = default,
                                        IsEnabled = true,
                                        PitchStart = timing.TimeStart,
                                        PitchEnd = timing.TimeEnd,
                                        CreatedDate = DateTime.Now,
                                        Date = GetDate(timing.Day, DateTime.Now),
                                        FacilityPitchTimingId = newTimingId,

                                        CreatedBy = IsUserLoggedIn.AdminId,
                                        IsFree = true
                                    };
                                    DbContext.UserPitchBookings.Add(newFreeBooking);


                                }
                            }
                        }
                    }

                    DbContext.FacilityPitchTimings.AddRange(timings);
                    newPitch.FacilityPitchTimingIds = timings[0].FacilityPitchTimingId.ToString();
                    DbContext.FacilityPitches.Add(newPitch);

                }

                await DbContext.SaveChangesAsync();
                return new APIResponse
                {
                    Message = "Successfully Created Slot!",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::CreateSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> GetAllFacilityPitchTiming()
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetAllFacilityPitchTiming --");
            try
            {
                var facilityPitches = await DbContext.FacilityPitches.ToListAsync();
                var list = new List<AddSlotViewModel>();
                foreach (var facility in facilityPitches)
                {
                    if (facility.FacilityPitchTimingIds != null && !string.IsNullOrWhiteSpace(facility.FacilityPitchTimingIds))
                    {
                        var facilityPitchTimingId = facility.FacilityPitchTimingIds.Split(";");
                        foreach (var item in facilityPitchTimingId)
                        {
                            var facilityPitchTimings = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == item).FirstOrDefaultAsync();
                            if (facilityPitchTimings != null)
                            {
                                list.Add(new AddSlotViewModel
                                {
                                    FacilityId = facility.FacilityId.Value,
                                    SportId = facility.SportId,
                                    FacilityPitchId = facility.FacilityPitchId.Value,
                                    MaxPlayers = facility.MaxPlayers,
                                    Date = facilityPitchTimings.Date,
                                    Start = facilityPitchTimings.TimeStart,
                                    End = facilityPitchTimings.TimeEnd,
                                    TotalPrice = facilityPitchTimings.CustomPrice,
                                    PlayerIds = facilityPitchTimings.PlayerIds,
                                    FacilityPitchTimingId = facilityPitchTimings.FacilityPitchTimingId,
                                    IsFree = facilityPitchTimings.IsFree,
                                    DateUpdated = facilityPitchTimings.LastEditedDate.Value
                                });
                            }

                        }
                    }
                }

                foreach (var timing in list)
                {
                    timing.PlayerCount = GetPlayerCount(timing.PlayerIds);
                    var commissions = DbContext.ComissionPlays.Where(c => c.SportId == timing.SportId).FirstOrDefault();
                    if (commissions != null)
                    {
                        timing.Commissions = commissions.ComissionPerPlayer;
                    }

                    var facility = DbContext.Facilities.Where(f => f.FacilityId == timing.FacilityId).FirstOrDefault();
                    if (facility != null)
                    {
                        timing.FacilityName = facility.Name;
                    }
                }


                return new APIResponse
                {
                    Message = "Retrieved Facility Pitch Timings",
                    Payload = list,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetAllFacilityPitchTiming --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> GetAllFacilityPitchBooking()
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetAllFacilityPitchBooking --");
            try
            {
                var facilitybooking = await DbContext.UserPitchBookings.ToListAsync();

                var list = from u in DbContext.UserPitchBookings
                            join f in DbContext.Facilities
                                on u.FacilityId equals f.FacilityId
                            join fp in DbContext.FacilityPitches
                                on u.FacilityPitchId equals fp.FacilityPitchId
                           select new AddSlotViewModel
                            {
                                FacilityId = u.FacilityId,
                                SportId = u.SportId,
                                FacilityName = f.Name,
                                FacilityPitchId = u.FacilityPitchId,
                                MaxPlayers = fp.MaxPlayers,
                                Date = u.Date,
                                Start = u.PitchStart,
                                End = u.PitchEnd,
                                TotalPrice = (decimal)u.PricePerUserVat,
                                PlayerIds = null,
                                PlayerCount = DbContext.FacilityPlayers.Where(f => f.PlayerStatus == EGamePlayerStatus.Approved && f.BookingId == u.BookingId).Count(),
                                FacilityPitchTimingId = u.FacilityPitchTimingId,
                                IsFree = u.IsPaid == true,
                                DateUpdated = u.LastEditedDate.GetValueOrDefault()
                            };

                return new APIResponse
                {
                    Message = "Retrieved Facility Pitch Timings",
                    Payload = list,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetAllFacilityPitchTiming --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        private int GetMaxPlayers(List<FacilityPitch> facilityPitches, Guid facilityPitchId, Guid facilityId, Guid sportId)
        {
            if (facilityPitches.Count != 0)
            {
                var facilityPitch = facilityPitches.Where(f => f.FacilityPitchId == facilityPitchId
                                                            && f.FacilityId == facilityId
                                                            && f.SportId == sportId).FirstOrDefault();
                if (facilityPitch != null)
                {
                    return facilityPitch.MaxPlayers;
                }

            }

            return default;
        }

        private Guid GetFacilityId(List<FacilityPitch> facilityPitches, Guid facilityPitchId)
        {
            if (facilityPitches.Count != 0)
            {
                var facilityPitch = facilityPitches.Where(f => f.FacilityPitchId == facilityPitchId).FirstOrDefault();
                if (facilityPitch != null)
                {
                    return facilityPitch.FacilityId.Value;
                }

            }

            return Guid.Empty;
        }

        private int GetPlayerCount(string playerIds)
        {
            if (string.IsNullOrWhiteSpace(playerIds))
            {
                return default;
            }

            var playerCount = new string[] { "" };
            playerCount = playerIds.Split(";");

            return playerCount.Length;
        }

        public async Task<APIResponse> GetSlot(Guid facilityPitchTimingId)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetSlot --");
            LogManager.LogDebugObject(facilityPitchTimingId);

            try
            {
                var viewModel = new EditSlotViewModel();
                var facilityPitchTiming = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId && f.IsEnabled == true).FirstOrDefaultAsync();
                if (facilityPitchTiming != null)
                {
                    viewModel.Date = facilityPitchTiming.Date;
                    viewModel.Start = facilityPitchTiming.TimeStart;
                    viewModel.End = facilityPitchTiming.TimeEnd;
                    viewModel.IsRepeatEveryWeek = facilityPitchTiming.IsRepeatEveryWeek;
                    viewModel.Price = facilityPitchTiming.CustomPrice;
                    viewModel.FacilityPitchTimingId = facilityPitchTiming.FacilityPitchTimingId;
                    viewModel.Day = facilityPitchTiming.Day;
                    viewModel.IsFree = facilityPitchTiming.IsFree;

                    viewModel.FacilityPitches = await DbContext.FacilityPitches.Where(f => f.IsEnabled == true).ToListAsync();
                    foreach (var facilityPitch in viewModel.FacilityPitches)
                    {
                        if (facilityPitch.FacilityPitchTimingIds != null && !string.IsNullOrWhiteSpace(facilityPitch.FacilityPitchTimingIds))
                        {
                            var facilityTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                            foreach (var item in facilityTimingIds)
                            {
                                if (item == facilityPitchTimingId.ToString())
                                {
                                    viewModel.MaxPlayers = facilityPitch.MaxPlayers;
                                    viewModel.SportId = facilityPitch.SportId;
                                    viewModel.FacilityPitchId = facilityPitch.FacilityPitchId.Value;
                                    viewModel.FacilityPitchIdTable = facilityPitch.Id;

                                    var facility = await DbContext.Facilities.Where(f => f.FacilityId == facilityPitch.FacilityId && f.IsEnabled == true).Include(f => f.Area).FirstOrDefaultAsync();
                                    if (facility != null)
                                    {
                                        viewModel.FacilityName = facility.Name;
                                        viewModel.FacilityImgUrl = facility.ImageUrl;
                                        viewModel.FacilityId = facility.FacilityId;
                                        viewModel.Area = facility.Area.AreaName;
                                    }

                                }
                            }
                        }

                    }

                    viewModel.Players = new List<FacilityPlayer>();
                    if (!string.IsNullOrWhiteSpace(facilityPitchTiming.PlayerIds))
                    {
                        GetPlayers(viewModel.Players, facilityPitchTiming.PlayerIds, viewModel.FacilityId);
                    }

                    return new APIResponse
                    {
                        Message = "Retrieved Facility Pitch Timing",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = viewModel
                    };
                }

                return new APIResponse
                {
                    Message = "Error in Getting the Slot",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        private void GetPlayers(List<FacilityPlayer> players, string playerIds, Guid facilityId)
        {
            var playerList = new string[] { "" };
            playerList = playerIds.Split(";");

            foreach (var item in playerList)
            {
                var facilityPlayer = DbContext.FacilityPlayers.Where(f => f.UserId.ToString() == item && f.IsEnabled == true && f.FacilityId == facilityId).FirstOrDefault();
                if (facilityPlayer != null)
                {
                    players.Add(new FacilityPlayer
                    {
                        Name = $"{facilityPlayer.FirstName} {facilityPlayer.LastName}",
                        ProfileImgUrl = facilityPlayer.ProfileImgUrl,
                        IsPaid = facilityPlayer.IsPaid
                    });
                }
            }
        }

        public async Task<APIResponse> UpdateSlot(string auth, EditSlotViewModel viewModel)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::UpdateSlot --");
            LogManager.LogDebugObject(viewModel);
            try
            {
                //TODO Validation in Slots
                var IsUserLoggedIn = await DbContext.AdminLoginTransactions.AsNoTracking().FirstOrDefaultAsync(ult => ult.Token == auth && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return new APIResponse
                    {
                        Message = "Unathorized access.",
                        Status = "Failed!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }
                var facilityPitchTiming = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId == viewModel.FacilityPitchTimingId && f.IsEnabled == true).FirstOrDefaultAsync();
                if (facilityPitchTiming == null)
                {
                    return new APIResponse
                    {
                        Message = "Slot is not existing!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }




                var unavailableSlots = await DbContext.UnavailableSlots.Where(u => u.FacilityId == viewModel.FacilityId).ToListAsync();
                var facilityPitchTimings = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchId == viewModel.FacilityPitchId && f.FacilityPitchTimingId != viewModel.FacilityPitchTimingId).ToListAsync();
                var date = GetDate(viewModel.Day, DateTime.Now);

                var pitchStart = date.Date.AddHours(viewModel.Start.Hour).AddMinutes(viewModel.Start.Minute);
                var pitchEnd = date.Date.AddHours(viewModel.End.Hour).AddMinutes(viewModel.End.Minute);
                //var overlappedSlotsForAllPitches = unavailableSlots.Where(u => u.Starts < pitchEnd && pitchStart < u.Ends && u.AllPitches == true).ToList();
                //if (overlappedSlotsForAllPitches.Any())
                //{
                //    return new APIResponse
                //    {
                //        Message = "Slot is already occupied",
                //        StatusCode = System.Net.HttpStatusCode.BadRequest
                //    };
                //}

                var overlappedSlots = unavailableSlots.Where(u => u.FacilityPitchId == viewModel.FacilityPitchId).ToList();
                foreach (var item in overlappedSlots)
                {
                    if ((int)item.Starts.DayOfWeek == pitchStart.Day && item.Starts.TimeOfDay < pitchEnd.TimeOfDay
                         && (int)item.Ends.DayOfWeek == pitchEnd.Day && pitchStart.TimeOfDay < item.Ends.TimeOfDay)
                    {
                        return new APIResponse
                        {
                            Message = "There is an overlapping schedule with the Slot",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }

                foreach (var blockSlot in unavailableSlots.Where(u => u.RepeatEveryWeek == true && u.AllPitches == true).ToList())
                {
                    if ((int)blockSlot.Starts.DayOfWeek == pitchStart.Day && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && (int)blockSlot.Ends.DayOfWeek == pitchEnd.Day && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                    {
                        return new APIResponse
                        {
                            Message = "There is an overlapping schedule with the Slot",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }

                foreach (var blockSlot in unavailableSlots.Where(u => u.RepeatEveryWeek == true && u.FacilityPitchId == viewModel.FacilityPitchId).ToList())
                {
                    if ((int)blockSlot.Starts.DayOfWeek == pitchStart.Day && blockSlot.Starts.TimeOfDay < pitchEnd.TimeOfDay
                        && (int)blockSlot.Ends.DayOfWeek == pitchEnd.Day && pitchStart.TimeOfDay < blockSlot.Ends.TimeOfDay)
                    {
                        return new APIResponse
                        {
                            Message = "There is an overlapping schedule with the Slot",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }


                //var facilityPitchTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                //foreach (var timing in facilityPitchTimingIds)
                //{
                //    var timingModel = facilityPitchTimings.Where(f => f.FacilityPitchTimingId.ToString() == timing).FirstOrDefault();
                //    if (timingModel != null)
                //    {
                //        var pitchStartTiming = timingModel.Date.Date.AddHours(timingModel.TimeStart.Hour).AddMinutes(timingModel.TimeStart.Minute);
                //        var pitchEndTiming = timingModel.Date.Date.AddHours(timingModel.TimeEnd.Hour).AddMinutes(timingModel.TimeEnd.Minute);
                //        if (pitchStartTiming.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && pitchStartTiming.TimeOfDay < pitchEnd.TimeOfDay
                //        && pitchEndTiming.DayOfWeek == pitch.FacilityPitchTimings.FirstOrDefault().Day && pitchStart.TimeOfDay < pitchEndTiming.TimeOfDay)
                //        {
                //            return new APIResponse
                //            {
                //                Message = "Slot is Occupied",
                //                StatusCode = System.Net.HttpStatusCode.BadRequest
                //            };
                //        }
                //    }
                //}

                var pitchTimings = new List<FacilityPitchTiming>();
                foreach (var item in facilityPitchTimings)
                {
                    var pitchStartTiming = item.Date.AddHours(item.TimeStart.Hour).AddMinutes(item.TimeStart.Minute);
                    var pitchEndTiming = item.Date.AddHours(item.TimeEnd.Hour).AddMinutes(item.TimeEnd.Minute);

                    if (pitchStartTiming.DayOfWeek == viewModel.Day && pitchStartTiming.TimeOfDay < pitchEnd.TimeOfDay
                        && pitchEndTiming.DayOfWeek == viewModel.Day && pitchStart.TimeOfDay < pitchEndTiming.TimeOfDay)
                    {
                        return new APIResponse
                        {
                            Message = "There is an overlapping schedule with the Slot",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }

                }

                facilityPitchTiming.FacilityPitchId = viewModel.FacilityPitchId;
                facilityPitchTiming.Date = viewModel.Date;
                facilityPitchTiming.Day = viewModel.Day;
                facilityPitchTiming.TimeStart = viewModel.Start;
                facilityPitchTiming.TimeEnd = viewModel.End;
                facilityPitchTiming.CustomPrice = viewModel.Price;
                facilityPitchTiming.IsRepeatEveryWeek = viewModel.IsRepeatEveryWeek;

                facilityPitchTiming.LastEditedDate = DateTime.Now;
                facilityPitchTiming.LastEditedBy = IsUserLoggedIn.AdminId;

                DbContext.FacilityPitchTimings.Update(facilityPitchTiming);

                var facilityPitch = await DbContext.FacilityPitches.Where(f => f.FacilityPitchId == viewModel.FacilityPitchId
                                                                            && f.FacilityId == viewModel.FacilityId
                                                                            && f.SportId == viewModel.SportId
                                                                            && f.IsEnabled == true).ToListAsync();

                if (facilityPitch.Any())
                {

                    var listGuidIds = new List<string>();
                    var IdsToRemoved = new List<string>();
                    foreach (var item in facilityPitch)
                    {
                        var Ids = item.FacilityPitchTimingIds.Split(";");
                        var isExistingFacilityTimingId = Ids.Where(Ids => Ids == facilityPitchTiming.FacilityPitchTimingId.ToString()).ToList();
                        if (isExistingFacilityTimingId.Any())
                        {
                            var existingPitch = await DbContext.FacilityPitches.Where(f => f.Id == viewModel.FacilityPitchIdTable && f.IsEnabled == true).FirstOrDefaultAsync();
                            if (existingPitch != null)
                            {
                                var timingIds = existingPitch.FacilityPitchTimingIds.Split(";");
                                var toRemove = timingIds.Where(timingIds => timingIds == facilityPitchTiming.FacilityPitchTimingId.ToString()).FirstOrDefault();
                                if (toRemove != null)
                                {
                                    IdsToRemoved.Add(toRemove);

                                }
                                var toRetain = timingIds.Where(timingIds => timingIds != facilityPitchTiming.FacilityPitchTimingId.ToString()).ToList();

                                existingPitch.FacilityPitchTimingIds = toRetain.Count != 0 ? string.Join(";", toRetain) : string.Empty;


                                item.FacilityPitchTimingIds = string.IsNullOrWhiteSpace(item.FacilityPitchTimingIds) ? facilityPitchTiming.FacilityPitchTimingId.ToString() : item.FacilityPitchTimingIds + ";" + facilityPitchTiming.FacilityPitchTimingId;
                                listGuidIds = new List<string>();

                            }
                            DbContext.FacilityPitches.Update(existingPitch);
                        }
                        else
                        {
                            var existingPitch = await DbContext.FacilityPitches.Where(f => f.Id == viewModel.FacilityPitchIdTable && f.IsEnabled == true).FirstOrDefaultAsync();
                            var IdList = existingPitch.FacilityPitchTimingIds.Split(";");
                            foreach (var Id in IdList)
                            {
                                if (Id != facilityPitchTiming.FacilityPitchTimingId.ToString())
                                {
                                    listGuidIds.Add(Id);
                                }
                            }

                            existingPitch.FacilityPitchTimingIds = listGuidIds.Count != 0 ? string.Join(";", listGuidIds) : string.Empty;
                            item.FacilityPitchTimingIds = string.IsNullOrWhiteSpace(item.FacilityPitchTimingIds) ? facilityPitchTiming.FacilityPitchTimingId.ToString() : item.FacilityPitchTimingIds + ";" + facilityPitchTiming.FacilityPitchTimingId;
                            listGuidIds = new List<string>();
                            DbContext.FacilityPitches.Update(existingPitch);
                        }
                    }

                    DbContext.FacilityPitches.UpdateRange(facilityPitch);
                }
                else
                {
                    var existingPitch = await DbContext.FacilityPitches.Where(f => f.Id == viewModel.FacilityPitchIdTable && f.IsEnabled == true).FirstOrDefaultAsync();
                    if (existingPitch != null)
                    {
                        if (existingPitch.FacilityPitchId.Value == viewModel.FacilityPitchId || existingPitch.SportId == viewModel.SportId)
                        {
                            var getPitch = await DbContext.FacilityPitches.Where(f => f.FacilityPitchId == viewModel.FacilityPitchId && f.IsEnabled == true).FirstOrDefaultAsync();

                            var newPitch = new FacilityPitch
                            {
                                FacilityPitchId = viewModel.FacilityPitchId,
                                FacilityId = viewModel.FacilityId,
                                SportId = viewModel.SportId,
                                Name = getPitch != null ? getPitch.Name : string.Empty,
                                MaxPlayers = getPitch != null ? getPitch.MaxPlayers : 30,//default to Max Num Players
                                FacilityPitchTimingIds = facilityPitchTiming.FacilityPitchTimingId.ToString(),

                                LastEditedBy = IsUserLoggedIn.AdminId,
                                LastEditedDate = DateTime.Now,
                                CreatedBy = IsUserLoggedIn.AdminId,
                                CreatedDate = DateTime.Now,
                                IsEnabled = true,
                                IsEnabledBy = IsUserLoggedIn.AdminId,
                                DateEnabled = DateTime.Now,
                                IsLocked = false,
                                LockedDateTime = DateTime.Now
                            };

                            var listGuidIds = existingPitch.FacilityPitchTimingIds.Split(";");
                            var newIds = new List<string>();
                            foreach (var item in listGuidIds)
                            {
                                if (item != newPitch.FacilityPitchTimingIds)
                                {
                                    newIds.Add(item);
                                }
                            }

                            existingPitch.FacilityPitchTimingIds = string.Join(";", newIds);

                            DbContext.FacilityPitches.Update(existingPitch);


                            DbContext.FacilityPitches.Add(newPitch);
                        }
                    }
                }

                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Successfully Updated Slot",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::UpdateSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> DeleteSlot(ChangeStatus status)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::UpdateSlot --");
            LogManager.LogDebugObject(status);

            try
            {
                var facilityPitchTiming = await DbContext.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId == status.GuID).FirstOrDefaultAsync();
                if (facilityPitchTiming == null)
                {
                    return new APIResponse
                    {
                        Message = "Slot is not existing!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                var facilityPitches = await DbContext.FacilityPitches.ToListAsync();
                var facilityPitchList = new List<FacilityPitch>();
                var Ids = new List<string>();
                foreach (var item in facilityPitches)
                {
                    if (item.FacilityPitchTimingIds != null && !string.IsNullOrWhiteSpace(item.FacilityPitchTimingIds))
                    {
                        var GuidIds = item.FacilityPitchTimingIds.Split(";");
                        foreach (var pitchTimingId in GuidIds)
                        {
                            if (pitchTimingId != facilityPitchTiming.FacilityPitchTimingId.ToString())
                            {
                                Ids.Add(pitchTimingId);
                            }
                        }
                    }


                    item.FacilityPitchTimingIds = String.Join(";", Ids);
                    Ids = new List<string>();
                }

                DbContext.FacilityPitches.UpdateRange(facilityPitches);

                DbContext.FacilityPitchTimings.Remove(facilityPitchTiming);

                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Successfully Removed Slot",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::DeleteSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> DeleteUnavailableSlot(ChangeStatus status)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetUnavailableSlot --");
            LogManager.LogDebugObject(status);

            try
            {
                var unavailableSlot = await DbContext.UnavailableSlots.Where(u => u.UnavailableSlotId == status.GuID).FirstOrDefaultAsync();
                if (unavailableSlot != null)
                {
                    DbContext.UnavailableSlots.Remove(unavailableSlot);
                    await DbContext.SaveChangesAsync();

                    return new APIResponse
                    {
                        Message = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK,
                    };
                }

                return new APIResponse
                {
                    Message = "Id not existing",
                    StatusCode = System.Net.HttpStatusCode.NotFound
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetUnavailableSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> GetUnavailableSlot(Guid unavailableSlotId)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetUnavailableSlot --");
            LogManager.LogDebugObject(unavailableSlotId);

            try
            {
                var blockedSlot = await DbContext.UnavailableSlots.Where(u => u.UnavailableSlotId == unavailableSlotId).FirstOrDefaultAsync();
                if (blockedSlot != null)
                {
                    return new APIResponse
                    {
                        Message = "Retrieved Slot",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = blockedSlot
                    };
                }

                var facilityPitchTiming = await DbContext.FacilityPitchTimings.Where(u => u.FacilityPitchTimingId == unavailableSlotId).FirstOrDefaultAsync();
                var facilityPitches = await DbContext.FacilityPitches.Where(f => f.FacilityPitchId == facilityPitchTiming.FacilityPitchId && f.IsEnabled == true).ToListAsync();
                var facilities = await DbContext.Facilities.Include(a => a.Area).Where(f => f.IsEnabled == true).ToListAsync();
                var sports = await DbContext.Sports.ToListAsync();
                var areas = await DbContext.Areas.Where(f => f.IsEnabled == true).ToListAsync();
                if (facilityPitchTiming != null)
                {
                    foreach (var facilityPitch in facilityPitches)
                    {
                        if (facilityPitch.FacilityPitchTimingIds != null && !string.IsNullOrWhiteSpace(facilityPitch.FacilityPitchTimingIds))
                        {
                            var facilityTimingIds = facilityPitch.FacilityPitchTimingIds.Split(";");
                            foreach (var facilityTimingId in facilityTimingIds)
                            {
                                if (facilityPitchTiming.FacilityPitchTimingId.ToString() == facilityTimingId)
                                {
                                    var timingCalendar = new TimingCalendarViewModel();
                                    var playerCount = facilityPitchTiming.PlayerIds.Split(";");
                                    timingCalendar.FacilityPitchName = facilityPitch.Name;
                                    timingCalendar.Description = facilityPitch.Description;
                                    timingCalendar.PlayerCount = $"{playerCount.Length} / {facilityPitch.MaxPlayers}";
                                    timingCalendar.FacilityPitchTimingId = facilityPitchTiming.FacilityPitchTimingId;
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

                                    timingCalendar.TimeStart = facilityPitchTiming.Date.AddHours(facilityPitchTiming.TimeStart.Hour).AddMinutes(facilityPitchTiming.TimeEnd.Minute);
                                    timingCalendar.TimeEnd = facilityPitchTiming.Date.AddHours(facilityPitchTiming.TimeEnd.Hour).AddMinutes(facilityPitchTiming.TimeEnd.Minute);
                                    timingCalendar.CustomPrice = facilityPitchTiming.CustomPrice;
                                    timingCalendar.IsRepeatEveryWeek = facilityPitchTiming.IsRepeatEveryWeek;

                                    return new APIResponse
                                    {
                                        Message = "Retrieved Slot",
                                        StatusCode = System.Net.HttpStatusCode.OK,
                                        Payload = timingCalendar
                                    };
                                }
                            }
                        }

                    }
                }
                if (facilityPitchTiming != null)
                {
                    return new APIResponse
                    {
                        Message = "Retrieved Slot",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = facilityPitchTiming
                    };
                }

                return new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Message = "Unavailable Slot not Existing"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetUnavailableSlot --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> GetFacilityPitchesByFacilityId(Guid facilityId, Guid facilityPitchId)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPitchRepository::GetFacilityPitchesByFacilityId --");
            LogManager.LogDebugObject(facilityId);
            try
            {
                var facilityPitches = await DbContext.FacilityPitches.Where(f => f.FacilityId == facilityId && f.FacilityPitchId == facilityPitchId).ToListAsync();
                return new APIResponse
                {
                    Message = "Retrieved Facility Pitches by Facility",
                    Payload = facilityPitches,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPitchRepository::GetFacilityPitchesByFacilityId --");
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
