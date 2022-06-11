using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.FireBase;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.Service.IService;
using Sidekick.Model;
using Sidekick.Model.Class;
using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using Sidekick.Model.Play;
using Sidekick.Model.SetupConfiguration.Level;
using Sidekick.Model.SetupConfiguration.Size;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class PlayRepository : APIBaseRepo, IPlayRepository
    {
        private readonly APIDBContext context;
        private APIConfigurationManager APIConfig { get; set; }
        private IUserNotificationHandler iUserNotificationHandler;
        private IUserHandler userHandler;
        private readonly ITelRService telRService;
        private readonly IUserHelper userHelper;
        private readonly IPushNotificationTemplateRepository pushNotificationTemplateRepository;
        ILoggerManager LogManager { get; }
        private readonly IUserDevicesRepository userDevicesRepository;

        public PlayRepository(APIDBContext context, ILoggerManager _logManager, IUserHandler userHandler, APIConfigurationManager _apiconfig, IUserNotificationHandler iUserNotificationHandler,
            IPushNotificationTemplateRepository pushNotificationTemplateRepository, IUserDevicesRepository userDevicesRepository, IUserHelper userHelper, ITelRService telRService)
        {
            this.context = context;
            this.userHandler = userHandler;
            this.iUserNotificationHandler = iUserNotificationHandler;
            LogManager = _logManager;
            APIConfig = _apiconfig;
            this.telRService = telRService;
            this.pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            this.userDevicesRepository = userDevicesRepository;
            this.userHelper = userHelper;
        }

        public async Task<IEnumerable<PlayFacilitiesModel>> FilterFacility(IEnumerable<PlayFilterViewModel> filters, Guid _sportId, string facilityName)
        {
            LogManager.LogInfo("-- Run::PlayRepository::FilterFacility --");
            LogManager.LogDebugObject(_sportId);
            var playFacilitiesModel = new List<PlayFacilitiesModel>();
            //filter facility sports
            var facilitiesBySport = context.FacilitySports
            .Where(x => x.IsEnabled == true && x.SportId == _sportId)
            .Select(i => i.FacilityId);

            IEnumerable<Facility> facilities = context.Facilities.Where(x => x.IsEnabled == true && facilitiesBySport.Contains(x.FacilityId) && x.Name.Contains(facilityName ?? x.Name)).Select(i => new Facility()
            {
                FacilityId = i.FacilityId,
                Name = i.Name,
                Street = i.Street,
                Area = i.Area,
                City = i.City,
                Country = i.Country,
                CreatedDate = i.CreatedDate,
                IsEnabled = i.IsEnabled,
                ImageUrl = i.ImageUrl,
                Latitude = i.Latitude,
                Longitude = i.Longitude
            }).ToList();

            //var facilityIdList = facilities.Select(i => i.FacilityId).ToList();

            //get facility pitches per facilityIds and filters


            foreach (var facility in facilities)
            {
                IEnumerable<FacilityPitch> facilityPitches = context.FacilityPitches
                .Where(x => x.IsEnabled == true && x.FacilityId.Value == facility.FacilityId)
                .Select(i => new FacilityPitch()
                {
                    FacilityId = i.FacilityId,
                    FacilityPitchId = i.FacilityPitchId,
                    SportId = i.SportId,
                    ImageUrl = facility.ImageUrl,
                    CreatedDate = i.CreatedDate,
                    IsEnabled = i.IsEnabled,
                    Name = i.Name,
                    AreaId = i.AreaId,
                    SurfaceId = i.SurfaceId,
                    LocationId = i.LocationId,
                    TeamSize = i.TeamSize,
                    FixedPrice = i.FixedPrice
                }).ToList();

                foreach (var filter in filters)
                {
                    if (filter.FilterType == FilterType.Area)
                    {
                        facilityPitches = facilityPitches.Where(x => x.AreaId == Guid.Parse(filter.FilterValue));
                    }
                    if (filter.FilterType == FilterType.Surface)
                    {
                        facilityPitches = facilityPitches.Where(x => x.SurfaceId == Guid.Parse(filter.FilterValue));
                    }
                    if (filter.FilterType == FilterType.TeamSize)
                    {
                        facilityPitches = facilityPitches.Where(x => x.TeamSize == Guid.Parse(filter.FilterValue));
                    }
                    if (filter.FilterType == FilterType.Location)
                    {
                        facilityPitches = facilityPitches.Where(x => x.LocationId == Guid.Parse(filter.FilterValue));
                    }
                    if (filter.FilterType == FilterType.Price)
                    {
                        var splPrice = filter.FilterValue.Split("-");
                        var firstValue = Convert.ToInt32(splPrice[0]);
                        var secondValue = Convert.ToInt32(splPrice[1]);

                        facilityPitches = facilityPitches.Where(x => x.FixedPrice <= firstValue && x.FixedPrice >= secondValue);
                    }
                }
                if (facilityPitches != null && facilityPitches.Any())
                {
                    var facilityModel = new PlayFacilitiesModel
                    {
                        FacilityId = facility.FacilityId,
                        Name = facility.Name,
                        Street = facility.Street,
                        Area = facility.AreaName,
                        Location = facility.Street + " " + facility.AreaName + " " + facility.City + " " + facility.Country,
                        City = facility.City,
                        CreatedDate = facility.CreatedDate,
                        IsEnabled = facility.IsEnabled,
                        FacilityImage = facility.ImageUrl,
                        PitchNo = facilityPitches.Count(),
                        Commission = 0 + "%",
                        FacilityPitches = facilityPitches,
                        Latitude = facility.Latitude,
                        Longitude = facility.Longitude
                    };
                    playFacilitiesModel.Add(facilityModel);
                }

            }

            return playFacilitiesModel;
        }

        public async Task<PlayFacilitiesViewModel> GetFacility(Guid _facilityId, DateTime bookDate, Guid _sportId)
        {
            LogManager.LogInfo("-- Run::PlayRepository::FilterFacility --");
            LogManager.LogDebugObject(_facilityId);
            var playFacilitiesModel = new List<PlayFacilitiesViewModel>();

            IEnumerable<Facility> facilities = context.Facilities.Where(x => x.IsEnabled == true && x.FacilityId == _facilityId).Select(i => new Facility()
            {
                FacilityId = i.FacilityId,
                Name = i.Name,
                Street = i.Street,
                AreaName = i.Area.AreaName,
                Area = i.Area,
                City = i.City,
                Country = i.Country,
                CreatedDate = i.CreatedDate,
                IsEnabled = i.IsEnabled,
                ImageUrl = i.ImageUrl,
                Description = i.Description
            }).ToList();

            //var facilityIdList = facilities.Select(i => i.FacilityId).ToList();

            //get facility pitches per facilityIds and filters


            foreach (var facility in facilities)
            {
                var weekDay = Helper.GetDayFromDayName(bookDate.DayOfWeek.ToString());

                var facilityPitches = (from facilitypitches in context.FacilityPitches
                                       where facilitypitches.IsEnabled == true && facilitypitches.FacilityId.Value == facility.FacilityId && facilitypitches.SportId == _sportId
                                       select new FacilityPitchDto()
                                       {
                                           FacilityId = facilitypitches.FacilityId,
                                           FacilityPitchId = facilitypitches.FacilityPitchId,
                                           SportId = facilitypitches.SportId,
                                           ImageUrl = facility.ImageUrl,
                                           Name = facilitypitches.Name,
                                           FixedPrice = facilitypitches.FixedPrice,
                                           IsFixedPrice = facilitypitches.IsFixedPrice,
                                           MaxPlayers = facilitypitches.MaxPlayers,
                                           FacilityPitchTimingIds = facilitypitches.FacilityPitchTimingIds,
                                           LocationId = facilitypitches.LocationId,
                                           SurfaceId = facilitypitches.SurfaceId,
                                           TeamSize = facilitypitches.TeamSize,
                                       });

                var listFacilityPitchIds = facilityPitches.Select(f => f.FacilityPitchId).ToList();

                var facilitytimings = context.FacilityTimings.FirstOrDefault(a => a.IsEveryday || (int)a.Day == (int)weekDay);
               
                var FacilityPitchTimings = (from facilityPitchTimings in context.FacilityPitchTimings
                                            where listFacilityPitchIds.Contains(facilityPitchTimings.FacilityPitchId)
                                            select new FacilityPitchTimings
                                            {
                                                FacilityPitchTimingId = facilityPitchTimings.FacilityPitchTimingId,
                                                TimeEnd = facilityPitchTimings.TimeEnd,
                                                CustomPrice = facilityPitchTimings.CustomPrice,
                                                Day = facilityPitchTimings.Day,
                                                IsFree = facilityPitchTimings.IsFree,
                                                TimeStart = facilityPitchTimings.TimeStart,
                                                FacilityPitchId = facilityPitchTimings.FacilityPitchId,
                                                IsBooked = context.UserPitchBookings.Where(a => a.FacilityPitchTimingId == facilityPitchTimings.FacilityPitchTimingId).Count() > 0 
                                            }).ToList();

                var facilitypitchtimings = new List<FacilityPitchDto>();
                var facilityPitchesList = await facilityPitches.ToListAsync();
                foreach (var itemfacilityPitches in facilityPitchesList)
                {
                    // get location,sport and surface name , teamsize.
                    string LocationName = string.Empty, SurfaceName = string.Empty, TeamSizeName = string.Empty, SportName = string.Empty;

                    Location location = context.Locations.FirstOrDefault(s => s.LocationId == itemfacilityPitches.LocationId);
                    if (location != null && !string.IsNullOrEmpty(location.Name))
                    {
                        LocationName = location.Name;
                    }

                    Sport sport = context.Sports.FirstOrDefault(s => s.SportId == itemfacilityPitches.SportId);
                    if (sport != null && !string.IsNullOrEmpty(sport.Name))
                    {
                        SportName = sport.Name;
                    }

                    Surface surface = context.Surfaces.FirstOrDefault(s => s.SurfaceId == itemfacilityPitches.SurfaceId);
                    if (surface != null && !string.IsNullOrEmpty(surface.Name))
                    {
                        SurfaceName = surface.Name;
                    }

                    TeamSize teamSize = context.Sizes.FirstOrDefault(s => s.SizeId == itemfacilityPitches.TeamSize);
                    if (teamSize != null && !string.IsNullOrEmpty(teamSize.SizeName))
                    {
                        TeamSizeName = teamSize.SizeName;
                    }

                    if (!string.IsNullOrEmpty(itemfacilityPitches.FacilityPitchTimingIds))
                    {
                        
                        if (weekDay != -1)
                        {
                            var listtimeIds = itemfacilityPitches.FacilityPitchTimingIds.Split(";");
                            foreach (var itemids in listtimeIds)
                            {
                                var myFacilityTiming = FacilityPitchTimings.Where(a => a.FacilityPitchTimingId == Guid.Parse(itemids) && (int)a.Day == (int)weekDay).ToList();
                                if (myFacilityTiming.Any())
                                {
                                    var facilitypitchObj = facilitypitchtimings.Where(f => f.FacilityPitchId == itemfacilityPitches.FacilityPitchId);
                                    if (facilitypitchObj != null && facilitypitchObj.Count() <= 0)
                                    {

                                        var facilitypitchtiming = new FacilityPitchDto
                                        {
                                            FacilityId = itemfacilityPitches.FacilityId,
                                            FacilityPitchTimings = myFacilityTiming,
                                            FacilityPitchId = itemfacilityPitches.FacilityPitchId,
                                            SportId = itemfacilityPitches.SportId,
                                            ImageUrl = itemfacilityPitches.ImageUrl,
                                            Name = itemfacilityPitches.Name,
                                            FixedPrice = itemfacilityPitches.FixedPrice,
                                            IsFixedPrice = itemfacilityPitches.IsFixedPrice,
                                            MaxPlayers = itemfacilityPitches.MaxPlayers,
                                            LocationId = itemfacilityPitches.LocationId,
                                            SurfaceId = itemfacilityPitches.SurfaceId,
                                            TeamSize = itemfacilityPitches.TeamSize,
                                            LocationName = LocationName,
                                            SurfaceName = SurfaceName,
                                            SportName = SportName,
                                            TeamSizeName = TeamSizeName
                                        };
                                        facilitypitchtimings.Add(facilitypitchtiming);
                                    }
                                    else
                                    {
                                        facilitypitchObj.FirstOrDefault().FacilityPitchTimings.AddRange(myFacilityTiming);
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        var facilitypitchtiming = new FacilityPitchDto
                        {
                            FacilityId = itemfacilityPitches.FacilityId,
                            FacilityPitchTimings = null,
                            FacilityPitchId = itemfacilityPitches.FacilityPitchId,
                            SportId = itemfacilityPitches.SportId,
                            ImageUrl = itemfacilityPitches.ImageUrl,
                            Name = itemfacilityPitches.Name,
                            FixedPrice = itemfacilityPitches.FixedPrice,
                            IsFixedPrice = itemfacilityPitches.IsFixedPrice,
                            MaxPlayers = itemfacilityPitches.MaxPlayers,
                            LocationId = itemfacilityPitches.LocationId,
                            SurfaceId = itemfacilityPitches.SurfaceId,
                            TeamSize = itemfacilityPitches.TeamSize,
                            LocationName = LocationName,
                            SurfaceName = SurfaceName,
                            SportName = SportName,
                            TeamSizeName = TeamSizeName
                        };
                        facilitypitchtimings.Add(facilitypitchtiming);
                    }
                }

                var facilityModel = new PlayFacilitiesViewModel
                {
                    FacilityId = facility.FacilityId,
                    Name = facility.Name,
                    Street = facility.Street,
                    Area = facility.AreaName,
                    TimeStart = facilitytimings != null ? facilitytimings.TimeStart.ToShortTimeString() : string.Empty,
                    TimeEnd = facilitytimings != null ? facilitytimings.TimeEnd.ToShortTimeString() : string.Empty,
                    Description = facility.Description,
                    Location = facility.Street + " " + facility.AreaName + " " + facility.City + " " + facility.Country,
                    City = facility.City,
                    CreatedDate = facility.CreatedDate,
                    IsEnabled = facility.IsEnabled,
                    FacilityImage = facility.ImageUrl,
                    PitchNo = facilityPitches.Count(),
                    Commission = 0 + "%",
                    FacilityPitches = facilitypitchtimings,
                    AreaId = facility.Area.AreaId.ToString()
                };
                playFacilitiesModel.Add(facilityModel);
            }

            return playFacilitiesModel.FirstOrDefault();
        }

        public IEnumerable<PlayFacilitiesModel> SearchFacilityByName(string facilityName)
        {
            LogManager.LogInfo("-- Run::PlayRepository::SearchFacilityByName --");
            LogManager.LogDebugObject(facilityName);
            var playFacilitiesModel = new List<PlayFacilitiesModel>();

            IEnumerable<Facility> facilities = context.Facilities.Where(x => x.IsEnabled == true && x.Name.Contains(facilityName))
                .Select(i => new Facility()
                {
                    FacilityId = i.FacilityId,
                    Name = i.Name,
                    Street = i.Street,
                    Area = i.Area,
                    City = i.City,
                    Country = i.Country,
                    CreatedDate = i.CreatedDate,
                    IsEnabled = i.IsEnabled,
                    ImageUrl = i.ImageUrl
                }).ToList();

            //get facility pitches per facilityIds and filters
            foreach (var facility in facilities)
            {
                IEnumerable<FacilityPitch> facilityPitches = context.FacilityPitches
                .Where(x => x.IsEnabled == true && x.FacilityId.Value == facility.FacilityId)
                .Select(i => new FacilityPitch()
                {
                    FacilityId = i.FacilityId,
                    FacilityPitchId = i.FacilityPitchId,
                    SportId = i.SportId,
                    ImageUrl = facility.ImageUrl,
                    CreatedDate = i.CreatedDate,
                    IsEnabled = i.IsEnabled,
                    Name = i.Name
                }).ToList();

                var facilityModel = new PlayFacilitiesModel
                {
                    FacilityId = facility.FacilityId,
                    Name = facility.Name,
                    Street = facility.Street,
                    Area = facility.AreaName,
                    Location = facility.Street + " " + facility.AreaName + " " + facility.City + " " + facility.Country,
                    //City = facility.City,
                    CreatedDate = facility.CreatedDate,
                    IsEnabled = facility.IsEnabled,
                    FacilityImage = facility.ImageUrl,
                    Commission = 0 + "%",
                    PitchNo = facilityPitches.Count(),
                    FacilityPitches = facilityPitches

                };
                playFacilitiesModel.Add(facilityModel);
            }
            return playFacilitiesModel;
        }

        public async Task<APIResponse> SaveFreeGame(FreeGame freeGame)
        {
            LogManager.LogInfo("-- Run::PlayRepository::SaveFreeGame --");

            User user = context.Users.Where(x => x.UserId == freeGame.UserId).FirstOrDefault();
            if (user != null)
            {

                freeGame.GameId = Guid.NewGuid();
                freeGame.IsCaptain = true; //set requester to captain
                context.FreeGame.Add(freeGame);
                await context.SaveChangesAsync();
                return new APIResponse
                {
                    Message = "Free Game Saved",
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            else
            {
                return new APIResponse
                {
                    Message = "User Not Found",
                    Status = "Error!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<APIResponse> SubmitPlayRequest(PlayRequest playRequest, Guid userId, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            LogManager.LogInfo("-- Run::PlayRepository::SubmitPlayRequest --");
            LogManager.LogDebugObject(playRequest.UserId);
            PlayRequestResposeModel playRequestResposeModel = new PlayRequestResposeModel();
            var booking = new UserPitchBooking();
            var facility = new Facility();
            FacilityPlayer request = context.FacilityPlayers.Where(x => x.UserId == playRequest.UserId && x.BookingId == playRequest.BookingId).FirstOrDefault();
            if (request == null)
            {
                booking = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId).FirstOrDefault();
                if (booking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                User user = context.Users.Where(x => x.UserId == playRequest.UserId).FirstOrDefault();
                if (user == null)
                {
                    return new APIResponse
                    {
                        Message = "User not found!",
                        Status = "Error!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                var requestStatus = EGamePlayerStatus.Pending;
                int waitingStatus = 0;
                if (playRequest.Status == EGamePlayerStatus.Waiting)
                {
                    requestStatus = EGamePlayerStatus.Waiting;
                    waitingStatus = context.FacilityPlayers.Where(a => a.BookingId == playRequest.BookingId).Max(a => a.WaitingCount) + 1;
                }
                else if (!booking.IsPrivate)
                    requestStatus = EGamePlayerStatus.Approved;

                var gameplayers = new List<FacilityPlayerModel>();
                var gamePlayer = new FacilityPlayerModel();
                gamePlayer.BookingId = playRequest.BookingId;
                gamePlayer.IsCaptain = false;
                gamePlayer.FacilityId = booking.FacilityId;
                gamePlayer.ContactNumber = user.MobileNumber;
                gamePlayer.SportId = booking.SportId;
                gamePlayer.Email = user.Email;
                gamePlayer.ProfileImgUrl = user.ImageUrl;
                gamePlayer.AreaId = booking.AreaId;
                gamePlayer.Name = user.FirstName + " " + user.LastName;
                gamePlayer.FirstName = user.FirstName;
                gamePlayer.LastName = user.LastName;
                gamePlayer.IsPaid = false;
                gamePlayer.WaitingCount = playRequest.Status == EGamePlayerStatus.Waiting ? waitingStatus : 0;
                gamePlayer.PlayerStatus = requestStatus;
                gamePlayer.UserId = playRequest.UserId;
                gamePlayer.TotalAmount = (decimal)((booking.PricePerUser + booking.PricePerUser * 5 / 100 + booking.Commission) * booking.PlayerCount);
                gameplayers.Add(gamePlayer);
                await AddEditGamePlayers(gameplayers, playRequest.BookingId, playRequest.UserId);

                int emailStatus = 0; //SendEmailToCaptain(playRequest, _mhttp, _conf);
                string emailStatusMessage = String.Empty;
                switch (emailStatus)
                {
                    case 0:
                        emailStatusMessage = "Email Request Sucessfully Sent to the Captain.";
                        break;
                    case 1:
                        emailStatusMessage = "Email Request not sent to the Captain.";
                        break;
                    default: break;
                }

                int emaiConfirmation = 0; //SendRequestConfirmation(playRequest, _mhttp, _conf);
                string emaiConfirmationMessage = String.Empty;
                switch (emaiConfirmation)
                {
                    case 0:
                        emaiConfirmationMessage = "Email Request Sucessfully Sent to the Requester.";
                        break;
                    case 1:
                        emaiConfirmationMessage = "Email Request not sent to the Requester.";
                        break;
                    default: break;
                }


                string locationName = "";
                facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId).FirstOrDefault();
                var FacilityPitches = context.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId).FirstOrDefault();
                if (facility != null)
                {
                    locationName = facility.Street + " " + facility.Area + " " + facility.City + " " + facility.Country;
                }

                var TeamSizeName = FacilityPitches != null ? context.Sizes.Where(u => u.SizeId == FacilityPitches.TeamSize && u.IsEnabled == true).Select(x => x.SizeName).FirstOrDefault() : string.Empty;

                var SurfaceName = FacilityPitches != null ? context.Surfaces.Where(u => u.SurfaceId == FacilityPitches.SurfaceId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault() : string.Empty;

                var SportName = context.Sports.Where(u => u.SportId == booking.SportId).Select(x => x.Name).FirstOrDefault();

                playRequestResposeModel.BookingId = booking.BookingId;
                playRequestResposeModel.UserImage = user.ImageUrl;
                playRequestResposeModel.FacilityId = booking.FacilityId;
                playRequestResposeModel.UserName = user.FirstName + " " + user.LastName;
                playRequestResposeModel.UserImage = user.ImageUrl;
                playRequestResposeModel.Location = locationName;
                playRequestResposeModel.SurfaceName = SurfaceName;
                playRequestResposeModel.TeamSizeName = TeamSizeName;
                playRequestResposeModel.SportId = booking.SportId;
                playRequestResposeModel.SportName = SportName;
                playRequestResposeModel.PitchStart = booking.PitchStart;
                playRequestResposeModel.PitchEnd = booking.PitchEnd;
                playRequestResposeModel.FacilityImage = facility != null ? facility.ImageUrl : string.Empty;
                playRequestResposeModel.FacilityName = facility != null ? facility.Name : string.Empty;
                playRequestResposeModel.UserId = user.UserId;

                playRequestResposeModel.PitchName = FacilityPitches != null ? FacilityPitches.Name : booking.Name;
                playRequestResposeModel.PriceIncludingVat = booking.PlayerCount * booking.PricePerUserVat.GetValueOrDefault();
                playRequestResposeModel.Date = booking.Date;
                playRequestResposeModel.SideKickCommission = booking.Commission.GetValueOrDefault();

                return new APIResponse
                {
                    Message = "Play Request Submitted," + emailStatusMessage + emaiConfirmationMessage,
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = playRequestResposeModel
                };
            }
            else
            {
                User user = context.Users.Where(x => x.UserId == playRequest.UserId).FirstOrDefault();
                if (user == null)
                {
                    return new APIResponse
                    {
                        Message = "User not found!",
                        Status = "Error!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                booking = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId).FirstOrDefault();
                if (booking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }
                string locationName = "";
                facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId).FirstOrDefault();
                var FacilityPitches = context.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId).FirstOrDefault();
                if (facility != null)
                {
                    locationName = facility.Street + " " + facility.Area + " " + facility.City + " " + facility.Country;
                }

                var TeamSizeName = FacilityPitches != null ? context.Sizes.Where(u => u.SizeId == FacilityPitches.TeamSize && u.IsEnabled == true).Select(x => x.SizeName).FirstOrDefault() : string.Empty;

                var SurfaceName = FacilityPitches != null ? context.Surfaces.Where(u => u.SurfaceId == FacilityPitches.SurfaceId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault() : string.Empty;

                var SportName = context.Sports.Where(u => u.SportId == booking.SportId).Select(x => x.Name).FirstOrDefault();

                playRequestResposeModel.BookingId = booking.BookingId;
                playRequestResposeModel.UserImage = user.ImageUrl;
                playRequestResposeModel.FacilityId = booking.FacilityId;
                playRequestResposeModel.UserName = user.FirstName + " " + user.LastName;
                playRequestResposeModel.UserImage = user.ImageUrl;
                playRequestResposeModel.Location = locationName;
                playRequestResposeModel.SurfaceName = SurfaceName;
                playRequestResposeModel.TeamSizeName = TeamSizeName;
                playRequestResposeModel.SportId = booking.SportId;
                playRequestResposeModel.SportName = SportName;
                playRequestResposeModel.PitchStart = booking.PitchStart;
                playRequestResposeModel.PitchEnd = booking.PitchEnd;
                playRequestResposeModel.FacilityImage = facility != null ? facility.ImageUrl : string.Empty;
                playRequestResposeModel.FacilityName = facility != null ? facility.Name : string.Empty;
                playRequestResposeModel.UserId = user.UserId;
                playRequestResposeModel.PitchName = FacilityPitches != null ? FacilityPitches.Name : booking.Name;
                playRequestResposeModel.PriceIncludingVat = booking.PlayerCount * booking.PricePerUserVat.GetValueOrDefault();
                playRequestResposeModel.Date = booking.Date;
                playRequestResposeModel.SideKickCommission = booking.Commission.GetValueOrDefault();

                if (playRequest.Status == EGamePlayerStatus.Cancelled)
                {
                    request.PlayerStatus = EGamePlayerStatus.Cancelled;
                    context.FacilityPlayers.Update(request);
                    await context.SaveChangesAsync();

                    UserPitchBooking pitchBooking = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId).FirstOrDefault();
                    // check user is captain or not Cancellation (From User) Confirmation > 24 Hours   Cancellation (From User) Confirmation > 24 Hours

                    FacilityPitch facilityPitch = context.FacilityPitches.Where(x => x.FacilityPitchId == pitchBooking.FacilityPitchId).FirstOrDefault();
                    facility = context.Facilities.Where(x => x.FacilityId == pitchBooking.FacilityId).FirstOrDefault();
                    Sport sportsDetails = context.Sports.Where(x => x.SportId == pitchBooking.SportId).FirstOrDefault();
                    User Playeruser = context.Users.FirstOrDefault(x => x.UserId == userId);
                    Location location = FacilityPitches != null ? context.Locations.FirstOrDefault(x => x.LocationId == facilityPitch.LocationId) : null;

                    string LocationName = string.Empty;

                    if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                    {
                        LocationName = location.Name;
                    }

                    var playCommission = context.ComissionPlays.Where(c => c.SportId == pitchBooking.SportId);
                    decimal commisionRate = 0;
                    if (playCommission != null && playCommission.Count() > 0)
                        commisionRate = playCommission.FirstOrDefault().ComissionPerPlayer;

                    BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                    {
                        BookingId = pitchBooking.BookingId,
                        FacilityName = facility != null ? facility.Name : string.Empty,
                        UserName = Playeruser.FirstName,
                        Sport = sportsDetails.Name,
                        BookingDate = pitchBooking.PitchStart,
                        BookingTime = pitchBooking.PitchStart.ToShortTimeString() + "-" + pitchBooking.PitchEnd.ToShortTimeString(),
                        Location = LocationName,
                        PricePitch = facilityPitch != null ? facilityPitch.FixedPrice : 0,
                        ServiceFees = 0,
                        TotalAmount = (decimal)((pitchBooking.PricePerUser + pitchBooking.PricePerUser * 5 / 100 + commisionRate) * pitchBooking.PlayerCount),
                        PricePerPlayer = pitchBooking.PricePerUser.HasValue ? pitchBooking.PricePerUser.Value : 0,
                        EmailTo = new List<string>() { Playeruser.Email },
                        FacilityId = facility != null ? facility.FacilityId : Guid.Empty,
                        PitchName = facilityPitch != null ? facilityPitch.Name : booking.Name,
                        NotificationType = (int)ENotificationType.PitchBooking,
                        UserId = Playeruser.UserId
                    };

                    if (pitchBooking.PitchStart.Subtract(DateTime.Now).TotalHours > 24)
                    {
                        EmailTemplatesHelper.PitchBookingCancellationFromUserMorethan24HoursToPlayer(APIConfig, LogManager, commonTemplate);
                        await iUserNotificationHandler.PitchBookingCancellationFromPlayerMorethan24HoursToPlayer(commonTemplate);
                        await iUserNotificationHandler.PitchBookingCancellationFromPlayerMorethan24HoursToFacility(commonTemplate);

                        // send notification to captain as player cancel request
                        var CaptainDetails = context.FacilityPlayers.Where(g => g.BookingId == playRequest.BookingId && g.IsCaptain == true).FirstOrDefault();
                        if (CaptainDetails != null)
                        {
                            User Captainuser = context.Users.FirstOrDefault(x => x.UserId == CaptainDetails.UserId);
                            if (Captainuser != null && !string.IsNullOrWhiteSpace(Captainuser.UserId.ToString()))
                            {
                                commonTemplate.EmailTo = new List<string>() { Captainuser.Email };
                                commonTemplate.UserName = Captainuser.FirstName;
                                commonTemplate.PlayerName = Playeruser.FirstName;
                                commonTemplate.UserId = Captainuser.UserId;
                                await iUserNotificationHandler.PitchBookingCancellationFromPlayerMorethan24HoursToCaptain(commonTemplate);

                            }
                        }
                    }
                    else
                    {
                        EmailTemplatesHelper.PitchBookingCancellationFromUserLessthan24HoursToPlayer(APIConfig, LogManager, commonTemplate);
                        await iUserNotificationHandler.PitchBookingCancellationFromPlayerLessthan24HoursToPlayer(commonTemplate);

                        // send notification to captain as player cancel request
                        var CaptainDetails = context.FacilityPlayers.Where(g => g.BookingId == playRequest.BookingId && g.IsCaptain == true).FirstOrDefault();
                        if (CaptainDetails != null)
                        {
                            User Captainuser = context.Users.FirstOrDefault(x => x.UserId == CaptainDetails.UserId);
                            if (Captainuser != null && !string.IsNullOrWhiteSpace(Captainuser.UserId.ToString()))
                            {
                                commonTemplate.EmailTo = new List<string>() { Captainuser.Email };
                                commonTemplate.UserName = Captainuser.FirstName;
                                commonTemplate.PlayerName = Playeruser.FirstName;
                                commonTemplate.UserId = Captainuser.UserId;
                                await iUserNotificationHandler.PitchBookingCancellationFromPlayerLessthan24HoursToCaptain(commonTemplate);
                            }

                        }
                    }

                    bool Cancel24HoursWithin = false;
                    var FacilityPlayerDetails = context.FacilityPlayers.Where(g => g.BookingId == playRequest.BookingId && g.UserId == playRequest.UserId).FirstOrDefault();
                    if (pitchBooking.PitchStart.Subtract(DateTime.Now).TotalHours < 24)
                    {
                        Cancel24HoursWithin = true;
                    }
                    var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                    if (checkPayment.Payload.Order != null)
                    {
                        telRService.CancelPayment(FacilityPlayerDetails.DepositeAuthCode, user.TransactionNo, 1);
                        if (Cancel24HoursWithin)
                        {
                            var paymentStatus = telRService.CapturePayment(FacilityPlayerDetails.AuthCode, user.TransactionNo, FacilityPlayerDetails.AuthorizedAmount);
                            if (paymentStatus.Payload.IsSuccess)
                            {
                            }
                            else
                            {
                                //To-Do //send payment failed notification
                            }
                        }
                        else
                        {
                            var paymentStatus = telRService.CancelPayment(FacilityPlayerDetails.AuthCode, user.TransactionNo, FacilityPlayerDetails.AuthorizedAmount);
                            if (paymentStatus.Payload.IsSuccess)
                            {
                            }
                            else
                            {
                                //To-Do //send payment failed notification
                            }
                        }
                    }


                    return new APIResponse
                    {
                        Message = "Play Request cancelled",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = playRequestResposeModel
                    };

                }
                else if (playRequest.Status == EGamePlayerStatus.CanInvite)
                {
                    request.PlayerStatus = EGamePlayerStatus.CanInvite;
                    context.FacilityPlayers.Update(request);
                    await context.SaveChangesAsync();

                    return new APIResponse
                    {
                        Message = "Request updated",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = playRequestResposeModel
                    };

                }
                else if (playRequest.Status == EGamePlayerStatus.Pending)
                {
                    request.PlayerStatus = EGamePlayerStatus.Pending;
                    context.FacilityPlayers.Update(request);
                    await context.SaveChangesAsync();

                    return new APIResponse
                    {
                        Message = "Request updated",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = playRequestResposeModel
                    };

                }
                else if (playRequest.Status == EGamePlayerStatus.Approved)
                {
                    request.PlayerStatus = EGamePlayerStatus.Approved;
                    context.FacilityPlayers.Update(request);
                    await context.SaveChangesAsync();

                    // check for user is captain or not
                    var currentCaptainuser = context.FacilityPlayers.Where(g => g.BookingId == playRequest.BookingId && g.UserId == userId && g.IsCaptain == true).FirstOrDefault();
                    if (currentCaptainuser != null && !string.IsNullOrWhiteSpace(currentCaptainuser.BookingId.ToString()))
                    {
                        // add user notification and push notification code here for accept captian to player
                        var captain = context.Users.Where(x => x.UserId == userId).FirstOrDefault();
                        Facility sportfacility = context.Facilities.Where(x => x.FacilityId == request.FacilityId).FirstOrDefault();
                        booking = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId).FirstOrDefault();
                        BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                        {
                            UserId = playRequest.UserId,
                            CaptainName = captain.FirstName + " " + captain.LastName,
                            FacilityName = sportfacility != null ? sportfacility.Name : string.Empty,
                            BookingDate = booking.PitchStart,
                            BookingTime = booking.PitchStart.ToShortTimeString() + " - " + booking.PitchEnd.ToShortTimeString(),
                            BookingId = playRequest.BookingId,
                            NotificationType = (int)ENotificationType.PitchBooking,
                            FacilityId = request.FacilityId
                        };
                        var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(playRequest.UserId);
                        if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                        {
                            // send notification
                            List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                            await pushNotificationTemplateRepository.CaptainAcceptsTheRequest(APIConfig, LogManager, DeviceFCMTokens, commonTemplate);
                        }
                        await iUserNotificationHandler.CaptainAcceptsTheRequestToPlayer(commonTemplate);
                    }
                    return new APIResponse
                    {
                        Message = "Play Request approved",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = playRequestResposeModel
                    };
                }
            }
            return new APIResponse
            {
                Message = "BadRequest",
                Status = "Error!",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            };

        }

        public async Task<IEnumerable<UserPitchBooking>> GetPitchBookings(Guid facilityId)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookings --");
            LogManager.LogDebugObject(facilityId);

            return await context.UserPitchBookings.Where(u => u.FacilityId == facilityId && u.IsEnabled == true).ToListAsync();
        }

        public async Task<IEnumerable<PlayRequestViewModel>> GetPlayRequestByBooking(Guid bookingId)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetPlayRequestByBooking --");
            LogManager.LogDebugObject(bookingId);

            return await (from x in context.FacilityPlayers
                          join y in context.Users
                           on x.UserId equals y.UserId
                          where x.BookingId == bookingId && x.PlayerStatus == EGamePlayerStatus.Pending
                          select new PlayRequestViewModel()
                          {
                              UserId = x.UserId,
                              UserImage = y.ImageUrl,
                              SportId = x.SportId,
                              BookingId = bookingId,
                              RequestId = x.FacilityPlayerId,
                              UserName = y.FirstName + " " + y.LastName,
                              PlayerStatus = x.PlayerStatus
                          }).ToListAsync();
        }

        public async Task<InviteRequestResponseModel> GetInviteRequestByBooking(Guid bookingId, string search)
        {
            InviteRequestResponseModel inviteRequestResponseModel = new InviteRequestResponseModel();
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            var userFriends = await userHandler.GetFriends(null);
            var userList = (List<UserFriendViewModel>)userFriends.Payload;
            var userSidList = new List<Guid>();
            if (userList != null && userList.Count() > 0)
            {
                userSidList = userList.Select(u => u.UserId).ToList();
            }

            var gameplayers = await GetGamePlayers(bookingId);
            var userGamePlayerList = new List<Guid>();
            if (gameplayers != null && gameplayers.Count() > 0)
            {
                userGamePlayerList = gameplayers.Select(u => u.UserId).ToList();
            }

            inviteRequestResponseModel.PlayRequests = await (from x in context.Users
                                                             where userSidList.Contains(x.UserId) && !userGamePlayerList.Contains(x.UserId)
                                                             && (x.FirstName.Contains(search ?? x.FirstName) || x.LastName.Contains(search ?? x.LastName))
                                                             select new PlayRequestViewModel()
                                                             {
                                                                 UserId = x.UserId,
                                                                 UserImage = x.ImageUrl,
                                                                 BookingId = bookingId,
                                                                 UserName = x.FirstName + " " + x.LastName,
                                                             }).ToListAsync();
            List<UserTeamResponseModel> userTeams = new List<UserTeamResponseModel>();
            var UserTeams = await context.UserTeams.Where(x => x.Fk_UserId.UserId == currentLogin && x.TeamName.Contains(search ?? x.TeamName)).ToListAsync();

            foreach (var UserTeamItem in UserTeams)
            {
                var UserTeamMembers = await (from x in context.UserTeamMembers
                                             join u in context.Users on x.MemberUserId equals u.Id
                                             where x.UserTeamId == UserTeamItem.Id
                                             select new UserTeamMemberResponseModel()
                                             {
                                                 UserId = u.UserId,
                                                 UserName = u.FirstName + " " + u.LastName
                                             }).ToListAsync();

                userTeams.Add(new UserTeamResponseModel()
                {
                    TeamName = UserTeamItem.TeamName,
                    UserTeamId = UserTeamItem.UserTeamId,
                    UserTeamMembers = UserTeamMembers,
                });
            }

            inviteRequestResponseModel.UserTeams = userTeams;
            return inviteRequestResponseModel;

        }


        public async Task<UserPitchBooking> GetPitchBooking(Guid bookingId)
        {
            return await context.UserPitchBookings
                .Where(x => x.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserPitchBooking>> GetAllPitchBookings5MinsPriorToStartDate()
        {
            DateTime currentDate = Helper.GetDateTime().AddDays(-1);
            DateTime nextDate = Helper.GetDateTime().AddSeconds(600);

            return from b in context.UserPitchBookings
                   where b.PitchStart <= nextDate && b.PitchStart >= currentDate
                   select b;
        }


        public async Task<IEnumerable<UserPitchBooking>> GetAllPitchBookingPrior48hrsToStartDate()
        {
            DateTime currentDate = Helper.GetDateTime().AddDays(-1);
            DateTime nextDate = Helper.GetDateTime().AddHours(48);

            return from b in context.UserPitchBookings
                              where b.PitchStart <= nextDate && b.PitchStart >= currentDate
                              select b;
        }

        public async Task<IEnumerable<UserPitchBookingResponseModel>> GetPitchBookingBySportId(IEnumerable<PlayFilterViewModel> filters, Guid sportId, Guid userId, string facilityName)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookingBySportId --");
            LogManager.LogDebugObject(sportId);

            List<UserPitchBooking> bookingList = context.UserPitchBookings.Where(u => u.SportId == sportId && u.IsEnabled == true).ToList();
            var bookingModel = new List<UserPitchBookingResponseModel>();
            if (bookingList.Any())
            {

                var FacilityIdList = bookingList.Select(s => s.FacilityId).ToList();
                IEnumerable<Facility> facilities = context.Facilities.Where(x => x.IsEnabled == true && FacilityIdList.Contains(x.FacilityId) && x.Name.Contains(facilityName ?? x.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                // add id here
                var FacilityIds = facilities.Select(s => s.FacilityId).ToList();
                var FacilityPitchIdList = bookingList.Select(s => s.FacilityPitchId).ToList();
                IEnumerable<FacilityPitch> facilitiesPitch = context.FacilityPitches.Where(x => x.IsEnabled == true && x.FacilityPitchId.HasValue && FacilityPitchIdList.Contains(x.FacilityPitchId.Value) && x.FacilityId.HasValue && FacilityIds.Contains(x.FacilityId.Value)).ToList();
                // get facility
                // get FacilityPitch

                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        if (filter.FilterType == FilterType.Area)
                        {
                            facilitiesPitch = facilitiesPitch.Where(x => x.AreaId == Guid.Parse(filter.FilterValue));
                        }
                        if (filter.FilterType == FilterType.Surface)
                        {
                            facilitiesPitch = facilitiesPitch.Where(x => x.SurfaceId == Guid.Parse(filter.FilterValue));
                        }
                        if (filter.FilterType == FilterType.TeamSize)
                        {
                            facilitiesPitch = facilitiesPitch.Where(x => x.TeamSize == Guid.Parse(filter.FilterValue));
                        }
                        if (filter.FilterType == FilterType.Location)
                        {
                            facilitiesPitch = facilitiesPitch.Where(x => x.LocationId == Guid.Parse(filter.FilterValue));
                        }
                        if (filter.FilterType == FilterType.Price)
                        {
                            var splPrice = filter.FilterValue.Split("-");
                            var firstValue = Convert.ToInt32(splPrice[0]);
                            var secondValue = Convert.ToInt32(splPrice[1]);

                            facilitiesPitch = facilitiesPitch.Where(x => x.FixedPrice <= firstValue && x.FixedPrice >= secondValue);
                        }
                    }

                }

                if (facilitiesPitch != null)
                {
                    var FilterFacilityPitchIds = facilitiesPitch.Select(s => s.FacilityPitchId);
                    var FilterFacilityIds = facilitiesPitch.Select(s => s.FacilityId);

                    // filter booking list here...
                    if (filters != null && filters.Any())
                    {
                        bookingList = bookingList.Where(s => FilterFacilityPitchIds.Contains(s.FacilityPitchId) && FilterFacilityIds.Contains(s.FacilityId)).ToList();
                    }
                    else
                    {
                        bookingList = bookingList.Where(s => (FilterFacilityPitchIds.Contains(s.FacilityPitchId) || s.FacilityPitchId == default(Guid)) && (FilterFacilityIds.Contains(s.FacilityId) || s.FacilityId == default(Guid))).ToList();
                    }

                }

                foreach (var booking in bookingList)
                {
                    Facility facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId && u.IsEnabled == true).FirstOrDefault();
                    //if (facility != null && !string.IsNullOrWhiteSpace(facility.FacilityId.ToString()))
                    //{
                    FacilityPitch facilityPitch = context.FacilityPitches.Where(u => u.FacilityId == booking.FacilityId && u.FacilityPitchId == booking.FacilityPitchId && u.IsEnabled == true).FirstOrDefault();
                    //if (facilityPitch != null && facilityPitch.FacilityPitchId.HasValue)
                    //{
                    var surfaceName = facilityPitch != null ? context.Surfaces.Where(u => u.SurfaceId == facilityPitch.SurfaceId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault() : "";
                    var TeamSizeName = facilityPitch != null ? context.Sizes.Where(u => u.SizeId == facilityPitch.TeamSize && u.IsEnabled == true).Select(x => x.SizeName).FirstOrDefault() : "";
                    var SportName = context.Sports.Where(u => u.SportId == booking.SportId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault();
                    bool isCaptain = false, isParticipate = false, IsPaymentPending = false, isRequestsent = false, IsCanInvite = false, IsWaitingRequest = false;
                    var gamePlayers = context.FacilityPlayers.Where(u => u.BookingId == booking.BookingId).ToList();
                    bool IsWaitinglist = context.FacilityPlayers.Where(g => g.BookingId == booking.BookingId && (g.PlayerStatus == EGamePlayerStatus.Approved || g.PlayerStatus == EGamePlayerStatus.CanInvite)).Count() == booking.PlayerCount;
                    var currentuser = context.FacilityPlayers.Where(g => g.BookingId == booking.BookingId && g.UserId == userId).FirstOrDefault();
                    if (currentuser != null && !string.IsNullOrWhiteSpace(currentuser.UserId.ToString()))
                    {
                        isCaptain = currentuser.IsCaptain;
                        isParticipate = currentuser.PlayerStatus == EGamePlayerStatus.Approved || currentuser.PlayerStatus == EGamePlayerStatus.CanInvite;
                        isRequestsent = currentuser.PlayerStatus == EGamePlayerStatus.Pending;
                        IsPaymentPending = !currentuser.IsPaid;
                        IsCanInvite = currentuser.PlayerStatus == EGamePlayerStatus.CanInvite;
                        IsWaitingRequest = currentuser.PlayerStatus == EGamePlayerStatus.Waiting;
                    }


                    var bookingDetail = new UserPitchBookingResponseModel
                    {

                        BookingId = booking.BookingId,
                        FacilityId = booking.FacilityId,
                        Area = facility != null ? facility.Area : null,
                        Name = facility != null ? facility.Name : booking.Name,
                        Street = facility != null ? facility.Street : "",
                        City = facility != null ? facility.City : "",
                        IsCanInvite = IsCanInvite,
                        Location = facility != null ? facility.Street + " " + facility.Area + " " + facility.City + " " + facility.Country : "",
                        Description = booking.Description,
                        IsWaitingRequest = IsWaitingRequest,
                        //CreatedDate = facility.CreatedDate,
                        //IsEnabled = facility.IsEnabled,
                        //FacilityImage = facility.ImageUrl,
                        //Commission = booking.Commission + "%",

                        PitchLocationId = facilityPitch != null ? facilityPitch.LocationId : Guid.Empty,
                        PitchLocation = facilityPitch != null ? context.Locations.Where(u => u.LocationId == facilityPitch.LocationId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault() : string.Empty,
                        FacilityPitchId = booking.FacilityPitchId,
                        //FacilityPitchName = facilityPitch.Name,
                        //IsFacilityTime = facilityPitch.IsFacilityTime,
                        //AreaId = facility.Area.AreaId,
                        SurfaceId = facilityPitch != null ? facilityPitch.SurfaceId : Guid.Empty,
                        SurfaceName = surfaceName,
                        SizeId = facilityPitch != null ? facilityPitch.TeamSize : Guid.Empty,
                        TeamSize = TeamSizeName,
                        UserId = booking.UserId,
                        PlayerCount = booking.PlayerCount,
                        IsPrivate = booking.IsPrivate,
                        IsPaid = booking.IsPaid,
                        IsCancelled = booking.IsCancelled,
                        PricePerUser = booking.PricePerUser,
                        PitchStart = booking.PitchStart,
                        PitchEnd = booking.PitchEnd,
                        Date = Helper.DisplayDateTime(booking.Date),
                        SportId = booking.SportId,
                        SportName = SportName,
                        MaxPlayers = facilityPitch != null ? facilityPitch.MaxPlayers : booking.PlayerCount,
                        IsCaptain = isCaptain,
                        IsParticipate = isParticipate,
                        FacilityImage = facility != null ? facility.ImageUrl : "",
                        IsLocked = booking.IsLocked.GetValueOrDefault(),
                        WaitingPlayerCount = context.FacilityPlayers.Count(a => a.BookingId == booking.BookingId && a.PlayerStatus == EGamePlayerStatus.Waiting),
                        ApprovedPlayerCount = context.FacilityPlayers.Count(a => a.BookingId == booking.BookingId && a.PlayerStatus == EGamePlayerStatus.Approved),
                        IsPaymentPending = IsPaymentPending,
                        IsRequestsent = isRequestsent,
                        IsWaitinglist = IsWaitinglist,

                        Latitude = facility != null ? facility.Latitude : 0,
                        Longitude = facility != null ? facility.Longitude : 0,
                        RegisteredPlayers = (from gameplayer in context.FacilityPlayers
                                             join user in context.Users
                                             on gameplayer.UserId equals user.UserId
                                             where gameplayer.BookingId == booking.BookingId && (gameplayer.PlayerStatus == EGamePlayerStatus.Approved || gameplayer.PlayerStatus == EGamePlayerStatus.CanInvite)
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
                        WaitingPlayers = (from gameplayer in context.FacilityPlayers
                                          join user in context.Users
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
                    bookingModel.Add(bookingDetail);
                    //}
                    //}
                }
            }
            return bookingModel;
        }
        public async Task<APIResponse> PitchBooking(UserPitchBookingModel pitchBooking, Guid userID, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            LogManager.LogInfo("-- Run::PlayRepository::SubmitPitchBooking --");
            LogManager.LogDebugObject(pitchBooking.UserId);

            UserPitchBooking booking = context.UserPitchBookings.Where(x => x.BookingId == pitchBooking.BookingId).FirstOrDefault();
            if (booking != null)
            {
                if (pitchBooking.IsPrivate == false)
                {
                    booking.IsPrivate = false;
                    context.UserPitchBookings.Update(booking);
                    await context.SaveChangesAsync();
                    return new APIResponse
                    {
                        Message = "Booking successfully changed",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else if (pitchBooking.IsPrivate == true)
                {
                    booking.IsPrivate = true;
                    context.UserPitchBookings.Update(booking);
                    await context.SaveChangesAsync();
                    return new APIResponse
                    {
                        Message = "Booking successfully changed",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else if (pitchBooking.IsCancelled == true && pitchBooking.IsPaid == false)
                {
                    // cancel by captain Cancellation (From User) Confirmation > 24 Hours
                    booking.IsEnabled = false;
                    context.UserPitchBookings.Update(booking);
                    await context.SaveChangesAsync();

                    //TODO: cancel by captain Cancellation (From User) Confirmation > 24 Hours

                    Facility facility = context.Facilities.Where(x => x.FacilityId == pitchBooking.FacilityId).FirstOrDefault();
                    FacilityPitch facilityPitch = context.FacilityPitches.Where(x => x.FacilityPitchId == pitchBooking.FacilityPitchId).FirstOrDefault();
                    Sport sportsDetails = context.Sports.Where(x => x.SportId == pitchBooking.SportId).FirstOrDefault();
                    User captainuser = context.Users.FirstOrDefault(x => x.UserId == userID);
                    Location location = context.Locations.FirstOrDefault(x => x.LocationId == facilityPitch.LocationId);
                    string LocationName = string.Empty;

                    if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                    {
                        LocationName = location.Name;
                    }

                    var playCommission = context.ComissionPlays.Where(c => c.SportId == pitchBooking.SportId);
                    decimal commisionRate = 0;
                    if (playCommission != null && playCommission.Count() > 0)
                        commisionRate = playCommission.FirstOrDefault().ComissionPerPlayer;

                    BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                    {
                        BookingId = pitchBooking.BookingId.Value,
                        FacilityName = facility.Name,
                        UserName = captainuser.FirstName,
                        Sport = sportsDetails.Name,
                        BookingDate = booking.PitchStart,
                        BookingTime = booking.PitchStart.ToShortTimeString() + "-" + booking.PitchEnd.ToShortTimeString(),
                        Location = LocationName,
                        PricePitch = facilityPitch.FixedPrice,
                        ServiceFees = 0,
                        TotalAmount = (decimal)((pitchBooking.PricePerUser + pitchBooking.PricePerUser * 5 / 100 + commisionRate) * pitchBooking.PlayerCount),
                        PricePerPlayer = booking.PricePerUser.HasValue ? booking.PricePerUser.Value : 0,
                        EmailTo = new List<string>() { captainuser.Email },
                        CaptainName = captainuser.FirstName,
                        FacilityId = facility.FacilityId,
                        PitchName = facilityPitch.Name,
                        NotificationType = (int)ENotificationType.PitchBooking
                    };

                    IEnumerable<FacilityPlayer> players = context.FacilityPlayers.Where(x => x.BookingId == pitchBooking.BookingId && x.IsCaptain == false);

                    // check cancel done morethen 24 hours
                    if (booking.PitchStart.Subtract(DateTime.Now).TotalHours > 24)
                    {
                        EmailTemplatesHelper.PitchBookingCancellationFromUserhMorethan24HoursToCaptain(APIConfig, LogManager, commonTemplate);
                        await iUserNotificationHandler.PitchBookingCancellationFromCaptainMorethan24HoursToCaptain(commonTemplate);

                        // TODO: need to send cancel confirm to all player..
                        if (players.Any())
                        {
                            foreach (var player in players)
                            {
                                commonTemplate.UserId = player.UserId;
                                await iUserNotificationHandler.PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers(commonTemplate);
                            }
                        }
                        //
                        commonTemplate.UserId = captainuser.UserId;
                        await iUserNotificationHandler.PitchBookingCancellationFromCaptainMorethan24HoursToFacility(commonTemplate);
                    }
                    else
                    {
                        EmailTemplatesHelper.PitchBookingCancellationFromUserLessthan24HoursToCaptain(APIConfig, LogManager, commonTemplate);
                        await iUserNotificationHandler.PitchBookingCancellationFromCaptainLessthan24HoursToCaptain(commonTemplate);
                        // TODO: need to send cancel confirm to all player..
                        if (players.Any())
                        {
                            foreach (var player in players)
                            {
                                commonTemplate.UserId = player.UserId;
                                await iUserNotificationHandler.PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers(commonTemplate);
                            }
                        }
                    }


                    return new APIResponse
                    {
                        Message = "Booking successfully cancelled",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    if (pitchBooking.IsCaptain == true)
                    {
                        booking.FacilityId = pitchBooking.FacilityId;
                        booking.FacilityPitchId = pitchBooking.FacilityPitchId;
                        booking.UserId = pitchBooking.UserId;
                        booking.Name = pitchBooking.Name;
                        booking.Description = pitchBooking.Description;
                        booking.PlayerCount = pitchBooking.PlayerCount;
                        booking.IsPrivate = pitchBooking.IsPrivate;
                        booking.IsPaid = pitchBooking.IsPaid;
                        booking.IsCancelled = pitchBooking.IsCancelled;
                        booking.AreaId = pitchBooking.AreaId;
                        booking.City = pitchBooking.City;
                        booking.IsCaptain = pitchBooking.IsCaptain.GetValueOrDefault();
                        booking.CreatedBy = userID;
                        //booking.LocationId = pitchBooking.LocationId;
                        booking.PricePerUser = pitchBooking.PricePerUser;
                        booking.Commission = 0; //to do: correct commision value
                        booking.PitchStart = pitchBooking.PitchStart;
                        booking.PitchEnd = pitchBooking.PitchEnd;
                        booking.SurfaceId = pitchBooking.SurfaceId;
                        booking.Date = pitchBooking.Date;
                        booking.SportId = pitchBooking.SportId;
                        context.UserPitchBookings.Update(booking);
                        await context.SaveChangesAsync();

                        if (pitchBooking.RegisteredPlayers.Any())
                        {
                            await DeleteGamePlayers(booking.BookingId, booking.UserId);
                            //await AddEditGamePlayers(pitchBooking.RegisteredPlayers, booking.BookingId, pitchBooking.UserId);
                        }
                        return new APIResponse
                        {
                            Message = "Booking successfully updated",
                            Status = "Success!",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Payload = booking.BookingId
                        };
                    }
                    else
                    {
                        return new APIResponse
                        {
                            Message = "Only Captains can update booking.",
                            Status = "Error!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
            }
            else
            {
                if (pitchBooking.FacilityId == Guid.Empty || pitchBooking.FacilityId == null)
                {
                    FacilitySport sports = context.FacilitySports.Where(x => x.SportId == pitchBooking.SportId).FirstOrDefault();
                    if (sports == null)
                    {
                        return new APIResponse
                        {
                            Message = "Sport not available on the Facility",
                            Status = "Error!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }

                    User user = context.Users.Where(x => x.UserId == pitchBooking.UserId).FirstOrDefault();
                    if (user == null)
                    {
                        return new APIResponse
                        {
                            Message = "User not found!",
                            Status = "Error!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }

                    var bookingID = Guid.NewGuid();
                    booking = new UserPitchBooking
                    {
                        BookingId = bookingID,
                        UserId = pitchBooking.UserId,
                        Name = pitchBooking.Name,
                        PlayerCount = pitchBooking.PlayerCount,
                        AreaId = pitchBooking.AreaId,
                        Location = pitchBooking.Location,
                        IsPrivate = true,
                        IsPaid = false,
                        CreatedBy = userID,
                        CreatedDate = Helper.GetDateTime(),
                        DateEnabled = Helper.GetDateTime(),
                        IsEnabled = true,
                        PricePerUser = 0,
                        PricePerUserVat = 0,
                        Commission = 0,
                        PitchStart = pitchBooking.PitchStart,
                        PitchEnd = pitchBooking.PitchEnd,
                        Date = pitchBooking.Date,
                        SportId = pitchBooking.SportId
                    };

                    context.UserPitchBookings.Add(booking);
                    await context.SaveChangesAsync();

                    var gameplayers = new List<FacilityPlayerModel>();
                    var gamePlayer = new FacilityPlayerModel();
                    gamePlayer.BookingId = bookingID;
                    gamePlayer.IsCaptain = true;
                    gamePlayer.SportId = pitchBooking.SportId;
                    gamePlayer.IsCanInvite = true;
                    gamePlayer.ContactNumber = user.MobileNumber;
                    gamePlayer.Email = user.Email;
                    gamePlayer.ProfileImgUrl = user.ImageUrl;
                    gamePlayer.FirstName = user.FirstName;
                    gamePlayer.LastName = user.LastName;
                    gamePlayer.Name = user.FirstName + " " + user.LastName;
                    gamePlayer.IsPaid = false;
                    gamePlayer.TotalAmount = 0;
                    gamePlayer.PlayerStatus = EGamePlayerStatus.Approved;
                    gamePlayer.UserId = userID;
                    gameplayers.Add(gamePlayer);
                    await AddEditGamePlayers(gameplayers, bookingID, userID);


                    return new APIResponse
                    {
                        Message = "Booking Submitted.",
                        Status = "Success!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = booking.BookingId
                    };

                }
                else if (pitchBooking.FacilityPitchId != null)
                {
                    Facility facility = context.Facilities.Where(x => x.FacilityId == pitchBooking.FacilityId).FirstOrDefault();
                    if (facility != null)
                    {
                        FacilityPitch facilityPitch = context.FacilityPitches.Where(x => x.FacilityPitchId == pitchBooking.FacilityPitchId).FirstOrDefault();
                        if (facilityPitch != null)
                        {
                            User user = context.Users.Where(x => x.UserId == pitchBooking.UserId).FirstOrDefault();
                            if (user == null)
                            {
                                return new APIResponse
                                {
                                    Message = "User not found!",
                                    Status = "Error!",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            FacilitySport sports = context.FacilitySports.Where(x => x.FacilityId == pitchBooking.FacilityId && x.SportId == pitchBooking.SportId).FirstOrDefault();
                            if (sports == null)
                            {
                                return new APIResponse
                                {
                                    Message = "Sport not available on the Facility",
                                    Status = "Error!",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            Sport sportsDetails = context.Sports.Where(x => x.SportId == pitchBooking.SportId).FirstOrDefault();
                            if (sportsDetails == null)
                            {
                                return new APIResponse
                                {
                                    Message = "Sport not available ",
                                    Status = "Error!",
                                    StatusCode = System.Net.HttpStatusCode.BadRequest
                                };
                            }

                            //check if a booking of same time exists

                            var playCommission = context.ComissionPlays.Where(c => c.SportId == pitchBooking.SportId);
                            decimal commisionRate = 0;
                            if (playCommission != null && playCommission.Count() > 0)
                                commisionRate = playCommission.FirstOrDefault().ComissionPerPlayer;

                            var bookingID = Guid.NewGuid();
                            booking = new UserPitchBooking
                            {
                                BookingId = bookingID,
                                FacilityId = pitchBooking.FacilityId,
                                FacilityPitchId = pitchBooking.FacilityPitchId,
                                UserId = pitchBooking.UserId,
                                Name = pitchBooking.Name,
                                Description = pitchBooking.Description,
                                PlayerCount = pitchBooking.PlayerCount,
                                IsPrivate = pitchBooking.IsPrivate,
                                IsPaid = pitchBooking.IsPaid,
                                IsCancelled = pitchBooking.IsCancelled,
                                AreaId = pitchBooking.AreaId,
                                City = pitchBooking.City,
                                FacilityPitchTimingId = pitchBooking.FacilityPitchTimingId,
                                CreatedBy = userID,
                                CreatedDate = Helper.GetDateTime(),
                                DateEnabled = Helper.GetDateTime(),
                                IsEnabled = true,
                                //LocationId = pitchBooking.LocationId,
                                PricePerUser = pitchBooking.PricePerUser,
                                PricePerUserVat = pitchBooking.PricePerUser * 5 / 100,
                                Commission = commisionRate,
                                PitchStart = pitchBooking.PitchStart,
                                PitchEnd = pitchBooking.PitchEnd,
                                SurfaceId = pitchBooking.SurfaceId,
                                Date = pitchBooking.Date,
                                SportId = pitchBooking.SportId,
                                IsCaptain = true
                            };

                            context.UserPitchBookings.Add(booking);
                            await context.SaveChangesAsync();

                            var gameplayers = new List<FacilityPlayerModel>();
                            var gamePlayer = new FacilityPlayerModel();
                            gamePlayer.BookingId = bookingID;
                            gamePlayer.IsCaptain = true;
                            gamePlayer.SportId = pitchBooking.SportId;
                            gamePlayer.IsCanInvite = true;
                            gamePlayer.FacilityId = booking.FacilityId;
                            gamePlayer.ContactNumber = user.MobileNumber;
                            gamePlayer.Email = user.Email;
                            gamePlayer.ProfileImgUrl = user.ImageUrl;
                            gamePlayer.AreaId = pitchBooking.AreaId;
                            gamePlayer.FirstName = user.FirstName;
                            gamePlayer.LastName = user.LastName;
                            gamePlayer.Name = user.FirstName + " " + user.LastName;
                            gamePlayer.IsPaid = false;
                            gamePlayer.TotalAmount = (decimal)((pitchBooking.PricePerUser + pitchBooking.PricePerUser * 5 / 100 + commisionRate) * pitchBooking.PlayerCount);
                            gamePlayer.PlayerStatus = EGamePlayerStatus.Approved;
                            gamePlayer.UserId = userID;
                            gameplayers.Add(gamePlayer);
                            await AddEditGamePlayers(gameplayers, bookingID, userID);
                            // TODO:Pitch Booking Confirmation check for 0
                            int confirmationSent = 0;
                            User captainuser = context.Users.FirstOrDefault(x => x.UserId == userID);
                            Location location = context.Locations.FirstOrDefault(x => x.LocationId == facilityPitch.LocationId);
                            string LocationName = string.Empty;
                            if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                            {
                                LocationName = location.Name;
                            }

                            BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                            {
                                BookingId = bookingID,
                                FacilityName = facility.Name,
                                UserName = captainuser.FirstName,
                                Sport = sportsDetails.Name,
                                BookingDate = booking.PitchStart,
                                BookingTime = booking.PitchStart.ToShortTimeString() + "-" + booking.PitchEnd.ToShortTimeString(),
                                Location = LocationName,
                                PricePitch = pitchBooking.PricePerUser.HasValue ? pitchBooking.PricePerUser.Value * pitchBooking.PlayerCount : 0,
                                ServiceFees = pitchBooking.PricePerUser.HasValue ? (pitchBooking.PricePerUser.Value * 5 / 100) * pitchBooking.PlayerCount : 0,
                                TotalAmount = gamePlayer.TotalAmount,
                                PricePerPlayer = booking.PricePerUser.HasValue ? booking.PricePerUser.Value : 0,
                                EmailTo = new List<string>() { captainuser.Email },
                                BookingConfirmed = true
                            };

                            confirmationSent = EmailTemplatesHelper.PitchBookingConfirmationToCaptain(APIConfig, LogManager, commonTemplate);
                            await iUserNotificationHandler.PitchBookingConfirmationToCaptain(commonTemplate);

                            string emailStatusMessage = String.Empty;
                            switch (confirmationSent)
                            {
                                case 0:
                                    emailStatusMessage = "Booking Confirmation Successfully Sent to the Captain.";
                                    break;
                                case 1:
                                    emailStatusMessage = "Booking Confirmation not sent to the Captain.";
                                    break;
                                default: break;
                            }
                            return new APIResponse
                            {
                                Message = "Booking Submitted." + emailStatusMessage,
                                Status = "Success!",
                                StatusCode = System.Net.HttpStatusCode.OK,
                                Payload = booking.BookingId
                            };

                        }
                        else
                        {
                            return new APIResponse
                            {
                                Message = "Facility Pitch not found!",
                                Status = "Error!",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }
                else
                {
                    return new APIResponse
                    {
                        Message = "Facility not found!",
                        Status = "Error!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }

            return new APIResponse
            {
                Message = "Facility not found!",
                Status = "Error!",
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

        }

        public async Task<Filters> GetFilters()
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetAllFilters --");

            IEnumerable<Area> areas = context.Areas
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<Surface> surfaces = context.Surfaces
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<Location> locations = context.Locations
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<TeamSize> sizes = context.Sizes
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<Sport> sports = context.Sports
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<Gym> Gyms = context.Gyms
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<Language> Languages = context.Languages
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<Level> Levels = context.Levels
            .Where(x => x.IsEnabled == true)
            .ToList();

            IEnumerable<string> prices = new List<string>() { "50-100", "100-200", "200-300" };

            return new Filters
            {
                AreaFilters = areas,
                SurfaceFilters = surfaces,
                LocationFilters = locations,
                TeamSizeFilters = sizes,
                Prices = prices,
                SportFilters = sports,
                GymFilters = Gyms,
                LanguageFilters = Languages,
                LevelFilters = Levels
            };
        }

        public async Task<FacilityPlayer> GetGamePlayer(Guid bookingId, Guid userId)
        {
            return await context.FacilityPlayers
                .Where(x => x.UserId == userId
                         && x.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<FacilityPlayer>> GetGamePlayers(Guid bookingId)
        {
            return await context.FacilityPlayers
                .Where(x => x.BookingId == bookingId
                         && x.PlayerStatus == EGamePlayerStatus.Approved)
                .ToListAsync();
        }

        public async Task UpdateGamePlayer(FacilityPlayer gamePlayer)
        {
            if (gamePlayer != null)
            {
                context.FacilityPlayers.Update(gamePlayer);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddEditGamePlayers(IEnumerable<FacilityPlayerModel> gamePlayers, Guid bookingId, Guid userId)
        {
            User user = context.Users.Where(x => x.UserId == userId && x.IsEnabled == true).FirstOrDefault();
            if (user != null)
            {
                foreach (var player in gamePlayers)
                {
                    try
                    {
                        FacilityPlayer players = new()
                        {
                            FacilityPlayerId = Guid.NewGuid(),
                            BookingId = player.BookingId,
                            IsCanInvite = player.IsCanInvite,
                            AreaId = player.AreaId,
                            ContactNumber = player.ContactNumber,
                            DateCreated = Helper.GetDateTime(),
                            Email = player.Email,
                            FacilityId = player.FacilityId,
                            FirstName = player.FirstName,
                            LastName = player.LastName,
                            Name = player.Name,
                            ProfileImgUrl = player.ProfileImgUrl,
                            UserId = userId,
                            IsCaptain = player.IsCaptain,
                            CreatedBy = userId,
                            CreatedDate = Helper.GetDateTime(),
                            DateEnabled = Helper.GetDateTime(),
                            IsEnabled = true,
                            IsEnabledBy = userId,
                            SportId = player.SportId,
                            TotalAmount = player.TotalAmount,
                            PlayerStatus = player.PlayerStatus,
                            WaitingCount = player.WaitingCount
                        };
                        context.FacilityPlayers.Add(players);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {


                    }

                }

            }
        }
        public async Task DeleteGamePlayers(Guid bookingId, Guid userId)
        {
            UserPitchBooking booking = context.UserPitchBookings.Where(x => x.BookingId == bookingId && x.IsEnabled == true).FirstOrDefault();
            if (booking != null)
            {

                IEnumerable<FacilityPlayer> players = context.FacilityPlayers.Where(x => x.BookingId == bookingId && x.UserId == userId);

                if (players.Any())
                {

                    context.FacilityPlayers.RemoveRange(players);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateGameplayerToPaid(Guid gameplayId)
        {
            var players = context.FacilityPlayers.Where(x => x.FacilityPlayerId == gameplayId);
            if (players != null)
            {
                var player = players.FirstOrDefault();
                player.DatePaid = Helper.GetDateTime();
                player.IsPaid = true;
                await UpdateGamePlayer(players.FirstOrDefault());
            }
        }

        public async Task<APIResponse> SentInviteRequest(Guid bookingId, List<string> userSids, Guid userID)
        {
            foreach (var itemusers in userSids)
            {
                var commonTemplate = new BookingNotificationCommonTemplate();
                UserPitchBooking booking = context.UserPitchBookings.Where(x => x.BookingId == bookingId && x.IsEnabled == true).FirstOrDefault();
                var captain = context.FacilityPlayers.Where(x => x.BookingId == bookingId && x.UserId == userID && (x.PlayerStatus == EGamePlayerStatus.CanInvite || x.IsCaptain == true));
                Sport sport = context.Sports.Where(x => x.SportId == booking.SportId).FirstOrDefault();
                Facility sportfacility = context.Facilities.Where(x => x.FacilityId == booking.FacilityId).FirstOrDefault();

                if (captain == null)
                {
                    return new APIResponse
                    {
                        Message = "User don't have rights to invite other users",
                        Status = "Error!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                var captainInfo = context.Users.Where(c => c.UserId == userID).FirstOrDefault();
                commonTemplate.CaptainName = captainInfo.FirstName + " " + captainInfo.LastName;
                commonTemplate.BookingDate = booking.Date;
                commonTemplate.BookingTime = booking.PitchStart.ToString("HH:mm");
                commonTemplate.Sport = sport.Name;
                commonTemplate.BookingId = bookingId;
                commonTemplate.BookingConfirmed = false;
                commonTemplate.UserId = Guid.Parse(itemusers);
                commonTemplate.FacilityName = sportfacility.Name;
                commonTemplate.NotificationType = (int)ENotificationType.PitchBooking;
                commonTemplate.FacilityId = booking.FacilityId;
                await this.iUserNotificationHandler.ShareEventToPlayer(commonTemplate);

                var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(Guid.Parse(itemusers));
                if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                {
                    // send notification
                    List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                    await pushNotificationTemplateRepository.InviteShareEvent(APIConfig, LogManager, DeviceFCMTokens, commonTemplate);
                }
            }


            return new APIResponse
            {
                Message = "Invite requests are submitted.",
                Status = "Success!",
                StatusCode = System.Net.HttpStatusCode.OK,
                Payload = null
            };
        }


        public async Task<IEnumerable<PlayBookingModel>> GetAllPitchBookings()
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetAllPitchBookings --");
            var playBookings = new List<PlayBookingModel>();

            IEnumerable<UserPitchBooking> bookings = await context.UserPitchBookings.Where(u => u.IsEnabled == true).ToListAsync();
            if (bookings.Any())
            {
                foreach (var booking in bookings)
                {
                    var bookinguser = context.Users.Where(u => u.UserId == booking.UserId).FirstOrDefault();
                    var bookingDetail = new PlayBookingModel
                    {
                        BookingId = booking.BookingId,
                        Name = booking.Name,
                        FacilityId = booking.FacilityId,
                        PitchStart = booking.PitchStart,
                        PitchEnd = booking.PitchEnd,
                        Date = Helper.DisplayDateTime(booking.Date),
                        PlayerCount = booking.PlayerCount,
                        PricePerPlayer = booking.PricePerUser.GetValueOrDefault(),
                        PriceIncludingVat = booking.PlayerCount * booking.PricePerUser.GetValueOrDefault(),
                        TotalPrice = (decimal)((booking.PlayerCount * booking.PricePerUser.GetValueOrDefault()) + booking.Commission ?? default),
                        Commission = booking.Commission != null ? booking.Commission.GetValueOrDefault() : 0,
                        Facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId)?.FirstOrDefault(),
                        FacilityPitch = context.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId)?.FirstOrDefault(),
                        CreatedDate = booking.CreatedDate.Value,
                        UserName = bookinguser.FirstName + " " + bookinguser.LastName

                    };
                    playBookings.Add(bookingDetail);
                }
            }

            return playBookings;
        }

        public async Task<IEnumerable<PlayBookingModel>> GetFacilityPitchBookingsDate(string dateFrom, string dateTo)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetAllPitchBookings --");
            var playBookings = new List<PlayBookingModel>();

            IEnumerable<UserPitchBooking> bookings = await context.UserPitchBookings.Where(u => u.IsEnabled == true).ToListAsync();
            if (bookings.Any())
            {
                foreach (var booking in bookings)
                {
                    var bookinguser = context.Users.Where(u => u.UserId == booking.UserId).FirstOrDefault();
                    var bookingDetail = new PlayBookingModel
                    {
                        BookingId = booking.BookingId,
                        Name = booking.Name,
                        FacilityId = booking.FacilityId,
                        PitchStart = booking.PitchStart,
                        PitchEnd = booking.PitchEnd,
                        Date = Helper.DisplayDateTime(booking.Date),
                        PlayerCount = booking.PlayerCount,
                        PricePerPlayer = booking.PricePerUser.GetValueOrDefault(),
                        PriceIncludingVat = booking.PlayerCount * booking.PricePerUser.GetValueOrDefault(),
                        TotalPrice = (decimal)((booking.PlayerCount * booking.PricePerUser.GetValueOrDefault()) + booking.Commission ?? default),
                        Commission = booking.Commission != null ? booking.Commission.GetValueOrDefault() : 0,
                        Facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId)?.FirstOrDefault(),
                        FacilityPitch = context.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId)?.FirstOrDefault(),
                        CreatedDate = booking.CreatedDate.Value,
                        UserName = bookinguser.FirstName + " " + bookinguser.LastName

                    };
                    playBookings.Add(bookingDetail);
                }
            }

            return playBookings.Where(x => x.CreatedDate >= Convert.ToDateTime(dateFrom) && x.CreatedDate < Convert.ToDateTime(dateTo).AddDays(1));
        }

        public async Task<PlayBookingModel> GetPitchBookingsDetails(Guid bookingID, Guid userId)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookingsDetails --");
            var playBookings = new List<PlayBookingModel>();
            var bookingDetail = new PlayBookingModel();

            UserPitchBooking booking = await context.UserPitchBookings.Where(u => u.IsEnabled == true && u.BookingId == bookingID).FirstOrDefaultAsync();
            if (booking != null)
            {
                var bookinguser = context.Users.Where(u => u.UserId == userId).FirstOrDefault();
                var currentuser = context.FacilityPlayers.Where(g => g.BookingId == bookingID && g.UserId == userId).FirstOrDefault();
                bool isCaptain = false;
                bool isParticipate = false;
                bool isRequestsent = false;
                bool IsWaitinglist = context.FacilityPlayers.Where(g => g.BookingId == bookingID && (g.PlayerStatus == EGamePlayerStatus.Approved || g.PlayerStatus == EGamePlayerStatus.CanInvite)).Count() == booking.PlayerCount;
                bool IsPaymentPending = false;
                bool IsCanInvite = false;
                bool IsWaitingRequest = false;
                if (currentuser != null && !string.IsNullOrWhiteSpace(currentuser.UserId.ToString()))
                {
                    isCaptain = currentuser.IsCaptain;
                    isRequestsent = currentuser.PlayerStatus == EGamePlayerStatus.Pending;
                    isParticipate = currentuser.PlayerStatus == EGamePlayerStatus.Approved || currentuser.PlayerStatus == EGamePlayerStatus.CanInvite;
                    IsCanInvite = currentuser.PlayerStatus == EGamePlayerStatus.CanInvite;
                    IsWaitingRequest = currentuser.PlayerStatus == EGamePlayerStatus.Waiting;
                    IsPaymentPending = !currentuser.IsPaid;
                }
                var locationName = "";
                var facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId).FirstOrDefault();
                var FacilityPitches = context.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId).FirstOrDefault();
                if (facility != null)
                {
                    locationName = facility.Street + " " + facility.Area + " " + facility.City + " " + facility.Country;
                }

                var TeamSizeName = FacilityPitches != null ? context.Sizes.Where(u => u.SizeId == FacilityPitches.TeamSize && u.IsEnabled == true).Select(x => x.SizeName).FirstOrDefault() : "";

                var SurfaceName = FacilityPitches != null ? context.Surfaces.Where(u => u.SurfaceId == FacilityPitches.SurfaceId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault() : "";

                var SportName = context.Sports.Where(u => u.SportId == booking.SportId).Select(x => x.Name).FirstOrDefault();

                bookingDetail = new PlayBookingModel
                {
                    BookingId = booking.BookingId,
                    Name = booking.Name,
                    FacilityId = booking.FacilityId,
                    PitchStart = booking.PitchStart,
                    PitchEnd = booking.PitchEnd,
                    SportId = booking.SportId,
                    Date = Helper.DisplayDateTime(booking.Date),
                    IsPrivate = booking.IsPrivate,
                    IsWaitinglist = IsWaitinglist,
                    PlayerCount = booking.PlayerCount,
                    PricePerPlayer = booking.PricePerUser.GetValueOrDefault(),
                    PriceIncludingVat = booking.PlayerCount * (booking.PricePerUser.GetValueOrDefault() + booking.PricePerUserVat.GetValueOrDefault()),
                    TotalPrice = (decimal)((booking.PlayerCount * (booking.PricePerUser.GetValueOrDefault() + booking.PricePerUserVat.GetValueOrDefault() + booking.Commission.GetValueOrDefault()))),
                    Commission = booking.Commission.GetValueOrDefault(),
                    Facility = facility,
                    FacilityPitch = FacilityPitches,
                    CreatedDate = booking.CreatedDate.Value,
                    UserName = bookinguser.FirstName + " " + bookinguser.LastName,
                    IsCaptain = isCaptain,
                    IsCanInvite = IsCanInvite,
                    IsParticipate = isParticipate,
                    IsWaitingRequest = IsWaitingRequest,
                    IsRequestsent = isRequestsent,
                    LocationName = locationName,
                    TeamSizeName = TeamSizeName,
                    SurfaceName = SurfaceName,
                    SportName = SportName,
                    IsPaid = booking.IsPaid,
                    Latitude = facility != null ? facility.Latitude : 0,
                    Longitude = facility != null ? facility.Longitude : 0,
                    IsPaymentPending = IsPaymentPending,
                    WaitingPlayerCount = context.FacilityPlayers.Count(a => a.BookingId == booking.BookingId && a.PlayerStatus == EGamePlayerStatus.Waiting),
                    RequestPlayerCount = context.FacilityPlayers.Count(a => a.BookingId == booking.BookingId && a.PlayerStatus == EGamePlayerStatus.Pending),
                    RegisteredPlayers = (from gameplayer in context.FacilityPlayers
                                         join user in context.Users
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
                    WaitingPlayers = (from gameplayer in context.FacilityPlayers
                                      join user in context.Users
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
            return bookingDetail;
        }


        public async Task<PlayBookingModel> GetPitchBookingsDetailsByTelRRefNo(string TelRRefNo)
        {
            LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookingByTelRRefNo --");
            var playBookings = new List<PlayBookingModel>();
            var bookingDetail = new PlayBookingModel();

            FacilityPlayer gamePlayer = await context.FacilityPlayers.Where(a => a.TelRRefNo == TelRRefNo).FirstOrDefaultAsync();
            if (gamePlayer != null)
            {
                UserPitchBooking booking = await context.UserPitchBookings.Where(u => u.IsEnabled == true && u.BookingId == gamePlayer.BookingId).FirstOrDefaultAsync();
                if (booking != null)
                {
                    var bookinguser = context.Users.Where(u => u.UserId == booking.UserId).FirstOrDefault();

                    var locationName = "";
                    var facility = context.Facilities.Where(u => u.FacilityId == booking.FacilityId).FirstOrDefault();
                    var FacilityPitches = context.FacilityPitches.Where(u => u.FacilityPitchId == booking.FacilityPitchId).FirstOrDefault();
                    if (facility != null)
                    {
                        locationName = facility.Street + " " + facility.Area + " " + facility.City + " " + facility.Country;
                    }
                    var TeamSizeName = context.Sizes.Where(u => u.SizeId == FacilityPitches.TeamSize && u.IsEnabled == true).Select(x => x.SizeName).FirstOrDefault();

                    var SurfaceName = context.Surfaces.Where(u => u.SurfaceId == FacilityPitches.SurfaceId && u.IsEnabled == true).Select(x => x.Name).FirstOrDefault();

                    var SportName = context.Sports.Where(u => u.SportId == booking.SportId).Select(x => x.Name).FirstOrDefault();

                    bookingDetail = new PlayBookingModel
                    {
                        BookingId = booking.BookingId,
                        Name = booking.Name,
                        FacilityId = booking.FacilityId,
                        PitchStart = booking.PitchStart,
                        PitchEnd = booking.PitchEnd,
                        Date = Helper.DisplayDateTime(booking.Date),
                        PricePerPlayer = booking.PricePerUser.GetValueOrDefault(),
                        PriceIncludingVat = booking.PlayerCount * (booking.PricePerUser.GetValueOrDefault() + booking.PricePerUserVat.GetValueOrDefault()),
                        TotalPrice = (decimal)((booking.PlayerCount * (booking.PricePerUser.GetValueOrDefault() + booking.PricePerUserVat.GetValueOrDefault() + booking.Commission))),
                        Commission = booking.Commission.GetValueOrDefault(),
                        PlayerCount = booking.PlayerCount,
                        Facility = facility,
                        FacilityPitch = FacilityPitches,
                        CreatedDate = booking.CreatedDate.Value,
                        UserName = bookinguser.FirstName + " " + bookinguser.LastName,
                        GamePlayerId = gamePlayer.FacilityPlayerId,
                        LocationName = locationName,
                        TeamSizeName = TeamSizeName,
                        SurfaceName = SurfaceName,
                        SportName = SportName
                    };

                }
            }
            return bookingDetail;
        }




        #region email
        private int SendEmailToCaptain(PlayRequest playRequest, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            //get captain of booking
            UserPitchBooking booking = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId).FirstOrDefault();
            if (booking != null)
            {
                Guid captainUserId = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId && x.IsCaptain == true).Select(d => d.UserId).FirstOrDefault();
                User captain = context.Users.Where(x => x.UserId == captainUserId).FirstOrDefault();
                User requester = context.Users.Where(x => x.UserId == playRequest.UserId).FirstOrDefault();
                Facility facility = context.Facilities.Where(x => x.FacilityId == playRequest.FacilityId).FirstOrDefault();

                var emailBody = String.Format(APIConfig.PlayRequestConfig.RequestMessage, captain.FirstName.ToUpper(), booking.BookingId, requester.FirstName + requester.LastName, facility.Name, booking.Date, booking.PitchStart.ToShortTimeString() + '-' + booking.PitchEnd.ToShortDateString());

                var EmailParam = _conf.MailConfig;
                EmailParam.To = new List<string>() { captain.Email };
                EmailParam.Subject = "Sidekick Admin: Request to Participate.";
                EmailParam.Body = emailBody;

                int sendStatus = SendEmailByEmailAddress(new List<string>() { captain.Email }, EmailParam, LogManager);
                return sendStatus;
            }
            else return 1;
        }

        private int SendBookingConfirmationToCaptain(UserPitchBooking booking, FacilitySport sports, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            UserPitchBooking pitchBooking = context.UserPitchBookings.Where(x => x.BookingId == booking.BookingId).FirstOrDefault();
            if (booking != null)
            {
                User captain = context.Users.Where(x => x.UserId == booking.UserId).FirstOrDefault();
                Facility facility = context.Facilities.Where(x => x.FacilityId == booking.FacilityId).FirstOrDefault();

                //var emailBody = String.Format(APIConfig.PlayRequestConfig.RequestMessage, captain.FirstName.ToUpper(), booking.BookingId, captain.FirstName + captain.LastName, facility.Name, booking.Date, booking.PitchStart.ToShortTimeString() + '-' + booking.PitchEnd.ToShortDateString());

                string sportName = "", facilityName = "", area = "", pitchPrice = "0", serviceFees = "0", totalPrice = "0", priceParPlayer = "0";
                var emailBody = String.Format(APIConfig.BookingNotificationConfig.PitchBookingConfirmationToCaptain,
                    captain.FirstName.ToUpper(),
                    sportName,
                    facilityName,
                    booking.Date.ToString() + '-' + booking.PitchStart.ToShortTimeString() + '-' + booking.PitchEnd.ToShortDateString(),
                    area,
                    pitchPrice,
                    serviceFees,
                    totalPrice,
                    priceParPlayer
                    );

                var EmailParam = _conf.MailConfig;
                EmailParam.To = new List<string>() { "jaycelle.rafanan@itfaq.global" };
                EmailParam.Subject = APIConfig.BookingNotificationConfig.PitchBookingConfirmationEmailSubjectToCaptain;
                EmailParam.Body = APIConfig.BookingNotificationConfig.PitchBookingConfirmationEmailSubjectToCaptain;

                int sendStatus = SendEmailByEmailAddress(new List<string>() { "jaycelle.rafanan@itfaq.global" }, EmailParam, LogManager);
                return sendStatus;
            }
            else return 1;
        }
        private int SendRequestConfirmation(PlayRequest playRequest, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            UserPitchBooking booking = context.UserPitchBookings.Where(x => x.BookingId == playRequest.BookingId).FirstOrDefault();
            if (booking != null)
            {
                User requester = context.Users.Where(x => x.UserId == playRequest.UserId).FirstOrDefault();
                Facility facility = context.Facilities.Where(x => x.FacilityId == playRequest.FacilityId).FirstOrDefault();

                var emailBody = String.Format(APIConfig.PlayRequestConfig.RequestMessage, requester.FirstName.ToUpper(), booking.BookingId, requester.FirstName + requester.LastName, facility.Name, booking.Date, booking.PitchStart.ToShortTimeString() + '-' + booking.PitchEnd.ToShortDateString());

                var EmailParam = _conf.MailConfig;
                EmailParam.To = new List<string>() { requester.Email };
                EmailParam.Subject = "Sidekick Admin: Request Confirmation.";
                EmailParam.Body = emailBody;

                int sendStatus = SendEmailByEmailAddress(new List<string>() { requester.Email }, EmailParam, LogManager);
                return sendStatus;
            }
            else return 1;
        }


        #endregion

        public async Task<MyPlayBookingResponseModel> MyPlayBooking(Guid userId)
        {
            MyPlayBookingResponseModel bookingResponseModel = new MyPlayBookingResponseModel();
            var pitchBookings = await context.UserPitchBookings.Where(x => x.UserId == userId).ToListAsync();
            var upComingBooking = pitchBookings.Where(x => x.Date.Date >= Helper.GetDateTime().Date).ToList();
            var BookingHistory = pitchBookings.Where(x => x.Date.Date < Helper.GetDateTime().Date).ToList();

            foreach (var pitchBookingsItem in upComingBooking)
            {
                PlayBookingResponseModel responseModel = BindBooking(userId, pitchBookingsItem);
                bookingResponseModel.UpComingBooking.Add(responseModel);
            }

            foreach (var pitchBookingsItem in BookingHistory)
            {
                PlayBookingResponseModel responseModel = BindBooking(userId, pitchBookingsItem);
                bookingResponseModel.BookingHistory.Add(responseModel);
            }

            return bookingResponseModel;
        }

        private PlayBookingResponseModel BindBooking(Guid userId, UserPitchBooking pitchBookingsItem)
        {
            PlayBookingResponseModel responseModel = new PlayBookingResponseModel();
            var facility = context.Facilities.Where(x => x.FacilityId == pitchBookingsItem.FacilityId).FirstOrDefault();
            var sport = context.Sports.Where(x => x.SportId == pitchBookingsItem.SportId).FirstOrDefault();

            responseModel.BookingId = pitchBookingsItem.BookingId;
            responseModel.FacilityId = pitchBookingsItem.FacilityId;
            responseModel.FacilityName = facility != null ? facility.Name : "";
            responseModel.Date = Helper.DisplayDateTime(pitchBookingsItem.Date);

            responseModel.PitchStart = pitchBookingsItem.PitchStart;
            responseModel.PitchEnd = pitchBookingsItem.PitchEnd;

            responseModel.PricePerPlayer = pitchBookingsItem.PricePerUser.GetValueOrDefault();
            responseModel.SportName = sport != null ? sport.Name : "";
            responseModel.SportImageUrl = sport != null ? sport.Icon : "";
            responseModel.TotalPrice = (decimal)((pitchBookingsItem.PlayerCount * (pitchBookingsItem.PricePerUser.GetValueOrDefault()) + pitchBookingsItem.Commission.GetValueOrDefault()));
            responseModel.IsPaid = pitchBookingsItem.IsPaid;
            responseModel.IsPrivate = pitchBookingsItem.IsPrivate;
            return responseModel;
        }

        public async Task PitchBookingConfirmationToFacility(Guid bookingId, Guid userId)
        {
            var currentCaptainuser = await context.FacilityPlayers.Where(g => g.BookingId == bookingId && g.UserId == userId && g.IsCaptain == true).FirstOrDefaultAsync();
            if (currentCaptainuser != null && !string.IsNullOrWhiteSpace(currentCaptainuser.BookingId.ToString()))
            {
                UserPitchBooking pitchBooking = context.UserPitchBookings.Where(x => x.BookingId == bookingId).FirstOrDefault();
                FacilityPitch facilityPitch = context.FacilityPitches.Where(x => x.FacilityPitchId == pitchBooking.FacilityPitchId).FirstOrDefault();
                Facility facility = context.Facilities.Where(x => x.FacilityId == pitchBooking.FacilityId).FirstOrDefault();
                Sport sportsDetails = context.Sports.Where(x => x.SportId == pitchBooking.SportId).FirstOrDefault();
                User Playeruser = context.Users.FirstOrDefault(x => x.UserId == userId);
                Location location = context.Locations.FirstOrDefault(x => x.LocationId == facilityPitch.LocationId);

                string LocationName = string.Empty;

                if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                {
                    LocationName = location.Name;
                }



                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    BookingId = pitchBooking.BookingId,
                    FacilityName = facility.Name,
                    UserName = Playeruser.FirstName,
                    CaptainName = Playeruser.FirstName,
                    Sport = sportsDetails.Name,
                    BookingDate = pitchBooking.PitchStart,
                    BookingTime = pitchBooking.PitchStart.ToShortTimeString() + "-" + pitchBooking.PitchEnd.ToShortTimeString(),
                    Location = LocationName,
                    PricePitch = facilityPitch.FixedPrice,
                    ServiceFees = 0,
                    FacilityId = facility.FacilityId,
                    PitchName = facilityPitch.Name,
                    NotificationType = (int)ENotificationType.PitchBooking,
                    UserId = userId
                };

                await iUserNotificationHandler.PitchBookingConfirmationToFacility(commonTemplate);
            }
        }


        public async Task<APIResponse> FacilitySendContactMessageToPlayer(FacilitySendContactMessageToPlayerRequestModel messageToPlayerRequestModel)
        {
            try
            {
                LogManager.LogInfo("-- Run::PlayRepository::GetPitchBookingByTelRRefNo --");
                var playBookings = new List<PlayBookingModel>();
                var bookingDetail = new PlayBookingModel();

                var gamePlayers = await context.FacilityPlayers.Where(a => a.BookingId == messageToPlayerRequestModel.BookingId && a.IsEnabled == true).ToListAsync();

                if (gamePlayers == null || (gamePlayers != null && !gamePlayers.Any()))
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                foreach (var playerDetail in gamePlayers)
                {
                    try
                    {
                        // get player user detials..
                        BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                        {
                            Message = messageToPlayerRequestModel.Message,
                            BookingId = messageToPlayerRequestModel.BookingId
                        };
                        var user = await context.Users.Where(u => u.UserId == playerDetail.UserId && u.IsEnabled == true).FirstOrDefaultAsync();
                        if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                        {
                            commonTemplate.EmailTo = new List<string>() { user.Email };
                            await EmailTemplatesHelper.FacilitySendContactMessageToPlayer(APIConfig, LogManager, commonTemplate);
                        }
                    }
                    catch (Exception ex)
                    {

                        LogManager.LogInfo("-- Run::PlayRepository::--FacilitySendContactMessageToPlayer" + ex.Message);
                    }
                }
                return new APIResponse()
                {
                    Message = "email send successfully",
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Run::PlayRepository::--FacilitySendContactMessageToPlayer" + ex.Message);
                return new APIResponse
                {
                    Message = "Error occurs while send email",
                    Status = "Error!",
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }

        }

        public async Task SendPlayPaymentFailNotification(Guid bookingId, Guid userId)
        {
            try
            {
                var currentPlayeruser = await context.FacilityPlayers.Where(g => g.BookingId == bookingId && g.UserId == userId).FirstOrDefaultAsync();
                if (currentPlayeruser != null && !string.IsNullOrWhiteSpace(currentPlayeruser.BookingId.ToString()))
                {
                    UserPitchBooking pitchBooking = context.UserPitchBookings.Where(x => x.BookingId == bookingId).FirstOrDefault();
                    FacilityPitch facilityPitch = context.FacilityPitches.Where(x => x.FacilityPitchId == pitchBooking.FacilityPitchId).FirstOrDefault();
                    Facility facility = context.Facilities.Where(x => x.FacilityId == pitchBooking.FacilityId).FirstOrDefault();
                    Sport sportsDetails = context.Sports.Where(x => x.SportId == pitchBooking.SportId).FirstOrDefault();
                    User Playeruser = context.Users.FirstOrDefault(x => x.UserId == userId);
                    Location location = context.Locations.FirstOrDefault(x => x.LocationId == facilityPitch.LocationId);

                    string LocationName = string.Empty;

                    if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                    {
                        LocationName = location.Name;
                    }

                    BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                    {
                        BookingId = pitchBooking.BookingId,
                        FacilityName = facility.Name,
                        UserName = Playeruser.FirstName,
                        CaptainName = Playeruser.FirstName,
                        Sport = sportsDetails.Name,
                        BookingDate = pitchBooking.PitchStart,
                        BookingTime = pitchBooking.PitchStart.ToShortTimeString() + "-" + pitchBooking.PitchEnd.ToShortTimeString(),
                        Location = LocationName,
                        PricePitch = facilityPitch.FixedPrice,
                        ServiceFees = 0,
                        FacilityId = facility.FacilityId,
                        PitchName = facilityPitch.Name,
                        NotificationType = (int)ENotificationType.PitchBooking,
                        UserId = userId
                    };
                    var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(userId);
                    if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                    {
                        // send notification
                        List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                        await pushNotificationTemplateRepository.PaymentFailPlay(APIConfig, LogManager, DeviceFCMTokens, commonTemplate);
                    }
                    EmailTemplatesHelper.PaymentFailedForPlay(APIConfig, LogManager, commonTemplate);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SENDEMAIL);
                LogManager.LogException(ex);
            }

        }
    }

}
