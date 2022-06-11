
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using System.Threading.Tasks;
using Sidekick.Model.Player;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class FacilityPlayerRepository : APIBaseRepo, IFacilityPlayerRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public FacilityPlayerRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse GetFacilityPlayers(Guid _facilityId)
        {
            APIResponse aResp = new();

            LogManager.LogInfo("-- Run::FaciityPlayerRepository::GetFacilityPlayers --");
            LogManager.LogDebugObject(_facilityId);

            try
            {
                var players =  DbContext.FacilityPlayers.Where(f => f.FacilityId == _facilityId && f.IsEnabled == true).ToList();
                var viewModel = new List<FacilityPlayerViewModel>();
                var areas = DbContext.Areas.ToList();
                var userPitchBookings = DbContext.UserPitchBookings.ToList();
                foreach (var player in players)
                {
                    var facilityPlayer = new FacilityPlayerViewModel();
                    facilityPlayer.UserId = player.UserId;
                    facilityPlayer.Name = player.Name;
                    facilityPlayer.FirstName = player.FirstName;
                    facilityPlayer.LastName = player.LastName;
                    facilityPlayer.Email = player.Email;
                    facilityPlayer.IsPaid = player.IsPaid;
                    facilityPlayer.ProfileImgUrl = player.ProfileImgUrl;
                    facilityPlayer.AreaName = areas.Where(a => a.AreaId == player.AreaId.Value).FirstOrDefault() != null ? areas.Where(a => a.AreaId == player.AreaId.Value).FirstOrDefault().AreaName : string.Empty;
                    facilityPlayer.CreatedDate = player.DateCreated.Value;
                    var pitchBookings = userPitchBookings.Where(u => u.UserId == player.UserId).ToList();
                    if (pitchBookings.Any())
                    {
                        facilityPlayer.LastBooking = pitchBookings.OrderByDescending(p => p.CreatedDate).FirstOrDefault().CreatedDate.Value.ToString("dd/MM/yyy");
                    }
                    else
                    {
                        facilityPlayer.LastBooking = string.Empty;
                    }

                    viewModel.Add(facilityPlayer);
                }

                return aResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = viewModel,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::GetFacilityPlayers --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> DeleteFacilityPlayer(Guid _facilityId, string userNo)
        {
            APIResponse aResp = new();
            try
            {
                FacilityPlayer player = null;
                player = DbContext.FacilityPlayers.Where(w => w.FacilityId == _facilityId && w.UserNo == userNo).First();
                player.IsEnabled = false;
                DbContext.FacilityPlayers.Update(player);
                await DbContext.SaveChangesAsync();
                return aResp = new APIResponse
                {
                    Message = "Player Successfully Deleted",
                    Status = "Success!",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::GetFacilityPlayers --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> GetAllFacilityPlayers()
        {
            APIResponse aResp = new();

            LogManager.LogInfo("-- Run::FaciityPlayerRepository::GetAllFacilityPlayers --");

            try
            {
                var players = await DbContext.Users.Where(f => f.IsEnabled == true).ToListAsync();
                var viewModel = new List<FacilityPlayer>();
                var userPitchBookings = DbContext.UserPitchBookings.ToList();
                if (userPitchBookings != null)
                {
                    foreach (var player in players)
                    {
                        var facilityPlayer = new FacilityPlayer
                        {
                            Name = player.FirstName + " " + player.LastName,
                            FirstName = player.FirstName,
                            LastName = player.LastName,
                            Email = player.Email,
                            UserId = player.UserId,
                            Profile = player.ImageUrl,
                            ProfileImgUrl = player.ImageUrl,
                            ContactNumber = player.MobileNumber,
                            DateCreated = player.CreatedDate.Value
                        };

                        var pitchBooking = userPitchBookings.Where(u => u.UserId == player.UserId).OrderByDescending(u => u.CreatedDate).FirstOrDefault();
                        if (pitchBooking != null)
                        {
                            facilityPlayer.FacilityId = pitchBooking.FacilityId;
                            facilityPlayer.LastBooking = pitchBooking.CreatedDate.Value;
                        }

                        viewModel.Add(facilityPlayer);
                    }
                }

                return aResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = viewModel,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::GetAllFacilityPlayers --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> GetAllFacilityPlayersDates(string dateFrom, string dateTo)
        {
            APIResponse aResp = new();

            LogManager.LogInfo("-- Run::FaciityPlayerRepository::GetAllFacilityPlayers --");

            try
            {
                var players = await DbContext.FacilityPlayers.Select(i => new FacilityPlayer
                {
                    FacilityId = i.FacilityId,
                    UserNo = i.UserNo,
                    Profile = i.Profile,
                    Name = i.Name,
                    DateCreated = i.DateCreated,
                    LastBooking = i.LastBooking
                }).Where(x => x.DateCreated >= Convert.ToDateTime(dateFrom) && x.DateCreated < Convert.ToDateTime(dateTo).AddDays(1)).ToListAsync();
                //IEnumerable<FacilityPlayer> players = null;
                //players = DbContext.FacilityPlayers.AsNoTracking()
                //    .Select(i => new FacilityPlayer
                //    {
                //        FacilityId = i.FacilityId,
                //        UserNo = i.UserNo,
                //        Profile = i.Profile,
                //        Name = i.Name,
                //        Area = i.Area,
                //        City = i.City,
                //        DateCreated = i.DateCreated,
                //        LastBooking = i.LastBooking

                //    }).ToList();

                //if (players.Any())
                //{
                //    foreach (var player in players)
                //    {
                //        FacilityPlayer details = DbContext.FacilityPlayers.Where(x => x.UserNo == player.UserNo).FirstOrDefault();
                //        player.Name = details.Name;
                //        player.Profile = details.Profile;
                //        player.UserNo = details.UserNo;
                //        player.Area = details.Area;
                //        player.City = details.City;
                //        player.DateCreated = details.DateCreated;
                //        player.LastBooking = details.LastBooking;
                //    }
                //}

                return aResp = new APIResponse
                {
                    Message = "All records found.",
                    Status = "Success!",
                    Payload = players,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::GetAllFacilityPlayers --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public async Task<APIResponse> AddOrEditPlayer(string auth, PlayerViewModel player)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPlayerRepository::AddOrEditPlayer --");
            LogManager.LogDebugObject(player);

            try
            {
                var isLoggedIn = await DbContext.AdminLoginTransactions.FirstOrDefaultAsync(alt => alt.Token == auth);
                if (isLoggedIn == null)
                {
                    return new APIResponse
                    {
                        Message = "Unauthorized!",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                //a player when added in BO is considered as a NORMAL USER
                if (player.UserId != Guid.Empty)
                {
                    var playerExisting = await DbContext.FacilityPlayers.Where(f => f.UserId == player.UserId).ToListAsync();
                    if (playerExisting.Count == 0)
                    {
                        var facilityPlayer = new FacilityPlayer()
                        {
                            UserId = player.UserId,
                            Name = player.FirstName + " " + player.LastName,
                            AreaId = player.AreaId,
                            Email = player.Email,
                            ProfileImgUrl = player.ImageUrl,
                            ContactNumber = player.MobileNumber,
                            FacilityPlayerId = Guid.NewGuid(),
                            BookingId = Guid.Empty,
                            IsPaid = false,
                            IsCanInvite = false,
                            IsCaptain = false,

                            LastEditedBy = isLoggedIn.AdminId,
                            LastEditedDate = DateTime.Now,
                            CreatedBy = isLoggedIn.AdminId,
                            CreatedDate = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = isLoggedIn.AdminId,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                        };

                        DbContext.FacilityPlayers.Add(facilityPlayer);
                        await DbContext.SaveChangesAsync();

                        return new APIResponse
                        {
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Message = "Successfully added a player!",
                        };
                    }

                    //update
                    var playerInfos = new List<FacilityPlayer>();
                    foreach (var playerInfo in playerExisting)
                    {
                        playerInfo.Name = player.FirstName + " " + player.LastName;
                        playerInfo.FirstName = player.FirstName;
                        playerInfo.LastName = player.LastName;
                        playerInfo.ProfileImgUrl = player.ImageUrl;
                        playerInfo.Profile = player.ImageUrl;
                        playerInfo.AreaId = player.AreaId;
                        playerInfo.ContactNumber = player.MobileNumber;
                        //playerInfo.IsEnabled = player.IsActive;
                        playerInfo.LastEditedBy = isLoggedIn.AdminId;
                        playerInfo.LastEditedDate = DateTime.Now;

                        playerInfos.Add(playerInfo);
                    }

                    DbContext.FacilityPlayers.UpdateRange(playerInfos);

                    UserAddress userAddress = await DbContext.UserAddresses.Where(ua => ua.UserId == player.UserId).FirstOrDefaultAsync();
                    if (userAddress != null)
                    {
                        userAddress.AreaId = player.AreaId;
                        userAddress.LastEditedBy = isLoggedIn.AdminId;
                        userAddress.LastEditedDate = DateTime.Now;
                        DbContext.UserAddresses.Update(userAddress);
                    }

                    await DbContext.SaveChangesAsync();

                    return new APIResponse
                    {
                        Message = "Player record updated successfully.",
                        StatusCode = System.Net.HttpStatusCode.OK,
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::AddOrEditPlayer --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> GetPlayer(Guid userId)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPlayerRepository::GetPlayer --");
            LogManager.LogDebugObject(userId);

            try
            {
                var playerProfile = await (from user in DbContext.Users
                              where user.UserId == userId
                              select new PlayerViewModel
                              {
                                  UserId = user.UserId,
                                  FirstName = user.FirstName,
                                  LastName = user.LastName,
                                  Email = user.Email,
                                  MobileNumber = user.MobileNumber,
                                  NationalityId = user.NationalityId,
                                  ImageUrl = user.ImageUrl,
                                  IsActive = user.IsEnabled.Value
                              }).FirstOrDefaultAsync();

                if (playerProfile != null)
                {
                    var userAddress = await DbContext.UserAddresses.Where(ua => ua.UserId == userId).FirstOrDefaultAsync();
                    if (userAddress != null)
                    {
                        playerProfile.AreaId = userAddress.AreaId;
                    }

                    var groupBookingViewModel = new List<PlayerBookingsViewModel>();
                    var groupBookings = await DbContext.GroupBookings.Where(g => g.ParticipantId == userId).ToListAsync();
                    if (groupBookings.Any())
                    {
                        foreach (var groupBooking in groupBookings)
                        {
                            var groupClass = DbContext.GroupClasses.Where(g => g.GroupClassId == groupBooking.GroupClassId).FirstOrDefault();
                            if (groupClass != null)
                            {
                                groupBookingViewModel.Add(new PlayerBookingsViewModel
                                {
                                    Title = groupClass.Title,
                                    Start = groupClass.Start.Value,
                                    End = groupClass.Start.GetValueOrDefault().AddHours(groupClass.Duration.GetValueOrDefault()),
                                    Date = groupClass.Start.Value,
                                    Price = groupClass.Price
                                });
                            }
                        }

                        if (groupBookingViewModel.Any())
                        {
                            playerProfile.Bookings = groupBookingViewModel;
                            return new APIResponse
                            {
                                Message = "Player not Found",
                                Payload = playerProfile,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                }

                return new APIResponse
                {
                    Message = "Player record retrieved.",
                    Payload = playerProfile,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::GetPlayer --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                aResp.Message = "Something Went wrong!";
                aResp.Status = "Internal Server Error";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> ChangeStatus(string auth, ChangeStatus user)
        {
            var aResp = new APIResponse();

            LogManager.LogInfo("-- Run::FaciityPlayerRepository::ChangeStatus --");
            LogManager.LogDebugObject(user);
            try
            {
                var isLoggedIn = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (isLoggedIn == null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.Unauthorized,
                        Message = "Unauthorized",
                    };
                }

                DateTime todaysDate = DateTime.Now;
                var hasExistingBooking = await DbContext.UserPitchBookings.AsNoTracking().Where(u => u.UserId == user.GuID && u.CreatedDate >= todaysDate.Date).OrderByDescending(u => u.CreatedDate).FirstOrDefaultAsync();
                if (hasExistingBooking != null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.Found,
                        Message = "Player cannot be deleted due to on-going or upcoming bookings.",
                    };
                }

                var userExisting = await DbContext.Users.Where(u => u.UserId == user.GuID).FirstOrDefaultAsync();
                if (userExisting == null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        Message = "Player record not found.",
                    };
                }

                var isPlayerACoach = await DbContext.Coaches.Where(u => u.CoachUserId == user.GuID).FirstOrDefaultAsync();
                if (isPlayerACoach != null)
                {
                    var hasExistingClasses = await DbContext.GroupClasses.AsNoTracking().Where(u => u.CoachId == user.GuID && u.CreatedDate >= todaysDate.Date).OrderByDescending(u => u.CreatedDate).FirstOrDefaultAsync();
                    if (hasExistingBooking != null)
                    {
                        return new APIResponse
                        {
                            StatusCode = System.Net.HttpStatusCode.Found,
                            Message = "Player is also a coach and cannot be deleted due to on-going or upcoming classes.",
                        };
                    }

                    isPlayerACoach.IsEnabled = false;
                    isPlayerACoach.LastEditedDate = todaysDate;
                    isPlayerACoach.LastEditedBy = isLoggedIn.AdminId;

                    DbContext.Coaches.Update(isPlayerACoach);
                }

                var isPlayerExisting = await DbContext.FacilityPlayers.Where(f => f.UserId == user.GuID).ToListAsync();
                if (isPlayerExisting.Count > 0)
                {
                    foreach (var player in isPlayerExisting)
                    {
                        player.IsEnabled = false;
                        player.LastEditedDate = todaysDate;
                        player.LastEditedBy = isLoggedIn.AdminId;

                        DbContext.FacilityPlayers.Update(player);
                    }
                }

                userExisting.IsEnabled = false;
                userExisting.LastEditedDate = todaysDate;
                userExisting.LastEditedBy = isLoggedIn.AdminId;

                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Player deleted successfully.",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::FaciityPlayerRepository::ChangeStatus --");
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
