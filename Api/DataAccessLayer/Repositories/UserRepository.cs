using CountryData.Standard;
using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Badges;
using Sidekick.Model.Booking;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class UserRepository : APIBaseRepo, IUserRepository
    {
        readonly APIDBContext DbContext;
        private readonly IUserHelper userHelper;

        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        ICoachRepository ICoachRepo { get; }

        public UserRepository(APIDBContext _dbCtxt,
            ILoggerManager _logManager,
            APIConfigurationManager _apiCon,
            ICoachRepository _iCoachRepo,
            IUserHelper userHelper)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            ICoachRepo = _iCoachRepo;
            this.userHelper = userHelper;
        }

        public APIResponse Add(UserRegistration _user, IMainHttpClient _mhttpc = null, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::Add --");
            LogManager.LogDebugObject(_user);

            try
            {
                // check mobile number
                User FoundUser = DbContext.Users.FirstOrDefault(u => u.MobileNumber == _user.MobileNumber);
                if (FoundUser != null)
                {
                    LogManager.LogWarn("User mobile already used:" + FoundUser.MobileNumber + " was already in use!");
                    FoundUser.Password = "";
                    LogManager.LogDebugObject(FoundUser);
                    apiResp = new APIResponse
                    {
                        Message = "Mobile Number already in used!",
                        Status = "Error",
                        StatusCode = System.Net.HttpStatusCode.Found
                    };
                    return apiResp;
                }

                // check email 
                FoundUser = DbContext.Users.FirstOrDefault(u => u.Email.ToLower() == _user.Email.ToLower());
                if (FoundUser != null)
                {
                    LogManager.LogWarn("User email address already used:" + FoundUser.Email + " was already in use!");
                    FoundUser.Password = "";
                    LogManager.LogDebugObject(FoundUser);
                    apiResp = new APIResponse
                    {
                        Message = "Email already in used!",
                        Status = "Error",
                        StatusCode = System.Net.HttpStatusCode.Found
                    };
                    return apiResp;
                }

                // checks passed continue to create the new user
                Guid UserGuid = Guid.NewGuid();
                DateTime RegistrationDate = DateTime.Now;
                User NewUser = new User
                {
                    UserId = UserGuid,
                    Email = _user.Email,
                    FirstName = _user.FirstName,
                    LastName = _user.LastName,
                    Gender = _user.Gender,
                    UserType = EUserType.NORMAL,
                    MobileNumber = _user.MobileNumber,
                    DateOfBirth = null,
                    DeviceRegistrationPlatform = _user.DevicePlatform,
                    UserRegistrationPlatform = _user.UserRegistrationPlatform,
                    LastEditedBy = UserGuid,
                    LastEditedDate = RegistrationDate,
                    CreatedBy = UserGuid,
                    CreatedDate = RegistrationDate,
                    IsEnabledBy = UserGuid,
                    IsEnabled = false,
                    DateEnabled = RegistrationDate,
                    IsLocked = true,
                    LockedDateTime = null,
                    ImageUrl = _user.ImageUrl
                };
                NewUser.Password = NewUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key);

                //user was added from the Backoffice
                if (_user.UserRegistrationPlatform == UserRegistrationPlatform.API)
                {
                    NewUser.IsEnabled = true;
                    NewUser.IsLocked = false;
                }

                //create default user address record
                UserAddress userAddress = new()
                {
                    UserId = NewUser.UserId,
                    CreatedBy = UserGuid,
                    CreatedDate = DateTime.Now,
                    DateEnabled = DateTime.Now,
                    IsEnabledBy = UserGuid,
                    IsEnabled = true,
                    IsLocked = false,
                    LockedDateTime = null,
                    AreaId = _user.AreaId ?? 0
                };
                DbContext.UserAddresses.Add(userAddress);

                //send verification code via email for testing initial setup
                string registrationCode = GenerateUniqueCode(4, true, false);
                LogManager.LogInfo("-- Run::GenerateUniqueCode -- [Registration Code] -- " + registrationCode);

                var EmailParam = _conf.MailConfig;
                EmailParam.To = new List<string>() { NewUser.Email };
                EmailParam.Subject = APIConfig.MsgConfigs.RegisterMobileUserEmailSubject;

                EmailParam.Body = String.Format(APIConfig.MsgConfigs.RegisterMobileUser, NewUser.FirstName, registrationCode);

                EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager);

                var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { NewUser.Email }, EmailParam, LogManager);

                if (_conf.SmsConfig.IsEnable)
                {
                    SmsParameter smsParameter = new SmsParameter();
                    smsParameter.To = NewUser.MobileNumber;
                    smsParameter.Text = String.Format(APIConfig.SMSTemplateConfig.OTPForRegistration, registrationCode);
                    smsParameter.Action = APIConfig.SmsConfig.Action;
                    smsParameter.From = APIConfig.SmsConfig.From;
                    smsParameter.User = APIConfig.SmsConfig.User;
                    smsParameter.Password = APIConfig.SmsConfig.Password;
                    // uncomment code once sms function working.
                    string smsstatus = SendSmsAsync(_mhttpc, smsParameter).GetAwaiter().GetResult();
                }


                if (sendStatus == 0)
                {
                    DbContext.Users.Add(NewUser);
                    DbContext.SaveChanges();

                    SaveVerificationCode(NewUser.UserId, NewUser.Email, NewUser.MobileNumber, EVerificationType.RegisterAccount, registrationCode);

                    NewUser.Password = string.Empty;
                    return apiResp = new APIResponse
                    {
                        Message = "We have sent you a registration code via SMS.",
                        Status = "Success",
                        Payload = NewUser,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Registration failed.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::Add --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> Update(UserProfile _userProfile)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::Update --");
            LogManager.LogDebugObject(_userProfile);

            try
            {
                // check mobile number
                User FoundUser = await GetUser(_userProfile.UserId);
                if (FoundUser != null)
                {
                    Guid Updater = userHelper.GetCurrentUserGuidLogin();
                    FoundUser.LastEditedBy = Updater;
                    FoundUser.LastEditedDate = DateTime.Now;

                    FoundUser.DateOfBirth = _userProfile.DateOfBirth;
                    FoundUser.Description = _userProfile.Description;
                    FoundUser.Gender = _userProfile.Gender;
                    FoundUser.FirstName = _userProfile.FirstName;
                    FoundUser.LastName = _userProfile.LastName;
                    FoundUser.Email = _userProfile.Email;
                    FoundUser.MobileNumber = _userProfile.MobileNumber;
                    FoundUser.ImageUrl = _userProfile.ImageUrl;

                    // get user address
                    UserAddress uAdd = DbContext.UserAddresses.FirstOrDefault(ua => ua.Id == _userProfile.UserAddress.Id);
                    if (uAdd == null)
                    {
                        UserAddress newUAdd = new UserAddress
                        {
                            AddressName = _userProfile.UserAddress.AddressName,
                            AddressNote = _userProfile.UserAddress.AddressNote,
                            City = _userProfile.UserAddress.City,
                            DoorNum = _userProfile.UserAddress.DoorNum,
                            FloorNum = _userProfile.UserAddress.FloorNum,
                            IsCurrentAddress = _userProfile.UserAddress.IsCurrentAddress,
                            IsEnabled = _userProfile.UserAddress.IsEnabled,
                            Latitude = _userProfile.UserAddress.Latitude,
                            Longitude = _userProfile.UserAddress.Longitude,
                            PostalCode = _userProfile.UserAddress.PostalCode,
                            Street = _userProfile.UserAddress.Street,
                            UserId = _userProfile.UserAddress.UserId,
                            CountryName = _userProfile.UserAddress.CountryName,
                            CreatedBy = FoundUser.UserId,
                            CreatedDate = DateTime.Now,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            IsEnabledBy = FoundUser.UserId,
                            LastEditedBy = FoundUser.UserId,
                            LastEditedDate = DateTime.Now,
                            LockedDateTime = null,
                            CountryAlpha3Code = _userProfile.UserAddress.CountryAlpha3Code
                        };
                        DbContext.UserAddresses.Add(newUAdd);
                    }
                    else
                    {
                        uAdd.AddressName = _userProfile.UserAddress.AddressName;
                        uAdd.AddressNote = _userProfile.UserAddress.AddressNote;
                        uAdd.City = _userProfile.UserAddress.City;
                        uAdd.DoorNum = _userProfile.UserAddress.DoorNum;
                        uAdd.FloorNum = _userProfile.UserAddress.FloorNum;
                        uAdd.IsCurrentAddress = _userProfile.UserAddress.IsCurrentAddress;
                        uAdd.Latitude = _userProfile.UserAddress.Latitude;
                        uAdd.Longitude = _userProfile.UserAddress.Longitude;
                        uAdd.PostalCode = _userProfile.UserAddress.PostalCode;
                        uAdd.Street = _userProfile.UserAddress.Street;
                        uAdd.UserId = _userProfile.UserAddress.UserId;
                        uAdd.CountryName = _userProfile.UserAddress.CountryName;
                        uAdd.LastEditedBy = FoundUser.UserId;
                        uAdd.LastEditedDate = DateTime.Now;
                        uAdd.CountryAlpha3Code = _userProfile.UserAddress.CountryAlpha3Code;
                        DbContext.UserAddresses.Update(uAdd);
                    }

                    DbContext.Update(FoundUser);
                    DbContext.SaveChanges();

                    apiResp.Message = "User Profile Update Success!";
                    apiResp.Status = "Success!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    apiResp.Payload = FoundUser;
                    return apiResp;
                }
                else
                {
                    apiResp.Message = "User Profile Update failed! Invalid UserId";
                    apiResp.Status = "Failed!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    apiResp.Payload = _userProfile;
                    return apiResp;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::Update --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<User> UpdateUser(User user,
            UserProfile updatedUserProfile)
        {
            Guid Updater = userHelper.GetCurrentUserGuidLogin();
            user.LastEditedBy = Updater;
            user.LastEditedDate = DateTime.Now;

            user.DateOfBirth = updatedUserProfile.DateOfBirth;
            user.Description = updatedUserProfile.Description;
            user.Gender = updatedUserProfile.Gender;
            user.FirstName = updatedUserProfile.FirstName;
            user.LastName = updatedUserProfile.LastName;
            user.Email = updatedUserProfile.Email;
            user.MobileNumber = updatedUserProfile.MobileNumber;
            user.ImageUrl = updatedUserProfile.ImageUrl;
            user.NationalityId = updatedUserProfile.NationalityId;
            DbContext.Users.Update(user);
            await DbContext.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUser(User user)
        {
            DbContext.Update(user);
            await DbContext.SaveChangesAsync();
        }

        public async Task InsertUpdateUserAddress(UserAddress userAddress)
        {
            if (userAddress != null)
            {
                Guid currentLogin = userHelper.GetCurrentUserGuidLogin();
                // get user address
                UserAddress uAdd = DbContext.UserAddresses.FirstOrDefault(ua => ua.Id == userAddress.Id);
                if (uAdd == null)
                {
                    UserAddress newUAdd = new UserAddress
                    {
                        AddressName = userAddress.AddressName,
                        AddressNote = userAddress.AddressNote,
                        City = userAddress.City,
                        DoorNum = userAddress.DoorNum,
                        FloorNum = userAddress.FloorNum,
                        IsCurrentAddress = userAddress.IsCurrentAddress,
                        IsEnabled = userAddress.IsEnabled,
                        Latitude = userAddress.Latitude,
                        Longitude = userAddress.Longitude,
                        PostalCode = userAddress.PostalCode,
                        Street = userAddress.Street,
                        UserId = userAddress.UserId,
                        CountryName = userAddress.CountryName,
                        CreatedBy = currentLogin,
                        CreatedDate = DateTime.Now,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        IsEnabledBy = currentLogin,
                        LastEditedBy = currentLogin,
                        LastEditedDate = DateTime.Now,
                        LockedDateTime = null,
                        CountryAlpha3Code = userAddress.CountryAlpha3Code,
                        AreaId = userAddress.AreaId
                    };
                    DbContext.UserAddresses.Add(newUAdd);
                    await DbContext.SaveChangesAsync();
                }
                else
                {
                    uAdd.AddressName = userAddress.AddressName;
                    uAdd.AddressNote = userAddress.AddressNote;
                    uAdd.City = userAddress.City;
                    uAdd.DoorNum = userAddress.DoorNum;
                    uAdd.FloorNum = userAddress.FloorNum;
                    uAdd.IsCurrentAddress = userAddress.IsCurrentAddress;
                    uAdd.Latitude = userAddress.Latitude;
                    uAdd.Longitude = userAddress.Longitude;
                    uAdd.PostalCode = userAddress.PostalCode;
                    uAdd.Street = userAddress.Street;
                    uAdd.UserId = userAddress.UserId;
                    uAdd.CountryName = userAddress.CountryName;
                    uAdd.LastEditedBy = currentLogin;
                    uAdd.LastEditedDate = DateTime.Now;
                    uAdd.CountryAlpha3Code = userAddress.CountryAlpha3Code;
                    uAdd.AreaId = userAddress.AreaId;

                    DbContext.UserAddresses.Update(uAdd);
                    await DbContext.SaveChangesAsync();
                }
            }
        }

        public async Task InsertUserPlayBadges(IEnumerable<UserPlayBadgeViewModel> userBadges)
        {
            if (userBadges != null)
            {
                if (userBadges.Any())
                {
                    var currentLogin = userHelper.GetCurrentUserGuidLogin();
                    var dateNow = Helper.GetDateTime();
                    foreach (var userBadge in userBadges)
                    {
                        var addUserBadge = new UserPlayBadge()
                        {
                            UserId = currentLogin,
                            SportId = userBadge.SportId,
                            Level = userBadge.Level,
                            CreatedDate = dateNow,
                            CreatedBy = currentLogin
                        };
                        DbContext.UserPlayBadges.Add(addUserBadge);
                    }
                    await DbContext.SaveChangesAsync();
                }
            }
        }

        public async Task InsertUserTrainBadges(IEnumerable<UserTrainBadgeViewModel> userBadges)
        {
            if (userBadges != null)
            {
                if (userBadges.Any())
                {
                    var currentLogin = userHelper.GetCurrentUserGuidLogin();
                    var dateNow = Helper.GetDateTime();
                    foreach (var userBadge in userBadges)
                    {
                        var addUserBadge = new UserTrainBadge()
                        {
                            UserId = currentLogin,
                            SpecialtyId = userBadge.SpecialtyId,
                            CreatedDate = dateNow,
                            CreatedBy = currentLogin
                        };
                        DbContext.UserTrainBadges.Add(addUserBadge);
                    }
                    await DbContext.SaveChangesAsync();
                }
            }
        }

        public async Task InsertUserGoals(IEnumerable<UserGoal> goals,
            Guid userId)
        {
            if (goals != null)
            {
                if (goals.Any())
                {
                    var dateNow = Helper.GetDateTime();
                    foreach (var goal in goals)
                    {
                        var userGoals = new UserGoal()
                        {
                            UserGoalId = Guid.NewGuid(),
                            UserId = userId,
                            GoalId = goal.GoalId,
                            CreatedDate = dateNow,
                            CreatedBy = userId
                        };
                        DbContext.UserGoals.Add(userGoals);
                    }
                    await DbContext.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<UserPlayBadge>> GetUserPlayBadges()
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            return await DbContext.UserPlayBadges
                .Where(x => x.UserId == currentLogin)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserTrainBadge>> GetUserTrainBadges()
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            return await DbContext.UserTrainBadges
                .Where(x => x.UserId == currentLogin)
                .ToListAsync();
        }
        public async Task<IEnumerable<UserGoal>> GetUserGoals(Guid userId)
        {
            return await DbContext.UserGoals
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserPlayBadgeViewModel>> GetPlayBadgesWithIcon(Guid userId)
        {
            return await (from badges in DbContext.UserPlayBadges
                          join sports in DbContext.Sports
                            on badges.SportId equals sports.SportId
                          where badges.UserId == userId
                          select new UserPlayBadgeViewModel()
                          {
                              Sport = sports.Name,
                              SportId = sports.SportId,
                              Level = badges.Level,
                              Icon = sports.Icon
                          })
                          .ToListAsync();
        }

        public async Task<IEnumerable<UserTrainBadgeViewModel>> GetTrainBadgesWithIcon(Guid userId)
        {
            return await (from badges in DbContext.UserTrainBadges
                          join specialties in DbContext.Specialties
                            on badges.SpecialtyId equals specialties.SpecialtyId
                          where badges.UserId == userId
                          select new UserTrainBadgeViewModel()
                          {
                              Specialty = specialties.Name,
                              SpecialtyId = specialties.SpecialtyId,
                              Icon = specialties.Icon
                          })
                          .ToListAsync();
        }

        public async Task<IEnumerable<UserFriend>> GetUserFriend(int userId)
        {
            return await DbContext.UserFriends.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<UpcomingBooking>> GetUpcomingBooking(Guid userId)
        {

            var groupActivityQuery = await (from x in DbContext.GroupBookings
                                            join y in DbContext.GroupClasses
                                              on x.GroupClassId equals y.GroupClassId
                                            join z in DbContext.Locations
                                               on y.LocationId equals z.LocationId
                                            join participant in DbContext.Users
                                               on x.ParticipantId equals participant.UserId
                                            where x.Status == EBookingStatus.Approved && x.ParticipantId == userId
                                            select new UpcomingBooking
                                            {
                                                Id = y.Id,
                                                Title = y.Title,
                                                Date = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy"),
                                                Image = participant.ImageUrl,
                                                Location = z.Name,
                                                Time = Convert.ToDateTime(y.Start).ToString("hh:mm tt"),
                                            }).ToListAsync();

            var groupActivityQueryNoLocation = await (from x in DbContext.GroupBookings
                                                      join y in DbContext.GroupClasses
                                                        on x.GroupClassId equals y.GroupClassId
                                                      join participant in DbContext.Users
                                                         on x.ParticipantId equals participant.UserId
                                                      where x.Status == EBookingStatus.Approved && x.ParticipantId == userId
                                                      select new UpcomingBooking
                                                      {
                                                          Id = y.Id,
                                                          Title = y.Title,
                                                          Date = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy"),
                                                          Image = participant.ImageUrl,
                                                          Location = "",
                                                          Time = Convert.ToDateTime(y.Start).ToString("hh:mm tt"),
                                                      }).ToListAsync();

            var individualActivityQuery = await (from x in DbContext.IndividualBookings
                                                 join y in DbContext.IndividualClasses
                                                   on x.ClassId equals y.ClassId
                                                 join participant in DbContext.Users
                                                    on x.TraineeId equals participant.UserId
                                                 where x.Status == EBookingStatus.Approved && x.TraineeId == userId
                                                 select new UpcomingBooking
                                                 {
                                                     Id = y.Id,
                                                     Title = "Individual Class",
                                                     Date = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy"),
                                                     Image = participant.ImageUrl,
                                                     Location = x.Location,
                                                     Time = Convert.ToDateTime(x.StartTime).ToString("hh:mm tt"),
                                                 }).ToListAsync();


            return groupActivityQuery.Union(groupActivityQueryNoLocation).Union(individualActivityQuery).ToList();
        }

        public async Task DeleteUserPlayBadges(IEnumerable<UserPlayBadge> userBadges)
        {
            if (userBadges.Any())
            {
                DbContext.UserPlayBadges.RemoveRange(userBadges);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteUserTrainBadges(IEnumerable<UserTrainBadge> userBadges)
        {
            if (userBadges.Any())
            {
                DbContext.UserTrainBadges.RemoveRange(userBadges);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteUserGoals(IEnumerable<UserGoal> goals)
        {
            if (goals.Any())
            {
                DbContext.UserGoals.RemoveRange(goals);
                await DbContext.SaveChangesAsync();
            }
        }

        public APIResponse Login(LoginCredentials _user)
        {
            APIResponse ApiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::Login --");
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if user exist
                User foundUser = DbContext.Users.Where(u => u.Email == _user.Email).FirstOrDefault();
                if (foundUser != null)
                {
                    //check if the user has no mobile number
                    if (foundUser.MobileNumber == null && foundUser.Password == foundUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key))
                    {
                        LogManager.LogError("MobileLogin > No mobile number found. " + foundUser.Email);
                        ApiResp.Message = "Unverified user account.";
                        ApiResp.Status = Status.Failed;
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                        ApiResp.Payload = foundUser.UserId;

                        return ApiResp;
                    }

                    //check if the user is verified
                    if (foundUser.IsEnabled == false && foundUser.IsLocked == true && foundUser.Password == foundUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key))
                    {
                        LogManager.LogError("MobileLogin > Unverified user account. " + foundUser.Email);
                        ApiResp.Message = "Unverified user account.";
                        ApiResp.Status = "Failed";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                        ApiResp.Payload = new
                        {
                            foundUser.UserId,
                            foundUser.MobileNumber
                        };
                        ApiResp.ResponseCode = APIResponseCode.UserIsNotYetVerified;

                        return ApiResp;
                    }
                    else
                    {
                        //check if password matches
                        LogManager.LogInfo("CheckPassword");
                        if (foundUser.Password == foundUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key))
                        {
                            CoachProfile UserCoachProfile = new CoachProfile();
                            if (foundUser.UserType == EUserType.NORMALANDCOACH)
                            {
                                UserCoachProfile = ICoachRepo.GetCoachProfile(foundUser.UserId);
                            }

                            ApiResp.Payload = new UserContext
                            {
                                UserInfo = foundUser,
                                CoachProfile = UserCoachProfile,
                                Tokens = AddLoginTransactionForUser(foundUser),
                                UnReadCount = DbContext.ChatConversations.Where(c => c.UserId == foundUser.UserId && c.IsUnRead == false).Count()
                            };

                            ApiResp.Message = "Login successful.";
                            ApiResp.Status = "Success";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                            LogManager.LogInfo("Login successful with user :" + _user.Email);
                            LogManager.LogDebugObject(foundUser);
                        }
                        else
                        {
                            ApiResp.Message = "Incorrect Password";
                            ApiResp.Status = "Failed";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            LogManager.LogError("The login details you have provided are not correct. Please try again with valid credentials.");
                        }
                    }
                }
                else
                {
                    LogManager.LogError("MobileLogin > Account not found. " + _user.Email);
                    ApiResp.Message = "Email not found.";
                    ApiResp.Status = "Failed";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::Login --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Something went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }

            return ApiResp;
        }

        public APIResponse SetFCMToken(string UserToken, string FCMToken, EDevicePlatform DeviceType)
        {
            LogManager.LogInfo("-- Run::UserRepository::SetFCMToken --");
            LogManager.LogDebugObject(UserToken);
            LogManager.LogDebugObject(FCMToken);
            LogManager.LogDebugObject(DeviceType);

            APIResponse ApiResp = new APIResponse();

            try
            {
                UserToken = UserToken.Split(' ')[1];
                var foundUserTrans = DbContext.UserLoginTransactions.
                                               OrderByDescending(ult => ult.DateCreated).
                                               FirstOrDefault(ult => ult.Token == UserToken);

                // Check if latest unexpired user login transaction exists
                LogManager.LogInfo("Check if auth token is valid: " + UserToken);
                LogManager.LogDebugObject(foundUserTrans);
                if (foundUserTrans != null && foundUserTrans.TokenExpiration > DateTime.Now)
                {
                    LogManager.LogInfo("Auth token is valid");
                    LogManager.LogInfo("Check user device fcm token");
                    var foundUserDevice = DbContext.UserDevices.FirstOrDefault(ud => ud.UserId == foundUserTrans.UserId &&
                                                                                     ud.DeviceType == DeviceType &&
                                                                                     ud.DeviceFCMToken == FCMToken);
                    LogManager.LogDebugObject(foundUserDevice);

                    // Check if user device is not yet registered
                    if (foundUserDevice == null)
                    {
                        LogManager.LogInfo("No FCM device found for user id: " + foundUserTrans.UserId + " and FCM token: " + FCMToken);
                        // If non existing, create new record
                        foundUserDevice = new UserDevice
                        {
                            UserId = foundUserTrans.UserId,
                            DeviceFCMToken = FCMToken,
                            DeviceType = DeviceType,
                            CreatedDate = DateTime.Now,
                            CreatedBy = foundUserTrans.UserId,
                            LastEditedDate = DateTime.Now,
                            LastEditedBy = foundUserTrans.UserId,
                            IsEnabled = true,
                            IsEnabledBy = foundUserTrans.UserId,
                            DateEnabled = DateTime.Now,
                            IsLocked = false
                        };
                        DbContext.Add(foundUserDevice);
                        DbContext.SaveChanges();
                        LogManager.LogInfo("Added new device : ");
                        LogManager.LogDebugObject(foundUserDevice);
                        LogManager.LogInfo("Added FCM Token Success");
                    }
                    else
                    {
                        // Re-enable token; probably user has logged out and logged in again using the same fcm
                        LogManager.LogInfo("Old fcm token found");
                        LogManager.LogDebugObject(foundUserDevice);
                        foundUserDevice.IsEnabled = true;
                        foundUserDevice.LastEditedDate = DateTime.Now;
                        foundUserDevice.LastEditedBy = foundUserTrans.UserId;
                        foundUserDevice.IsEnabledBy = foundUserTrans.UserId;
                        foundUserDevice.DateEnabled = DateTime.Now;
                        DbContext.Update(foundUserDevice);
                        DbContext.SaveChanges();
                        LogManager.LogInfo("Enabled Old fcm token");
                        LogManager.LogInfo("Update FCM Token Success");
                    }

                    ApiResp.Message = "Update FCM Token Success!";
                    ApiResp.Status = "Success";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {

                    ApiResp.Message = "User login was not available";
                    ApiResp.Status = "User Login Transaction Not Found";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (SqlException ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::SetFCMToken --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Something went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }

            return ApiResp;
        }

        public APIResponse ReGenerateTokens(UserLoginTransaction _user)
        {
            APIResponse ApiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::ReGenerateTokens --");
            LogManager.LogDebugObject(_user);

            try
            {
                LogManager.LogDebugObject(_user);
                //check if the requesting user is valid
                var foundUser = DbContext.Users.FirstOrDefault(u => u.UserId == _user.UserId);
                DateTime ExpirationDates = DateTime.Now;
                UserLoginTransaction ulTransactions = new UserLoginTransaction();
                if (foundUser != null)
                {
                    // user id is valid
                    // check if tokens are valid
                    ulTransactions = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == _user.Token
                                                                                        && ult.RefreshToken == _user.RefreshToken
                                                                                        && ult.RefreshTokenExpiration > ExpirationDates
                                                                                        && ult.UserId == _user.UserId);
                    if (ulTransactions != null)
                    {
                        // tokens are valid lets make a new one
                        UserLoginTransaction newUserLoginTrans = new UserLoginTransaction
                        {
                            UserId = foundUser.UserId,
                            Device = _user.Device,
                            UserType = _user.UserType,
                            Token = CreateJWTToken(foundUser, ExpirationDates, APIConfig),
                            TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                            RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                            RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                            IsEnabled = true,
                            DateCreated = DateTime.Now
                        };

                        DbContext.UserLoginTransactions.Add(newUserLoginTrans);
                        DbContext.SaveChanges();

                        ApiResp.Message = "Refresh token regenerated";
                        ApiResp.Status = "Success";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                        ApiResp.Payload = new UserContext
                        {
                            UserInfo = foundUser,
                            Tokens = newUserLoginTrans
                        };
                    }
                    else
                    {
                        ApiResp.Message = "Login Expired! Please login again!";
                        ApiResp.Status = "Error!";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    ApiResp.Message = "Login Expired! Please login again!";
                    ApiResp.Status = "Error!";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::ReGenerateTokens --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Something went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        public APIResponse MobileLogout(LogoutCredentials _user)
        {
            APIResponse ApiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::MobileLogout --");
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if user device exist
                var foundUserDevice = DbContext.UserDevices.FirstOrDefault(ud => ud.DeviceFCMToken == _user.DeviceFCMToken &&
                                                                                 ud.UserId == _user.UserId);

                if (foundUserDevice != null)
                {
                    foundUserDevice.IsEnabled = false;
                    foundUserDevice.LastEditedBy = _user.UserId;
                    foundUserDevice.LastEditedDate = DateTime.Now;
                    DbContext.Update(foundUserDevice);

                    var foundUserLoginTransaction = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == _user.AuthToken);
                    if (foundUserLoginTransaction != null)
                    {
                        foundUserLoginTransaction.IsEnabled = false;
                        DbContext.Update(foundUserLoginTransaction);
                    }

                    DbContext.SaveChanges();

                    ApiResp.Message = "FCM token disabled";
                    ApiResp.Status = "Logout successful!";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                }
                else
                {
                    ApiResp.Message = "User device not found";
                    ApiResp.Status = "Invalid FCM Token";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK; // it's ok if token is not found, just return ok status
                }
            }
            catch (SqlException ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::MobileLogout --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Something went wrong!";
                ApiResp.Status = "Internal Server Error";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        public UserLoginTransaction AddLoginTransactionForUser(User _user)
        {
            DateTime ExpirationDates = DateTime.Now;
            UserLoginTransaction ulT = new UserLoginTransaction();
            UserLoginTransaction retUlT = new UserLoginTransaction();
            //
            // check if there is an existing login transaction that is not yet expiered 
            // for the same platform
            //
            try
            {
                ulT = DbContext.UserLoginTransactions
                                                  .FirstOrDefault(u => u.UserId == _user.UserId &&
                                                              u.Device == _user.DeviceRegistrationPlatform &&
                                                              u.IsEnabled == true &&
                                                              u.TokenExpiration > DateTime.Now);

                if (ulT != null) // deactivate existing
                {
                    ulT.IsEnabled = false;
                    DbContext.UserLoginTransactions.Update(ulT);
                    DbContext.SaveChanges();
                }

                // create new
                retUlT = new UserLoginTransaction
                {
                    UserId = _user.UserId,
                    UserType = _user.UserType,
                    Device = _user.DeviceRegistrationPlatform,
                    Token = CreateJWTToken(_user, ExpirationDates, APIConfig),
                    TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                    RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                    RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                    CreatedDate = DateTime.Now,
                    CreatedBy = _user.UserId,
                    IsEnabled = true,
                    IsLocked = false
                };

                DbContext.UserLoginTransactions.Add(retUlT);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::AddLoginTransactionForUser --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }

            return retUlT;
        }

        public APIResponse VerifyUserCode(VerifyCode _verification, APIConfigurationManager _conf = null)
        {
            LogManager.LogInfo("-- Run::UserRepository::VerifyUserCode --");
            LogManager.LogDebugObject(_verification);

            APIResponse apiResp = new APIResponse();

            DateTime CurrentDate = DateTime.Now;
            UserVerificationCode FoundUserVerificationCode = new UserVerificationCode();

            try
            {
                if (_verification.VerificationType == EVerificationType.RegisterAccount)
                {
                    FoundUserVerificationCode = DbContext.UserVerificationCodes.AsNoTracking().Where(e => e.Email == _verification.Email
                        && e.VerificationCode == _verification.VerificationCode && e.VerificationType == EVerificationType.RegisterAccount && e.IsEnabled == true).FirstOrDefault();
                    if (FoundUserVerificationCode != null)
                    {
                        FoundUserVerificationCode.LastEditedBy = FoundUserVerificationCode.UserId;
                        FoundUserVerificationCode.LastEditedDate = CurrentDate;
                        FoundUserVerificationCode.DateEnabled = CurrentDate;
                        FoundUserVerificationCode.IsEnabledBy = FoundUserVerificationCode.UserId;
                        FoundUserVerificationCode.IsEnabled = false;

                        //activate user account
                        User FoundUser = DbContext.Users.AsNoTracking().Where(u => u.UserId == FoundUserVerificationCode.UserId).FirstOrDefault();
                        FoundUser.LastEditedBy = FoundUser.UserId;
                        FoundUser.LastEditedDate = CurrentDate;
                        FoundUser.DateEnabled = CurrentDate;
                        FoundUser.IsEnabledBy = FoundUser.UserId;
                        FoundUser.IsEnabled = true;
                        FoundUser.IsLocked = false;

                        DbContext.Users.Update(FoundUser);
                        DbContext.UserVerificationCodes.Update(FoundUserVerificationCode);
                        DbContext.SaveChanges();

                        var EmailParam = _conf.MailConfig;
                        EmailParam.To = new List<string>() { FoundUser.Email };
                        EmailParam.Subject = APIConfig.MsgConfigs.WelcomeMsgEmailSubject;
                        EmailParam.Body = String.Format(APIConfig.MsgConfigs.WelcomeMsg, FoundUser.FirstName);
                        EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager);
                        var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { FoundUser.Email }, EmailParam, LogManager);

                        return apiResp = new APIResponse
                        {
                            Message = "Thank you for joining the Sidekick community. Your registration is confirmed.",
                            Status = "Success",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Invalid verification type.",
                            Status = "Failed",
                            StatusCode = System.Net.HttpStatusCode.NotFound
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::VerifyUserCode --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse ResendVerificationCode(ResendCode _verification, APIConfigurationManager _conf = null, IMainHttpClient _mhttpc = null)
        {
            LogManager.LogInfo("-- Run::UserRepository::ResendVerificationCode --");
            LogManager.LogDebugObject(_verification);

            APIResponse apiResp = new APIResponse();

            DateTime CurrentDate = DateTime.Now;
            string NewVerificationCode = GenerateUniqueCode(4, true, false);
            LogManager.LogInfo("-- Run::GenerateUniqueCode -- [New Verification Code] -- " + NewVerificationCode);

            User FoundUser = new User();
            UserVerificationCode FoundUserVerificationCode = new UserVerificationCode();

            try
            {
                if (_verification.VerificationType == EVerificationType.RegisterAccount)
                {
                    FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _verification.Email);
                    if (FoundUser != null)
                    {
                        var EmailParam = _conf.MailConfig;
                        EmailParam.To = new List<string>() { FoundUser.Email };
                        EmailParam.Subject = APIConfig.MsgConfigs.ResendCodeUserEmailSubject;
                        EmailParam.Body = String.Format(APIConfig.MsgConfigs.ResendCode, FoundUser.FirstName, NewVerificationCode);
                        // read email template here..

                        EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager);

                        var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { FoundUser.Email }, EmailParam, LogManager);

                        if (_conf.SmsConfig.IsEnable)
                        {
                            SmsParameter smsParameter = new SmsParameter();
                            smsParameter.To = FoundUser.MobileNumber;
                            smsParameter.Text = String.Format(APIConfig.SMSTemplateConfig.OTPForRegistration, NewVerificationCode);
                            smsParameter.Action = APIConfig.SmsConfig.Action;
                            smsParameter.From = APIConfig.SmsConfig.From;
                            smsParameter.User = APIConfig.SmsConfig.User;
                            smsParameter.Password = APIConfig.SmsConfig.Password;
                            string smsstatus = SendSmsAsync(_mhttpc, smsParameter).GetAwaiter().GetResult();
                        }


                        if (sendStatus == 0)
                        {
                            SaveVerificationCode(FoundUser.UserId, FoundUser.Email, FoundUser.MobileNumber, EVerificationType.RegisterAccount, NewVerificationCode);

                            return apiResp = new APIResponse
                            {
                                Message = "We have sent you a new registration code vis SMS.",
                                Status = "Success",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            return apiResp = new APIResponse
                            {
                                Message = "Sending failed.",
                                Status = "Failed",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                    else
                    {
                        apiResp.Message = "No record found.";
                        apiResp.Status = "Failed";
                        apiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Invalid verification type.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::ResendVerificationCode --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        void SaveVerificationCode(Guid _userId, string _email, string _mobileNumber, EVerificationType _vType, string _vCode)
        {
            LogManager.LogInfo("-- Run::SaveVerificationCode -- FOR -- " + _userId + " >> " + _vType);
            DateTime CurrentDate = DateTime.Now;

            UserVerificationCode FoundUserVerificationCode = new();
            FoundUserVerificationCode = DbContext.UserVerificationCodes.AsNoTracking()
                              .FirstOrDefault(u => u.UserId == _userId && u.IsEnabled == true && u.VerificationType == _vType);

            if (FoundUserVerificationCode != null)
            {
                FoundUserVerificationCode.VerificationCode = _vCode;
                FoundUserVerificationCode.LastEditedDate = CurrentDate;
                FoundUserVerificationCode.LastEditedBy = FoundUserVerificationCode.UserId;

                DbContext.UserVerificationCodes.Update(FoundUserVerificationCode);
                DbContext.SaveChanges();
            }
            else
            {
                UserVerificationCode NewUserVerificationCode = new()
                {
                    CreatedBy = _userId,
                    CreatedDate = CurrentDate,
                    IsEnabledBy = _userId,
                    DateEnabled = CurrentDate,
                    IsEnabled = true,

                    UserId = _userId,
                    Email = _email,
                    MobileNumber = _mobileNumber,
                    VerificationType = _vType,
                    VerificationCode = _vCode
                };

                DbContext.UserVerificationCodes.Add(NewUserVerificationCode);
                DbContext.SaveChanges();
            }
        }

        public APIResponse ForgotUserPassword(UserForgotPassword _user, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::ForgotUserPassword --");
            LogManager.LogDebugObject(_user);

            try
            {
                User FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _user.Email);
                if (FoundUser != null)
                {
                    var sysGenPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 6).ToLower();

                    var EmailParam = _conf.MailConfig;
                    EmailParam.To = new List<string>() { FoundUser.Email };
                    EmailParam.Subject = APIConfig.MsgConfigs.ForgotPasswordEmailSubject;
                    EmailParam.Body = String.Format(APIConfig.MsgConfigs.ForgotPassword, FoundUser.FirstName, sysGenPassword);
                    EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, LogManager);
                    var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { FoundUser.Email }, EmailParam, LogManager);

                    if (sendStatus == 0)
                    {
                        FoundUser.Password = FoundUser.HashP(sysGenPassword.ToString(), APIConfig.TokenKeys.Key);
                        FoundUser.LastEditedBy = FoundUser.UserId;
                        FoundUser.LastEditedDate = DateTime.Now;

                        DbContext.Users.Update(FoundUser);
                        DbContext.SaveChanges();

                        return apiResp = new APIResponse
                        {
                            Message = "You will receive an email containing your new password.",
                            Status = "Success",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        return apiResp = new APIResponse
                        {
                            Message = "Sending failed.",
                            Status = "Failed",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
                else
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Email not found.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::ForgotUserPassword --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse ChangeUserPassword(UserChangePassword _user)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::ChangeUserPassword --");
            LogManager.LogDebugObject(_user);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.UserId == _user.UserId && ult.IsEnabled == true);
                if (IsUserLoggedIn == null)
                {
                    return apiResp = new APIResponse
                    {
                        Message = "Unathorized Access.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }

                User FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == IsUserLoggedIn.UserId);

                string HashedCurrPass = FoundUser.HashP(_user.CurrentPassword.ToString(), APIConfig.TokenKeys.Key);
                string HashedNewPass = FoundUser.HashP(_user.NewPassword.ToString(), APIConfig.TokenKeys.Key);

                if (FoundUser.Password == HashedCurrPass)
                {
                    FoundUser.Password = HashedNewPass;
                    FoundUser.LastEditedBy = FoundUser.UserId;
                    FoundUser.LastEditedDate = DateTime.Now;
                    FoundUser.IsLocked = false;
                    FoundUser.IsEnabled = true;

                    DbContext.Users.Update(FoundUser);
                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "Password changed.",
                        Status = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    apiResp = new APIResponse
                    {
                        Message = "Incorrect current password.",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::ChangeUserPassword --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse UpdateUserStatus(string auth, int id)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::UpdateUserStatus --");
            LogManager.LogDebugObject(auth);
            LogManager.LogDebugObject(id);

            try
            {
                //get user login creds
                AdminLoginTransaction aLT = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (aLT != null)
                {
                    LogManager.LogInfo("Valid Auth token, proceeding to UpdateUserStatus userid: " + id.ToString());
                    // get user profile
                    User userToUpdateStatus = DbContext.Users.FirstOrDefault(u => u.Id == id);
                    if (userToUpdateStatus != null)
                    {
                        string newStatus = userToUpdateStatus.IsEnabled != true ? "Inactive" : "Active";
                        userToUpdateStatus.IsEnabled = !userToUpdateStatus.IsEnabled;
                        userToUpdateStatus.LastEditedBy = aLT.AdminId;
                        userToUpdateStatus.LastEditedDate = DateTime.Now;
                        DbContext.Update(userToUpdateStatus);
                        DbContext.SaveChanges();

                        LogManager.LogInfo("User: " + userToUpdateStatus.UserId.ToString() + " set to status " + newStatus + " by admin " + aLT.AdminId.ToString());
                        apiResp.Message = userToUpdateStatus.FirstName + " " + userToUpdateStatus.LastName + " set to " + newStatus + " status!";
                        apiResp.Status = "Success!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else  // no user found
                    {
                        LogManager.LogError("Invalid user id : " + id.ToString() + "! by admin " + aLT.AdminId.ToString());
                        apiResp.Message = "Invalid User(" + id.ToString() + ")! cannot update user status!";
                        apiResp.Status = "Failed!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    LogManager.LogError("Invalid Auth token : " + auth);
                    apiResp.Message = "Invalid Credentials please login again!";
                    apiResp.Status = "Failed!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::UpdateUserStatus --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;

        }

        public APIResponse GetCountries()
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::GetCountries --");

            try
            {
                var helper = new CountryHelper();
                var data = helper.GetCountryData();
                List<CountryList> countryList = new List<CountryList>();
                var region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .Select(t => new RegionInfo(t.LCID)).Distinct().OrderBy(a => a.EnglishName).ToList();
                Dictionary<string, string> TelePhoneDictionary = getTelePhoneCode();
                foreach (var x in region)
                {
                    CountryList cntry = new CountryList();
                    cntry.Name = x.EnglishName;
                    cntry.Alpha2Code = x.TwoLetterISORegionName;
                    cntry.Alpha3Code = x.ThreeLetterISORegionName;
                    cntry.TelephoneCode = TelePhoneDictionary.Where(d => d.Key == cntry.Alpha2Code).Select(d => d.Value).FirstOrDefault();
                    countryList.Add(cntry);
                }
                if (countryList.Count() > 0)
                {
                    apiResp = new APIResponse
                    {
                        Message = "Country List retrieved successfully",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Status = "Success!",
                        Payload = countryList
                    };
                }
                else
                {
                    apiResp = new APIResponse
                    {
                        Message = "Failed to retrieve country list.",
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Status = "Failed!",
                        Payload = countryList
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetCountries --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public static Dictionary<string, string> getTelePhoneCode()
        {
            var dictionary = new Dictionary<string, string>();

            dictionary.Add("AC", "+247");
            dictionary.Add("AD", "+376");
            dictionary.Add("AE", "+971");
            dictionary.Add("AF", "+93");
            dictionary.Add("AG", "+1-268");
            dictionary.Add("AI", "+1-264");
            dictionary.Add("AL", "+355");
            dictionary.Add("AM", "+374");
            dictionary.Add("AN", "+599");
            dictionary.Add("AO", "+244");
            dictionary.Add("AR", "+54");
            dictionary.Add("AS", "+1-684");
            dictionary.Add("AT", "+43");
            dictionary.Add("AU", "+61");
            dictionary.Add("AW", "+297");
            dictionary.Add("AX", "+358-18");
            dictionary.Add("AZ", "+994"); // or +374-97
            dictionary.Add("BA", "+387");
            dictionary.Add("BB", "+1-246");
            dictionary.Add("BD", "+880");
            dictionary.Add("BE", "+32");
            dictionary.Add("BF", "+226");
            dictionary.Add("BG", "+359");
            dictionary.Add("BH", "+973");
            dictionary.Add("BI", "+257");
            dictionary.Add("BJ", "+229");
            dictionary.Add("BM", "+1-441");
            dictionary.Add("BN", "+673");
            dictionary.Add("BO", "+591");
            dictionary.Add("BR", "+55");
            dictionary.Add("BS", "+1-242");
            dictionary.Add("BT", "+975");
            dictionary.Add("BW", "+267");
            dictionary.Add("BY", "+375");
            dictionary.Add("BZ", "+501");
            dictionary.Add("CA", "+1");
            dictionary.Add("CC", "+61");
            dictionary.Add("CD", "+243");
            dictionary.Add("CF", "+236");
            dictionary.Add("CG", "+242");
            dictionary.Add("CH", "+41");
            dictionary.Add("CI", "+225");
            dictionary.Add("CK", "+682");
            dictionary.Add("CL", "+56");
            dictionary.Add("CM", "+237");
            dictionary.Add("CN", "+86");
            dictionary.Add("CO", "+57");
            dictionary.Add("CR", "+506");
            dictionary.Add("CS", "+381");
            dictionary.Add("CU", "+53");
            dictionary.Add("CV", "+238");
            dictionary.Add("CX", "+61");
            dictionary.Add("CY", "+357"); // or +90-392
            dictionary.Add("CZ", "+420");
            dictionary.Add("DE", "+49");
            dictionary.Add("DJ", "+253");
            dictionary.Add("DK", "+45");
            dictionary.Add("DM", "+1-767");
            dictionary.Add("DO", "+1-809"); // and 1-829?
            dictionary.Add("DZ", "+213");
            dictionary.Add("EC", "+593");
            dictionary.Add("EE", "+372");
            dictionary.Add("EG", "+20");
            dictionary.Add("EH", "+212");
            dictionary.Add("ER", "+291");
            dictionary.Add("ES", "+34");
            dictionary.Add("ET", "+251");
            dictionary.Add("FI", "+358");
            dictionary.Add("FJ", "+679");
            dictionary.Add("FK", "+500");
            dictionary.Add("FM", "+691");
            dictionary.Add("FO", "+298");
            dictionary.Add("FR", "+33");
            dictionary.Add("GA", "+241");
            dictionary.Add("GB", "+44");
            dictionary.Add("GD", "+1-473");
            dictionary.Add("GE", "+995");
            dictionary.Add("GF", "+594");
            dictionary.Add("GG", "+44");
            dictionary.Add("GH", "+233");
            dictionary.Add("GI", "+350");
            dictionary.Add("GL", "+299");
            dictionary.Add("GM", "+220");
            dictionary.Add("GN", "+224");
            dictionary.Add("GP", "+590");
            dictionary.Add("GQ", "+240");
            dictionary.Add("GR", "+30");
            dictionary.Add("GT", "+502");
            dictionary.Add("GU", "+1-671");
            dictionary.Add("GW", "+245");
            dictionary.Add("GY", "+592");
            dictionary.Add("HK", "+852");
            dictionary.Add("HN", "+504");
            dictionary.Add("HR", "+385");
            dictionary.Add("HT", "+509");
            dictionary.Add("HU", "+36");
            dictionary.Add("ID", "+62");
            dictionary.Add("IE", "+353");
            dictionary.Add("IL", "+972");
            dictionary.Add("IM", "+44");
            dictionary.Add("IN", "+91");
            dictionary.Add("IO", "+246");
            dictionary.Add("IQ", "+964");
            dictionary.Add("IR", "+98");
            dictionary.Add("IS", "+354");
            dictionary.Add("IT", "+39");
            dictionary.Add("JE", "+44");
            dictionary.Add("JM", "+1-876");
            dictionary.Add("JO", "+962");
            dictionary.Add("JP", "+81");
            dictionary.Add("KE", "+254");
            dictionary.Add("KG", "+996");
            dictionary.Add("KH", "+855");
            dictionary.Add("KI", "+686");
            dictionary.Add("KM", "+269");
            dictionary.Add("KN", "+1-869");
            dictionary.Add("KP", "+850");
            dictionary.Add("KR", "+82");
            dictionary.Add("KW", "+965");
            dictionary.Add("KY", "+1-345");
            dictionary.Add("KZ", "+7");
            dictionary.Add("LA", "+856");
            dictionary.Add("LB", "+961");
            dictionary.Add("LC", "+1-758");
            dictionary.Add("LI", "+423");
            dictionary.Add("LK", "+94");
            dictionary.Add("LR", "+231");
            dictionary.Add("LS", "+266");
            dictionary.Add("LT", "+370");
            dictionary.Add("LU", "+352");
            dictionary.Add("LV", "+371");
            dictionary.Add("LY", "+218");
            dictionary.Add("MA", "+212");
            dictionary.Add("MC", "+377");
            dictionary.Add("MD", "+373"); // or +373-533
            dictionary.Add("ME", "+382");
            dictionary.Add("MG", "+261");
            dictionary.Add("MH", "+692");
            dictionary.Add("MK", "+389");
            dictionary.Add("ML", "+223");
            dictionary.Add("MM", "+95");
            dictionary.Add("MN", "+976");
            dictionary.Add("MO", "+853");
            dictionary.Add("MP", "+1-670");
            dictionary.Add("MQ", "+596");
            dictionary.Add("MR", "+222");
            dictionary.Add("MS", "+1-664");
            dictionary.Add("MT", "+356");
            dictionary.Add("MU", "+230");
            dictionary.Add("MV", "+960");
            dictionary.Add("MW", "+265");
            dictionary.Add("MX", "+52");
            dictionary.Add("MY", "+60");
            dictionary.Add("MZ", "+258");
            dictionary.Add("NA", "+264");
            dictionary.Add("NC", "+687");
            dictionary.Add("NE", "+227");
            dictionary.Add("NF", "+672");
            dictionary.Add("NG", "+234");
            dictionary.Add("NI", "+505");
            dictionary.Add("NL", "+31");
            dictionary.Add("NO", "+47");
            dictionary.Add("NP", "+977");
            dictionary.Add("NR", "+674");
            dictionary.Add("NU", "+683");
            dictionary.Add("NZ", "+64");
            dictionary.Add("OM", "+968");
            dictionary.Add("PA", "+507");
            dictionary.Add("PE", "+51");
            dictionary.Add("PF", "+689");
            dictionary.Add("PG", "+675");
            dictionary.Add("PH", "+63");
            dictionary.Add("PK", "+92");
            dictionary.Add("PL", "+48");
            dictionary.Add("PM", "+508");
            dictionary.Add("PR", "+1-787"); // and 1-939 ?
            dictionary.Add("PS", "+970");
            dictionary.Add("PT", "+351");
            dictionary.Add("PW", "+680");
            dictionary.Add("PY", "+595");
            dictionary.Add("QA", "+974");
            dictionary.Add("RE", "+262");
            dictionary.Add("RO", "+40");
            dictionary.Add("RS", "+381");
            dictionary.Add("RU", "+7");
            dictionary.Add("RW", "+250");
            dictionary.Add("SA", "+966");
            dictionary.Add("SB", "+677");
            dictionary.Add("SC", "+248");
            dictionary.Add("SD", "+249");
            dictionary.Add("SE", "+46");
            dictionary.Add("SG", "+65");
            dictionary.Add("SH", "+290");
            dictionary.Add("SI", "+386");
            dictionary.Add("SJ", "+47");
            dictionary.Add("SK", "+421");
            dictionary.Add("SL", "+232");
            dictionary.Add("SM", "+378");
            dictionary.Add("SN", "+221");
            dictionary.Add("SO", "+252");
            dictionary.Add("SR", "+597");
            dictionary.Add("ST", "+239");
            dictionary.Add("SV", "+503");
            dictionary.Add("SY", "+963");
            dictionary.Add("SZ", "+268");
            dictionary.Add("TA", "+290");
            dictionary.Add("TC", "+1-649");
            dictionary.Add("TD", "+235");
            dictionary.Add("TG", "+228");
            dictionary.Add("TH", "+66");
            dictionary.Add("TJ", "+992");
            dictionary.Add("TK", "+690");
            dictionary.Add("TL", "+670");
            dictionary.Add("TM", "+993");
            dictionary.Add("TN", "+216");
            dictionary.Add("TO", "+676");
            dictionary.Add("TR", "+90");
            dictionary.Add("TT", "+1-868");
            dictionary.Add("TV", "+688");
            dictionary.Add("TW", "+886");
            dictionary.Add("TZ", "+255");
            dictionary.Add("UA", "+380");
            dictionary.Add("UG", "+256");
            dictionary.Add("US", "+1");
            dictionary.Add("UY", "+598");
            dictionary.Add("UZ", "+998");
            dictionary.Add("VA", "+379");
            dictionary.Add("VC", "+1-784");
            dictionary.Add("VE", "+58");
            dictionary.Add("VG", "+1-284");
            dictionary.Add("VI", "+1-340");
            dictionary.Add("VN", "+84");
            dictionary.Add("VU", "+678");
            dictionary.Add("WF", "+681");
            dictionary.Add("WS", "+685");
            dictionary.Add("YE", "+967");
            dictionary.Add("YT", "+262");
            dictionary.Add("ZA", "+27");
            dictionary.Add("ZM", "+260");
            dictionary.Add("ZW", "+263");

            return dictionary;
        }

        public async Task<User> GetUser(Guid userId)
        {
            return await DbContext.Users
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserAddress> GetUserAddress(Guid userId)
        {
            return await DbContext.UserAddresses.Where(x => x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedViewModel<UserFriendViewModel>> GetFriends(PagedRequestViewModel search)
        {
            var currentLogin = userHelper.GetCurrentUserIdLogin();
            var list = new PagedViewModel<UserFriendViewModel>();
            var data = (from friends in DbContext.UserFriends
                        where friends.UserId == currentLogin
                        select new UserFriendViewModel
                        {
                            UserId = friends.Fk_FriendUserId.UserId,
                            FirstName = friends.Fk_FriendUserId.FirstName,
                            LastName = friends.Fk_FriendUserId.LastName,
                            ImageUrl = friends.Fk_FriendUserId.ImageUrl
                        })
                    .OrderByField(search.SortColumn, search.IsAscending);
            await Pagination<UserFriendViewModel>.Data(search.PageSize, search.PageNumber, list, data);
            return list;
        }

        public async Task<PagedViewModel<UserFriendViewModel>> GetFriendRequests(PagedRequestViewModel search)
        {
            var list = new PagedViewModel<UserFriendViewModel>();
            var currentLogin = userHelper.GetCurrentUserIdLogin();
            var data = (from friendRequests in DbContext.UserFriendRequests
                        join userLocation in DbContext.UserAddresses
                        on friendRequests.Fk_FriendRequestUserId.UserId equals userLocation.UserId
                        into userLocationTemp
                        from location in userLocationTemp.DefaultIfEmpty()
                        where friendRequests.UserId == currentLogin
                        select new UserFriendViewModel
                        {
                            UserId = friendRequests.Fk_FriendRequestUserId.UserId,
                            FirstName = friendRequests.Fk_FriendRequestUserId.FirstName,
                            LastName = friendRequests.Fk_FriendRequestUserId.LastName,
                            ImageUrl = friendRequests.Fk_FriendRequestUserId.ImageUrl,
                            Location = location.City
                        })
                           .OrderByField(search.SortColumn, search.IsAscending);
            await Pagination<UserFriendViewModel>.Data(search.PageSize, search.PageNumber, list, data);
            return list;
        }

        public async Task<UserFriendViewModel> GetFriendRequest(Guid friendUserId)
        {
            var currentUser = userHelper.GetCurrentUserGuidLogin();
            var requestUser = await DbContext.UserFriendRequests.Include(u => u.Fk_FriendRequestUserId)
                                                                .Include(u => u.Fk_UserId)
                                                                .FirstOrDefaultAsync(u => u.Fk_FriendRequestUserId.UserId == friendUserId
                                                                && u.Fk_UserId.UserId == currentUser);
            var description = DbContext.Users.Where(u => u.UserId == currentUser).FirstOrDefault();
            var location = DbContext.UserAddresses.Where(u => u.UserId == currentUser).FirstOrDefault();
            var userPlayBadges = await GetPlayBadgesWithIcon(friendUserId);
            var userTrainBadges = await GetTrainBadgesWithIcon(friendUserId);
            return new UserFriendViewModel
            {
                FirstName = requestUser.Fk_FriendRequestUserId.FirstName,
                LastName = requestUser.Fk_FriendRequestUserId.LastName,
                ImageUrl = requestUser.Fk_FriendRequestUserId.ImageUrl,
                UserId = requestUser.Fk_FriendRequestUserId.UserId,
                Location = location != null ? location.City : string.Empty,
                TrainBadges = userTrainBadges,
                PlayBadges = userPlayBadges,
                Description = description != null ? description.Description : string.Empty
            };
        }

        public async Task<bool> AcceptFriendRequest(Guid friendUserId)
        {
            var currentUser = userHelper.GetCurrentUserGuidLogin();
            var getCurrentUser = await GetUser(currentUser);
            var getFriendUser = await GetUser(friendUserId);

            var requestExisting = await DbContext.UserFriendRequests.Include(u => u.Fk_FriendRequestUserId)
                                                                      .Include(u => u.Fk_UserId)
                                                                      .FirstOrDefaultAsync(x => x.Fk_FriendRequestUserId.UserId == friendUserId && x.Fk_UserId.UserId == currentUser);
            if (requestExisting != null)
            {
                var newfriends = new List<UserFriend>
                {
                    new UserFriend
                    {
                        Fk_FriendUserId = getFriendUser,
                        Fk_UserId = getCurrentUser,

                        LastEditedBy = currentUser,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsBlockedUser = false,
                        BlockedUserBy = Guid.Empty,
                        IsEnabledBy = currentUser,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                    },
                    new UserFriend
                    {
                        Fk_FriendUserId = getCurrentUser,
                        Fk_UserId = getFriendUser,

                        LastEditedBy = currentUser,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsBlockedUser = false,
                        BlockedUserBy = Guid.Empty,
                        IsEnabledBy = currentUser,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                    }
                };
                DbContext.UserFriends.AddRange(newfriends);

                DbContext.UserFriendRequests.Remove(requestExisting);
                await DbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DeclineFriendRequest(Guid userFriendRequestId)
        {
            var currentUser = userHelper.GetCurrentUserGuidLogin();
            var userRequest = await DbContext.UserFriendRequests.Include(u => u.Fk_FriendRequestUserId)
                                                                .Include(u => u.Fk_UserId)
                                                                .FirstOrDefaultAsync(u => u.Fk_FriendRequestUserId.UserId == userFriendRequestId
                                                                && u.Fk_UserId.UserId == currentUser);
            if (userRequest != null)
            {
                DbContext.UserFriendRequests.Remove(userRequest);
                await DbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<UserFriendViewModel>> GetAllFriendRequest(string filter)
        {
            var currentUser = userHelper.GetCurrentUserGuidLogin();
            var blockedUsers = await DbContext.BlockedUsers.Include(b => b.Fk_UserId)
                                                           .Include(b => b.Fk_BlockedUserId)
                                                          .ToListAsync();
            var friendRequests = new List<UserFriendViewModel>();

            if (DbContext.UserFriendRequests.Count() > 0)
            {
                if (filter != null)
                {
                    friendRequests = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                        .Include(u => u.Fk_FriendRequestUserId)
                                                        .Where(u => u.Fk_UserId.UserId == currentUser && u.IsBlockedUser == false)
                                                        .Select(u => new UserFriendViewModel
                                                        {
                                                            UserId = u.Fk_FriendRequestUserId.UserId,
                                                            FirstName = u.Fk_FriendRequestUserId.FirstName,
                                                            LastName = u.Fk_FriendRequestUserId.LastName,
                                                            ImageUrl = u.Fk_FriendRequestUserId.ImageUrl,
                                                            Location = DbContext.UserAddresses.FirstOrDefault(u => u.UserId == currentUser).City
                                                        }).Where(x => x.FirstName.Contains(filter) || x.LastName.Contains(filter)).ToListAsync();
                }
                else
                {
                    friendRequests = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                        .Include(u => u.Fk_FriendRequestUserId)
                                                        .Where(u => u.Fk_UserId.UserId == currentUser && u.IsBlockedUser == false)
                                                        .Select(u => new UserFriendViewModel
                                                        {
                                                            UserId = u.Fk_FriendRequestUserId.UserId,
                                                            FirstName = u.Fk_FriendRequestUserId.FirstName,
                                                            LastName = u.Fk_FriendRequestUserId.LastName,
                                                            ImageUrl = u.Fk_FriendRequestUserId.ImageUrl,
                                                            Location = DbContext.UserAddresses.FirstOrDefault(u => u.UserId == currentUser).City
                                                        }).ToListAsync();
                }

            }
            var filteredUsers = new List<UserFriendViewModel>();
            if (blockedUsers.Any())
            {
                foreach (var user in friendRequests)
                {
                    var blockedUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == currentUser && user.UserId == b.Fk_BlockedUserId.UserId).FirstOrDefault();
                    if (blockedUser == null)
                    {
                        var blockedFromUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == currentUser && user.UserId == b.Fk_UserId.UserId).FirstOrDefault();
                        if (blockedFromUser == null)
                        {
                            filteredUsers.Add(user);
                        }
                    }
                }

                return filteredUsers;
            }

            return friendRequests;
        }

        public async Task<bool> AddToFriendRequest(Guid friendUserId)
        {
            var currentUser = await GetUser(userHelper.GetCurrentUserGuidLogin());
            var getFriendRequestUser = await GetUser(friendUserId);
            var existingFriend = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                            .Include(u => u.Fk_FriendUserId)
                                                            .Where(u => u.Fk_FriendUserId.UserId == friendUserId && u.Fk_UserId.UserId == currentUser.UserId)
                                                            .ToListAsync();

            var existingFriendRequest = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                                          .Include(u => u.Fk_FriendRequestUserId)
                                                                          .Where(u => u.Fk_FriendRequestUserId.UserId == currentUser.UserId && u.Fk_UserId.UserId == friendUserId)
                                                                          .ToListAsync();
            if (!existingFriend.Any() && !existingFriendRequest.Any())
            {
                var friendReq = new UserFriendRequest
                {
                    Fk_UserId = getFriendRequestUser,
                    Fk_FriendRequestUserId = currentUser,

                    LastEditedBy = currentUser.UserId,
                    LastEditedDate = DateTime.Now,
                    CreatedBy = currentUser.UserId,
                    CreatedDate = DateTime.Now,
                    IsEnabled = true,
                    IsEnabledBy = currentUser.UserId,
                    DateEnabled = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now,
                };

                DbContext.UserFriendRequests.Add(friendReq);
                await DbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<UserFriendViewModel> GetFriendDetail(Guid friendUserId)
        {
            var friendDetail = await DbContext.Users.Where(u => u.UserId == friendUserId).FirstOrDefaultAsync();
            var location = DbContext.UserAddresses.Where(u => u.UserId == friendUserId).FirstOrDefault();
            var userPlayBadges = await GetPlayBadgesWithIcon(friendUserId);
            var userTrainBadges = await GetTrainBadgesWithIcon(friendUserId);

            return new UserFriendViewModel
            {
                FirstName = friendDetail.FirstName,
                LastName = friendDetail.LastName,
                Description = friendDetail.Description,
                ImageUrl = friendDetail.ImageUrl,
                Location = location != null ? location.City : string.Empty,
                TrainBadges = userTrainBadges,
                PlayBadges = userPlayBadges,
                UserId = friendUserId,
                UserChatId = friendDetail.Id
            };
        }

        public async Task<bool> ReportFriend(string reason, Guid userId)
        {
            var currentUser = userHelper.GetCurrentUserGuidLogin();

            if (userId != Guid.Empty)
            {
                var newReport = new Report
                {
                    Reason = reason,
                    ReportedByUser = currentUser,
                    ReportedUser = userId,
                    Status = Model.Enums.RequestStatus.New,

                    LastEditedBy = currentUser,
                    LastEditedDate = DateTime.Now,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.Now,
                    IsEnabled = true,
                    IsEnabledBy = currentUser,
                    DateEnabled = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now,

                };

                DbContext.Reports.Add(newReport);
                await DbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Report>> GetReports(Guid friendUserId)
        {
            return await DbContext.Reports.Where(r => r.ReportedUser == friendUserId).ToListAsync();
        }

        public async Task<APIResponse> UpdateReport(string auth, ReportDto report)
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::UpdateReport --");
            LogManager.LogDebugObject(report);
            LogManager.LogDebugObject(auth);
            try
            {
                var isLoggedIn = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (isLoggedIn != null)
                {
                    var existingReport = await DbContext.Reports.Where(r => r.ReportedByUser == report.ReportedByUserId && r.ReportedUser == report.ReportedUserId).FirstOrDefaultAsync();
                    if (existingReport != null)
                    {
                        existingReport.Reason = report.Reasons;
                        existingReport.Status = report.Status;

                        existingReport.LastEditedBy = isLoggedIn.AdminId;
                        existingReport.LastEditedDate = DateTime.Now;

                        DbContext.Reports.Update(existingReport);
                        await DbContext.SaveChangesAsync();

                        return new APIResponse
                        {
                            Message = "Successfully Updated Report",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }

                    return new APIResponse
                    {
                        Message = "There is no existing Report",
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    };
                }

                return new APIResponse
                {
                    Message = "Unauthorized!",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::UpdateReport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> UpdateUser(string auth, ReportDto report)
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::UpdateReport --");
            LogManager.LogDebugObject(report);
            LogManager.LogDebugObject(auth);
            try
            {
                var isLoggedIn = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (isLoggedIn != null)
                {
                    var existingReport = await DbContext.Requests.Where(r => r.Id == report.Id).FirstOrDefaultAsync();
                    if (existingReport != null)
                    {
                        existingReport.Description = report.Reasons;
                        existingReport.Status = report.Status;

                        existingReport.LastEditedBy = isLoggedIn.AdminId;
                        existingReport.LastEditedDate = DateTime.Now;

                        DbContext.Requests.Update(existingReport);
                        await DbContext.SaveChangesAsync();

                        return new APIResponse
                        {
                            Message = "Successfully Updated User Request",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }

                    return new APIResponse
                    {
                        Message = "There is no existing User Request",
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    };
                }

                return new APIResponse
                {
                    Message = "Unauthorized!",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::UpdateReport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> DeleteReport(string auth, ReportDto report)
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::DeleteReport --");
            LogManager.LogDebugObject(report);
            LogManager.LogDebugObject(auth);
            try
            {
                var isLoggedIn = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (isLoggedIn != null)
                {
                    var existingReport = await DbContext.Reports.Where(r => r.ReportedByUser == report.ReportedByUserId && r.ReportedUser == report.ReportedUserId).FirstOrDefaultAsync();
                    if (existingReport != null)
                    {
                        DbContext.Reports.Remove(existingReport);
                        await DbContext.SaveChangesAsync();

                        return new APIResponse
                        {
                            Message = "Successfully Deleted Report",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }

                    return new APIResponse
                    {
                        Message = "There is no existing Report",
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    };
                }

                return new APIResponse
                {
                    Message = "Unauthorized!",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::DeleteReport --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetAllReports()
        {
            var apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::GetAllReports --");
            try
            {
                var users = await DbContext.Users.ToListAsync();
                var reports = await DbContext.Reports.Where(r => r.IsEnabled == true)
                                            .Select(r => new ReportDto
                                            {
                                                ReportedByUserId = r.ReportedByUser,
                                                ReportedUserId = r.ReportedUser,
                                                Reasons = r.Reason,
                                                ReportedDate = r.CreatedDate.Value,
                                                Status = r.Status
                                            }).ToListAsync();
                foreach (var report in reports)
                {
                    var reportedbyUser = users.Where(u => u.UserId == report.ReportedByUserId).FirstOrDefault();
                    report.ReportedByUser = reportedbyUser != null ? $"{ reportedbyUser.FirstName} {reportedbyUser.LastName}" : string.Empty;
                    var reportedUser = users.Where(u => u.UserId == report.ReportedUserId).FirstOrDefault();
                    report.ReportedUser = reportedUser != null ? $"{ reportedUser.FirstName} {reportedUser.LastName}" : string.Empty;
                }

                return new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = reports,
                    Message = "Success!"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetAllReports --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<IEnumerable<BlockViewModel>> GetBlockedList(Guid userId)
        {
            return await DbContext.BlockedUsers.Include(b => b.Fk_UserId)
                                               .Include(b => b.Fk_BlockedUserId)
                                               .Where(b => b.Fk_UserId.UserId == userId)
                                               .Select(b => new BlockViewModel
                                               {
                                                   BlockedBy = $"{b.Fk_UserId.FirstName} {b.Fk_UserId.LastName}",
                                                   BlockedByUserId = b.Fk_UserId.UserId,
                                                   BlockedUser = $"{b.Fk_BlockedUserId.FirstName} {b.Fk_BlockedUserId.LastName}",
                                                   BlockedUserId = b.Fk_BlockedUserId.UserId,
                                                   ImageUrl = b.Fk_BlockedUserId.ImageUrl
                                               }).ToListAsync();
        }

        public async Task BlockUser(Guid userId)
        {
            var currentUser = await GetUser(userHelper.GetCurrentUserGuidLogin());
            var getUserBlock = await GetUser(userId);
            var blockUser = new BlockedUser
            {
                Fk_UserId = currentUser,
                Fk_BlockedUserId = getUserBlock,

                LastEditedBy = currentUser.UserId,
                LastEditedDate = DateTime.Now,
                CreatedBy = currentUser.UserId,
                CreatedDate = DateTime.Now,
                IsEnabled = true,
                IsEnabledBy = currentUser.UserId,
                DateEnabled = DateTime.Now,
                IsLocked = false,
                LockedDateTime = DateTime.Now,
            };

            var userFriend = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                        .Include(u => u.Fk_FriendUserId)
                                                        .Where(u => u.Fk_UserId.UserId == currentUser.UserId && u.Fk_FriendUserId.UserId == userId)
                                                        .FirstOrDefaultAsync();

            var userFriendRequest = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                                      .Include(u => u.Fk_FriendRequestUserId)
                                                                      .Where(u => u.Fk_FriendRequestUserId.UserId == currentUser.UserId && u.Fk_UserId.UserId == userId)
                                                                      .FirstOrDefaultAsync();
            if (userFriend != null)
            {
                userFriend.IsBlockedUser = true;
                userFriend.BlockedUserBy = currentUser.UserId;

                DbContext.UserFriends.Update(userFriend);
            }

            if (userFriendRequest != null)
            {
                userFriendRequest.IsBlockedUser = true;
                userFriendRequest.BlockedUserBy = currentUser.UserId;

                DbContext.UserFriendRequests.Update(userFriendRequest);
            }

            DbContext.BlockedUsers.Add(blockUser);
            await DbContext.SaveChangesAsync();
        }

        public async Task UnBlockUser(Guid userId)
        {
            var currentUser = userHelper.GetCurrentUserGuidLogin();
            var blockedUsers = await DbContext.BlockedUsers.Include(b => b.Fk_BlockedUserId).ToListAsync();
            if (blockedUsers.Count != 0)
            {
                var getUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == userId).FirstOrDefault();
                DbContext.BlockedUsers.Remove(getUser);
            }

            var userFriend = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                       .Include(u => u.Fk_FriendUserId)
                                                       .Where(u => u.Fk_UserId.UserId == currentUser && u.Fk_FriendUserId.UserId == userId)
                                                       .FirstOrDefaultAsync();

            var userFriendRequest = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                                      .Include(u => u.Fk_FriendRequestUserId)
                                                                      .Where(u => u.Fk_FriendRequestUserId.UserId == currentUser && u.Fk_UserId.UserId == userId)
                                                                      .FirstOrDefaultAsync();

            if (userFriend != null)
            {
                userFriend.IsBlockedUser = false;
                userFriend.BlockedUserBy = Guid.Empty;

                DbContext.UserFriends.Update(userFriend);
            }

            if (userFriendRequest != null)
            {
                userFriendRequest.IsBlockedUser = false;
                userFriendRequest.BlockedUserBy = Guid.Empty;

                DbContext.UserFriendRequests.Update(userFriendRequest);
            }

            await DbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserFriendViewModel>> GetFriends(Guid userId, string filter)
        {
            List<UserFriendViewModel> friends = new List<UserFriendViewModel>();

            if (filter != null)
            {
                friends = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                    .Include(u => u.Fk_FriendUserId)
                                                    .Where(u => u.Fk_UserId.UserId == userId && u.IsBlockedUser == false)
                                                    .Select(f => new UserFriendViewModel
                                                    {
                                                        UserId = f.Fk_FriendUserId.UserId,
                                                        FirstName = f.Fk_FriendUserId.FirstName,
                                                        LastName = f.Fk_FriendUserId.LastName,
                                                        ImageUrl = f.Fk_FriendUserId.ImageUrl,
                                                        Location = DbContext.UserAddresses.FirstOrDefault(u => u.UserId == userId).City
                                                    }).Where(x => x.FirstName.Contains(filter) || x.LastName.Contains(filter)).ToListAsync();
            }
            else
            {
                friends = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                    .Include(u => u.Fk_FriendUserId)
                                                    .Where(u => u.Fk_UserId.UserId == userId && u.IsBlockedUser == false)
                                                    .Select(f => new UserFriendViewModel
                                                    {
                                                        UserId = f.Fk_FriendUserId.UserId,
                                                        FirstName = f.Fk_FriendUserId.FirstName,
                                                        LastName = f.Fk_FriendUserId.LastName,
                                                        ImageUrl = f.Fk_FriendUserId.ImageUrl,
                                                        Location = DbContext.UserAddresses.FirstOrDefault(u => u.UserId == userId).City
                                                    }).ToListAsync();
            }


            var blockedUsers = await DbContext.BlockedUsers.Include(b => b.Fk_UserId)
                                                         .Include(b => b.Fk_BlockedUserId)
                                                         .ToListAsync();

            var filteredUsers = new List<UserFriendViewModel>();
            if (blockedUsers.Any())
            {
                foreach (var user in friends)
                {
                    var blockedUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == userId && user.UserId == b.Fk_BlockedUserId.UserId).FirstOrDefault();
                    if (blockedUser == null)
                    {
                        var blockedFromUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == userId && user.UserId == b.Fk_UserId.UserId).FirstOrDefault();
                        if (blockedFromUser == null)
                        {
                            filteredUsers.Add(user);
                        }
                    }
                }

                return filteredUsers;
            }

            return friends;
        }

        public async Task<bool> DeleteFriend(Guid currentUser, Guid friendUserId)
        {
            var existingFriends = new List<UserFriend>();
            var existingFriend = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                            .Include(u => u.Fk_FriendUserId)
                                                            .Where(u => u.Fk_UserId.UserId == currentUser && u.Fk_FriendUserId.UserId == friendUserId)
                                                            .FirstOrDefaultAsync();
            var existingFriend_2 = await DbContext.UserFriends.Include(u => u.Fk_UserId)
                                                            .Include(u => u.Fk_FriendUserId)
                                                            .Where(u => u.Fk_UserId.UserId == friendUserId && u.Fk_FriendUserId.UserId == currentUser)
                                                            .FirstOrDefaultAsync();
            if (existingFriend != null && existingFriend_2 != null)
            {
                existingFriends.Add(existingFriend);
                existingFriends.Add(existingFriend_2);
            }

            if (existingFriends.Any())
            {
                DbContext.UserFriends.RemoveRange(existingFriends);
                await DbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> CreateTeam(string teamName, List<Guid> memberUserId)
        {
            if (!string.IsNullOrWhiteSpace(teamName))
            {
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                var getCurrentUserInfo = await GetUser(currentUser);
                var newTeam = new UserTeam
                {
                    UserTeamId = Guid.NewGuid(),
                    Fk_UserId = getCurrentUserInfo,
                    TeamName = teamName,

                    LastEditedBy = currentUser,
                    LastEditedDate = DateTime.Now,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.Now,
                    IsEnabled = true,
                    IsEnabledBy = currentUser,
                    DateEnabled = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now,
                };

                DbContext.UserTeams.Add(newTeam);

                var memberList = new List<UserTeamMember>();
                if (memberUserId.Any())
                {
                    foreach (var memberUser in memberUserId)
                    {
                        var members = new UserTeamMember
                        {
                            Fk_UserTeamId = newTeam,
                            Fk_UserId = await GetUser(memberUser),

                            LastEditedBy = currentUser,
                            LastEditedDate = DateTime.Now,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = currentUser,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                        };

                        memberList.Add(members);
                    }

                    DbContext.UserTeamMembers.AddRange(memberList);
                }

                await DbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<IEnumerable<UserTeamViewModel>> GetTeams(Guid currentUserId, string filter)
        {
            List<UserTeamViewModel> existingTeams = new List<UserTeamViewModel>();
            if (filter != null)
            {
                existingTeams = await DbContext.UserTeams.Include(u => u.Fk_UserId)
                                                        .Where(u => u.Fk_UserId.UserId == currentUserId)
                                                        .Select(u => new UserTeamViewModel
                                                        {
                                                            UserTeamId = u.UserTeamId,
                                                            TeamName = u.TeamName,
                                                            TeamMembers = DbContext.UserTeamMembers.Include(t => t.Fk_UserTeamId)
                                                                                                   .Include(t => t.Fk_UserId)
                                                                                                   .Where(t => t.Fk_UserTeamId.UserTeamId == u.UserTeamId)
                                                                                                   .Select(t => new UserTeamMemberViewModel
                                                                                                   {
                                                                                                       FirstName = t.Fk_UserId.FirstName,
                                                                                                       LastName = t.Fk_UserId.LastName,
                                                                                                       UserId = t.Fk_UserId.UserId,
                                                                                                       UserTeamMemberId = t.Fk_UserTeamId.UserTeamId,
                                                                                                       ImageUrl = t.Fk_UserId.ImageUrl
                                                                                                   }).ToList()

                                                        }).Where(x => x.TeamName.Contains(filter)).ToListAsync();
            }
            else
            {
                existingTeams = await DbContext.UserTeams.Include(u => u.Fk_UserId)
                                                        .Where(u => u.Fk_UserId.UserId == currentUserId)
                                                        .Select(u => new UserTeamViewModel
                                                        {
                                                            UserTeamId = u.UserTeamId,
                                                            TeamName = u.TeamName,
                                                            TeamMembers = DbContext.UserTeamMembers.Include(t => t.Fk_UserTeamId)
                                                                                                   .Include(t => t.Fk_UserId)
                                                                                                   .Where(t => t.Fk_UserTeamId.UserTeamId == u.UserTeamId)
                                                                                                   .Select(t => new UserTeamMemberViewModel
                                                                                                   {
                                                                                                       FirstName = t.Fk_UserId.FirstName,
                                                                                                       LastName = t.Fk_UserId.LastName,
                                                                                                       UserId = t.Fk_UserId.UserId,
                                                                                                       UserTeamMemberId = t.Fk_UserTeamId.UserTeamId,
                                                                                                       ImageUrl = t.Fk_UserId.ImageUrl
                                                                                                   }).ToList(),

                                                        }).ToListAsync();
            }


            foreach (var memberCount in existingTeams)
            {
                memberCount.MemberCount = memberCount.TeamMembers.Count;
            }

            return existingTeams;
        }

        public async Task<UserTeamViewModel> GetUserTeam(Guid userTeamId, string filter)
        {
            if (filter != null)
            {
                return await DbContext.UserTeams.Include(u => u.Fk_UserId)
                                                    .Where(u => u.UserTeamId == userTeamId)
                                                    .Select(u => new UserTeamViewModel
                                                    {
                                                        UserTeamId = u.UserTeamId,
                                                        TeamName = u.TeamName,
                                                        TeamMembers = DbContext.UserTeamMembers.Include(t => t.Fk_UserTeamId)
                                                                                               .Include(t => t.Fk_UserId)
                                                                                               .Where(t => t.Fk_UserTeamId.UserTeamId == u.UserTeamId)
                                                                                               .Select(t => new UserTeamMemberViewModel
                                                                                               {
                                                                                                   FirstName = t.Fk_UserId.FirstName,
                                                                                                   LastName = t.Fk_UserId.LastName,
                                                                                                   UserId = t.Fk_UserId.UserId,
                                                                                                   UserTeamMemberId = t.Fk_UserTeamId.UserTeamId,
                                                                                                   ImageUrl = t.Fk_UserId.ImageUrl
                                                                                               }).Where(x => x.FirstName.Contains(filter) || x.LastName.Contains(filter)).ToList()
                                                    })
                                                    .FirstOrDefaultAsync();
            }
            else
            {
                return await DbContext.UserTeams.Include(u => u.Fk_UserId)
                                                    .Where(u => u.UserTeamId == userTeamId)
                                                    .Select(u => new UserTeamViewModel
                                                    {
                                                        UserTeamId = u.UserTeamId,
                                                        TeamName = u.TeamName,
                                                        TeamMembers = DbContext.UserTeamMembers.Include(t => t.Fk_UserTeamId)
                                                                                               .Include(t => t.Fk_UserId)
                                                                                               .Where(t => t.Fk_UserTeamId.UserTeamId == u.UserTeamId)
                                                                                               .Select(t => new UserTeamMemberViewModel
                                                                                               {
                                                                                                   FirstName = t.Fk_UserId.FirstName,
                                                                                                   LastName = t.Fk_UserId.LastName,
                                                                                                   UserId = t.Fk_UserId.UserId,
                                                                                                   UserTeamMemberId = t.Fk_UserTeamId.UserTeamId,
                                                                                                   ImageUrl = t.Fk_UserId.ImageUrl
                                                                                               }).ToList()
                                                    })
                                                    .FirstOrDefaultAsync();
            }

        }

        public async Task<bool> EditTeam(UserTeamViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.TeamName))
            {
                var currentUser = userHelper.GetCurrentUserGuidLogin();
                var existingTeam = await DbContext.UserTeams.Include(u => u.Fk_UserId).Where(u => u.UserTeamId == viewModel.UserTeamId).FirstOrDefaultAsync();
                existingTeam.TeamName = viewModel.TeamName;

                existingTeam.LastEditedBy = currentUser;
                existingTeam.LastEditedDate = DateTime.Now;

                DbContext.UserTeams.Update(existingTeam);

                var teamMembers = await DbContext.UserTeamMembers.Include(u => u.Fk_UserTeamId)
                                                                 .Include(u => u.Fk_UserId)
                                                                 .Where(u => u.Fk_UserTeamId.UserTeamId == viewModel.UserTeamId)
                                                                 .ToListAsync();
                var newModelList = new List<UserTeamMember>();
                if (viewModel.TeamMembers.Count != 0)
                {
                    foreach (var member in viewModel.TeamMembers)
                    {
                        var memberExisting = teamMembers.Where(u => u.Fk_UserId.UserId == member.UserId).FirstOrDefault();
                        if (memberExisting != null)
                        {
                            newModelList.Add(memberExisting);
                        }
                        else
                        {
                            newModelList.Add(new UserTeamMember
                            {
                                Fk_UserId = await GetUser(member.UserId),
                                Fk_UserTeamId = existingTeam,

                                LastEditedBy = currentUser,
                                LastEditedDate = DateTime.Now,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.Now,
                                IsEnabled = true,
                                IsEnabledBy = currentUser,
                                DateEnabled = DateTime.Now,
                                IsLocked = false,
                                LockedDateTime = DateTime.Now,
                            });
                        }
                    }
                }

                DbContext.UserTeamMembers.RemoveRange(teamMembers);
                await DbContext.SaveChangesAsync();
                if (newModelList.Any())
                {
                    DbContext.UserTeamMembers.AddRange(newModelList);
                }

                await DbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<APIResponse> GetUserList(string auth)
        {
            var apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::GetUserList --");
            LogManager.LogDebugObject(auth);

            try
            {
                //get user login creds
                var isLoggedIn = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (isLoggedIn != null)
                {
                    var users = await DbContext.Users.ToListAsync();
                    return new APIResponse
                    {
                        Payload = users,
                        Status = "Success",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                return new APIResponse
                {
                    Message = "Unathorized Access.",
                    Status = "Failed",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetUserList --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<IEnumerable<UserFriendViewModel>> GetAllUsers(Guid currentUser, string filter)
        {
            List<User> users = new List<User>();
            if (filter != null)
            {
                users = await DbContext.Users.Where(u => u.UserId != currentUser && (u.FirstName.Contains(filter) || u.LastName.Contains(filter))).ToListAsync();
            }
            else
            {
                users = await DbContext.Users.Where(u => u.UserId != currentUser).ToListAsync();
            }


            var blockedUsers = await DbContext.BlockedUsers.Include(b => b.Fk_UserId)
                                                           .Include(b => b.Fk_BlockedUserId)
                                                           .ToListAsync();
            var filteredUsers = new List<User>();
            if (blockedUsers.Any())
            {
                foreach (var user in users)
                {
                    var blockedUser = blockedUsers.Where(b => b.Fk_UserId.UserId == currentUser && user.UserId == b.Fk_BlockedUserId.UserId).FirstOrDefault();
                    if (blockedUser == null)
                    {
                        var blockedFromUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == currentUser && user.UserId == b.Fk_UserId.UserId).FirstOrDefault();
                        if (blockedFromUser == null)
                        {
                            filteredUsers.Add(user);
                        }
                    }
                }
            }

            var userAddresses = await DbContext.UserAddresses.ToListAsync();
            var userFriendViewModel = new List<UserFriendViewModel>();
            foreach (var user in filteredUsers.Count != 0 ? filteredUsers : users)
            {
                userFriendViewModel.Add(new UserFriendViewModel
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ImageUrl = user.ImageUrl,
                    Location = userAddresses.Where(u => u.UserId == user.UserId).FirstOrDefault() != null ? userAddresses.Where(u => u.UserId == user.UserId).FirstOrDefault().City : string.Empty
                });
            }

            return userFriendViewModel;
        }

        public async Task<IEnumerable<UserFriendViewModel>> AddedFriendRequests(Guid currentUser, string filter)
        {
            var addedRequests = new List<UserFriendViewModel>();

            if (DbContext.UserFriendRequests.Count() > 0)
            {
                if (filter != null)
                {
                    addedRequests = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                              .Include(u => u.Fk_FriendRequestUserId)
                                                              .Where(u => u.Fk_FriendRequestUserId.UserId == currentUser && u.IsBlockedUser == false)
                                                              .Select(u => new UserFriendViewModel
                                                              {
                                                                  UserId = u.Fk_UserId.UserId,
                                                                  FirstName = u.Fk_UserId.FirstName,
                                                                  LastName = u.Fk_UserId.LastName,
                                                                  ImageUrl = u.Fk_UserId.ImageUrl,
                                                                  Location = DbContext.UserAddresses.FirstOrDefault(l => l.UserId == u.Fk_UserId.UserId) != null ? DbContext.UserAddresses.FirstOrDefault(l => l.UserId == u.Fk_UserId.UserId).City : string.Empty
                                                              }).Where(x => x.FirstName.Contains(filter) || x.LastName.Contains(filter)).ToListAsync();
                }
                else
                {
                    addedRequests = await DbContext.UserFriendRequests.Include(u => u.Fk_UserId)
                                                              .Include(u => u.Fk_FriendRequestUserId)
                                                              .Where(u => u.Fk_FriendRequestUserId.UserId == currentUser && u.IsBlockedUser == false)
                                                              .Select(u => new UserFriendViewModel
                                                              {
                                                                  UserId = u.Fk_UserId.UserId,
                                                                  FirstName = u.Fk_UserId.FirstName,
                                                                  LastName = u.Fk_UserId.LastName,
                                                                  ImageUrl = u.Fk_UserId.ImageUrl,
                                                                  Location = DbContext.UserAddresses.FirstOrDefault(l => l.UserId == u.Fk_UserId.UserId) != null ? DbContext.UserAddresses.FirstOrDefault(l => l.UserId == u.Fk_UserId.UserId).City : string.Empty
                                                              }).ToListAsync();
                }

            }
            var blockedUsers = await DbContext.BlockedUsers.Include(b => b.Fk_UserId)
                                                          .Include(b => b.Fk_BlockedUserId)
                                                          .ToListAsync();

            var filteredUsers = new List<UserFriendViewModel>();
            if (blockedUsers.Any())
            {
                foreach (var user in addedRequests)
                {
                    var blockedUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == currentUser && user.UserId == b.Fk_BlockedUserId.UserId).FirstOrDefault();
                    if (blockedUser == null)
                    {
                        var blockedFromUser = blockedUsers.Where(b => b.Fk_BlockedUserId.UserId == currentUser && user.UserId == b.Fk_UserId.UserId).FirstOrDefault();
                        if (blockedFromUser == null)
                        {
                            filteredUsers.Add(user);
                        }
                    }
                }

                return filteredUsers;
            }

            return addedRequests;
        }

        public async Task<CoachRenderViewModel> GetCoachUserProfile(Guid userId)
        {
            var coachRenderViewModel = new CoachRenderViewModel();
            var user = await GetUser(userId);
            if (user != null)
            {
                var coachSpecialties = await ICoachRepo.GetSpecialties(userId);
                var coachGyms = await ICoachRepo.GetGymsAccess(userId);
                var languages = await ICoachRepo.GetLanguages(userId);

                coachRenderViewModel.FirstName = user.FirstName;
                coachRenderViewModel.LastName = user.LastName;
                coachRenderViewModel.ImageUrl = user.ImageUrl;
                coachRenderViewModel.MobileNumber = user.MobileNumber;
                coachRenderViewModel.Email = user.Email;
                coachRenderViewModel.NationalityId = user.NationalityId;
                coachRenderViewModel.Description = user.Description;
                coachRenderViewModel.SpecialtyIds = new List<Guid>();
                coachRenderViewModel.GymIds = new List<Guid>();
                coachRenderViewModel.LanguageIds = new List<Guid>();
                coachRenderViewModel.IsActive = user.IsEnabled.Value;
                foreach (var coachSpecialty in coachSpecialties)
                {
                    coachRenderViewModel.SpecialtyIds.Add(coachSpecialty.SpecialtyId);
                }
                foreach (var coachGym in coachGyms)
                {
                    coachRenderViewModel.GymIds.Add(coachGym.GymID);
                }
                foreach (var language in languages)
                {
                    coachRenderViewModel.LanguageIds.Add(language.LanguageId);
                }
            }

            coachRenderViewModel.Bookings = new List<CoachBookingsViewModel>();
            var groupBookings = await DbContext.GroupClasses
                .Where(x => x.CoachId == userId
                         && x.IsEnabled == true)
                .ToListAsync();
            if (groupBookings.Any())
            {
                foreach (var groupBooking in groupBookings)
                {
                    coachRenderViewModel.Bookings.Add(new CoachBookingsViewModel
                    {
                        ClassCategory = groupBooking.Title,
                        Start = groupBooking.Start.Value,
                        End = groupBooking.Start.GetValueOrDefault().AddHours(groupBooking.Duration.GetValueOrDefault()),
                        Date = groupBooking.Start.Value,
                        Price = groupBooking.Price
                    });
                }
            }

            var individualBookings = await (from x in DbContext.IndividualClassDetails
                                            join y in DbContext.IndividualClasses
                                              on x.IndividualClassId equals y.ClassId
                                            where y.CoachId == userId
                                            && x.IsEnabled == true && y.IsEnabled == true
                                            select x)
                          .ToListAsync();
            if (individualBookings.Any())
            {
                foreach (var individualBooking in individualBookings)
                {
                    coachRenderViewModel.Bookings.Add(new CoachBookingsViewModel
                    {
                        Price = individualBooking.Price,
                        ClassCategory = "Individual"
                    });
                }
            }

            return coachRenderViewModel;
        }

        public async Task<APIResponse> ChangeStatus(string auth, ChangeStatus user)
        {
            var apiResp = new APIResponse();
            LogManager.LogDebugObject(user);
            LogManager.LogInfo("-- Run::UserRepository::ChangeStatus --");
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

                var userExisting = await GetUser(user.GuID);
                if (userExisting == null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        Message = "User Not Found",
                    };
                }

                userExisting.IsEnabled = user.IsEnabled;

                userExisting.LastEditedBy = isLoggedIn.AdminId;
                userExisting.LastEditedDate = DateTime.Now;


                DbContext.Users.Update(userExisting);

                var coachExisting = await ICoachRepo.GetCoach(user.GuID);
                if (coachExisting == null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        Message = "Coach Information Not Found",
                    };
                }

                coachExisting.IsEnabled = user.IsEnabled;
                coachExisting.LastEditedBy = isLoggedIn.AdminId;
                coachExisting.LastEditedDate = DateTime.Now;

                //var trainBookings = await DbContext.GroupClasses.Where(g => g.CoachId == user.GuID).ToListAsync();

                //DbContext.Users.Remove(userExisting);
                DbContext.Coaches.Update(coachExisting);

                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Successfully Set Coach to Inactive",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::ChangeStatus --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> SendRequest(RequestViewModel model)
        {
            var apiResp = new APIResponse();
            LogManager.LogDebugObject(model.Description);
            LogManager.LogInfo("-- Run::UserRepository::SendRequest --");
            var currentUser = userHelper.GetCurrentUserGuidLogin();
            try
            {
                var newRequest = new Request
                {
                    Description = model.Description,
                    UserId = currentUser,
                    Status = Model.Enums.RequestStatus.New,

                    LastEditedBy = currentUser,
                    LastEditedDate = DateTime.Now,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.Now,
                    IsEnabled = true,
                    IsEnabledBy = currentUser,
                    DateEnabled = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now,
                };

                DbContext.Requests.Add(newRequest);
                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Successfully Sent Request",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::SendRequest --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetUserRequests()
        {
            var apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::GetUserRequests --");
            try
            {
                var userRequestList = new List<UserRequestViewModel>();
                var requests = await DbContext.Requests.ToListAsync();
                foreach (var user in requests)
                {
                    var isUser = await DbContext.Users.Where(f => f.UserId == user.UserId).FirstOrDefaultAsync();
                    if (isUser != null)
                    {
                        userRequestList.Add(new UserRequestViewModel
                        {
                            UserId = user.UserId,
                            Id = user.Id,
                            Name = $"{isUser.FirstName} {isUser.LastName}",
                            Description = user.Description,
                            MobileNumber = isUser.MobileNumber,
                            Email = isUser.Email,
                            Date = user.CreatedDate.Value,
                            Status = user.Status
                        });
                    }
                }

                return new APIResponse
                {
                    Message = "Retrieved User Requests",
                    Payload = userRequestList,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetUserRequests --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> UpdateUserRequest(string auth, UserRequestViewModel userRequest)
        {
            var apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::UpdateUserRequest --");
            LogManager.LogDebugObject(userRequest);
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

                var existingRequest = await DbContext.Requests.Where(r => r.Id == userRequest.Id).FirstOrDefaultAsync();
                if (existingRequest == null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        Message = "Request not Found",
                    };
                }

                existingRequest.Status = userRequest.Status;

                existingRequest.LastEditedBy = isLoggedIn.AdminId;
                existingRequest.LastEditedDate = DateTime.Now;

                DbContext.Requests.Update(existingRequest);
                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Updated User Request",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::UpdateUserRequest --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetCoachRequests()
        {
            var apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::GetCoachRequests --");
            try
            {
                var coachRequestList = new List<CoachRequestViewModel>();
                var requests = await DbContext.Requests.ToListAsync();
                foreach (var user in requests)
                {
                    var coachUser = await DbContext.Coaches.Where(f => f.CoachUserId == user.UserId).FirstOrDefaultAsync();
                    if (coachUser != null)
                    {
                        var getUserProfile = await GetUser(coachUser.CoachUserId.Value);
                        coachRequestList.Add(new CoachRequestViewModel
                        {
                            Id = user.Id,
                            Name = $"{getUserProfile.FirstName} {getUserProfile.LastName}",
                            Description = user.Description,
                            MobileNumber = getUserProfile.MobileNumber,
                            Email = getUserProfile.Email,
                            Date = user.CreatedDate.Value,
                            Status = user.Status
                        });
                    }
                }

                return new APIResponse
                {
                    Message = "Retrieved Coach Requests",
                    Payload = coachRequestList,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetCoachRequests --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> UpdateCoachRequest(string auth, CoachRequestViewModel coachRequest)
        {
            var apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::UserRepository::UpdateCoachRequest --");
            LogManager.LogDebugObject(coachRequest);
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

                var existingRequest = await DbContext.Requests.Where(r => r.Id == coachRequest.Id).FirstOrDefaultAsync();
                if (existingRequest == null)
                {
                    return new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        Message = "Request not Found",
                    };
                }

                existingRequest.Status = coachRequest.Status;

                existingRequest.LastEditedBy = isLoggedIn.AdminId;
                existingRequest.LastEditedDate = DateTime.Now;

                DbContext.Requests.Update(existingRequest);
                await DbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = "Updated User Request",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::UpdateCoachRequest --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetUserUpdates()
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var groupActivityQuery = await (from x in DbContext.GroupBookings
                                                join y in DbContext.GroupClasses
                                                  on x.GroupClassId equals y.GroupClassId
                                                join z in DbContext.Locations
                                                   on y.LocationId equals z.LocationId
                                                join participant in DbContext.Users
                                                   on x.ParticipantId equals participant.UserId
                                                where x.Status == EBookingStatus.Approved && x.ParticipantId == currentLogin
                                                select new ActivityList
                                                {
                                                    Id = y.Id,
                                                    Title = y.Title,
                                                    Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                    Image = participant.ImageUrl,
                                                    Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                    Locations = z.Name,
                                                    StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                    CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                    Type = ReviewType.GroupClass,
                                                }).ToListAsync();

                var groupActivityQueryNoLocation = await (from x in DbContext.GroupBookings
                                                          join y in DbContext.GroupClasses
                                                            on x.GroupClassId equals y.GroupClassId
                                                          join participant in DbContext.Users
                                                             on x.ParticipantId equals participant.UserId
                                                          where x.Status == EBookingStatus.Approved && x.ParticipantId == currentLogin
                                                          select new ActivityList
                                                          {
                                                              Id = y.Id,
                                                              Title = y.Title,
                                                              Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                              Image = participant.ImageUrl,
                                                              Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                              Locations = "",
                                                              StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                              CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                              Type = ReviewType.GroupClass,
                                                          }).ToListAsync();

                var individualActivityQuery = await (from x in DbContext.IndividualBookings
                                                     join y in DbContext.IndividualClasses
                                                       on x.ClassId equals y.ClassId
                                                     join participant in DbContext.Users
                                                        on x.TraineeId equals participant.UserId
                                                     where x.Status == EBookingStatus.Approved && x.TraineeId == currentLogin
                                                     select new ActivityList
                                                     {
                                                         Id = y.Id,
                                                         Title = "Individual Class",
                                                         Description = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm tt"),
                                                         Image = participant.ImageUrl,
                                                         Price = decimal.Round(x.TotalAmount, 2, MidpointRounding.AwayFromZero),
                                                         Locations = x.Location,
                                                         StartIn = GetTimeSince(Convert.ToDateTime(x.Date)),
                                                         CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                         Type = ReviewType.IndividualClass,
                                                     }).ToListAsync();

                var activityQuery = groupActivityQuery.Union(groupActivityQueryNoLocation).Union(individualActivityQuery).Take(2);

                var groupRequstQuery = await (from x in DbContext.GroupBookings
                                              join y in DbContext.GroupClasses
                                                on x.GroupClassId equals y.GroupClassId
                                              join z in DbContext.Locations
                                               on y.LocationId equals z.LocationId
                                              join participant in DbContext.Users
                                                 on x.ParticipantId equals participant.UserId
                                              where x.Status == EBookingStatus.Pending && x.ParticipantId == currentLogin
                                              select new ActivityList
                                              {
                                                  Id = y.Id,
                                                  Title = y.Title,
                                                  Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                  Image = participant.ImageUrl,
                                                  Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                  Locations = z.Name,
                                                  StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                  CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                  Type = ReviewType.GroupClass,
                                              }).ToListAsync();

                var groupRequstQueryNoLocation = await (from x in DbContext.GroupBookings
                                                        join y in DbContext.GroupClasses
                                                          on x.GroupClassId equals y.GroupClassId
                                                        join participant in DbContext.Users
                                                           on x.ParticipantId equals participant.UserId
                                                        where x.Status == EBookingStatus.Pending && x.ParticipantId == currentLogin
                                                        select new ActivityList
                                                        {
                                                            Id = y.Id,
                                                            Title = y.Title,
                                                            Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                            Image = participant.ImageUrl,
                                                            Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                            Locations = "",
                                                            StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                            CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                            Type = ReviewType.GroupClass,
                                                        }).ToListAsync();

                var individualRequestQuery = await (from x in DbContext.IndividualBookings
                                                    join y in DbContext.IndividualClasses
                                                       on x.ClassId equals y.ClassId
                                                    join participant in DbContext.Users
                                                       on x.TraineeId equals participant.UserId
                                                    where x.Status == EBookingStatus.Pending && x.TraineeId == currentLogin
                                                    select new ActivityList
                                                    {
                                                        Id = y.Id,
                                                        Title = "Individual Class",
                                                        Description = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm tt"),
                                                        Image = participant.ImageUrl,
                                                        Price = decimal.Round(x.TotalAmount, 2, MidpointRounding.AwayFromZero),
                                                        Locations = x.Location,
                                                        StartIn = GetTimeSince(Convert.ToDateTime(x.Date)),
                                                        CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                        Type = ReviewType.IndividualClass,
                                                    }).ToListAsync();

                var requstQuery = individualRequestQuery.Union(groupRequstQueryNoLocation).Union(groupRequstQuery).Take(2);

                var friendQuery = (from x in DbContext.UserFriendRequests
                                   join y in DbContext.Users
                                     on x.FriendRequestUserId equals y.Id
                                   where y.UserId == currentLogin
                                   select new WhatsNewList
                                   {
                                       Title = "Pending Request from " + y.FirstName,
                                       Description = GetTimeSince(Convert.ToDateTime(x.CreatedDate)),
                                       Image = y.ImageUrl,
                                   });

                UserActivities response = new UserActivities();
                response.Activities = activityQuery.ToList();
                response.ActivityRequest = requstQuery.ToList();
                response.WhatsNew = await friendQuery.Take(2).ToListAsync();
                response.Banner = await DbContext.Banners.Where(x => x.IsEnabled == true && DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate).ToListAsync();

                return new APIResponse
                {
                    Message = "Retrieved User Updates",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetUserUpdates --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetUserUpdatesByDate(string date)
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var currentIdLogin = userHelper.GetCurrentUserIdLogin();

                var groupActivityQuery = await (from x in DbContext.GroupBookings
                                                join y in DbContext.GroupClasses
                                                  on x.GroupClassId equals y.GroupClassId
                                                join z in DbContext.Locations
                                                   on y.LocationId equals z.LocationId
                                                join participant in DbContext.Users
                                                   on x.ParticipantId equals participant.UserId
                                                where x.Status == EBookingStatus.Approved && x.ParticipantId == currentLogin
                                                select new ActivityList
                                                {
                                                    Id = y.Id,
                                                    Title = y.Title,
                                                    Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                    Image = participant.ImageUrl,
                                                    Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                    Locations = z.Name,
                                                    StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                    CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                    Type = ReviewType.GroupClass,
                                                }).ToListAsync();

                var individualActivityQuery = await (from x in DbContext.IndividualBookings
                                                     join y in DbContext.IndividualClasses
                                                       on x.ClassId equals y.ClassId
                                                     join participant in DbContext.Users
                                                        on x.TraineeId equals participant.UserId
                                                     where x.Status == EBookingStatus.Approved && x.TraineeId == currentLogin
                                                     select new ActivityList
                                                     {
                                                         Id = y.Id,
                                                         Title = "Individual Class",
                                                         Description = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm tt"),
                                                         Image = participant.ImageUrl,
                                                         Price = decimal.Round(x.TotalAmount, 2, MidpointRounding.AwayFromZero),
                                                         Locations = x.Location,
                                                         StartIn = GetTimeSince(Convert.ToDateTime(x.Date)),
                                                         CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                         Type = ReviewType.IndividualClass,
                                                     }).ToListAsync();

                var activityQuery = groupActivityQuery.Union(individualActivityQuery).Where(x => x.CreatedDate == Convert.ToDateTime(date).ToString("MM/dd/yyyy")).Take(2);

                var groupRequstQuery = await (from x in DbContext.GroupBookings
                                              join y in DbContext.GroupClasses
                                                on x.GroupClassId equals y.GroupClassId
                                              join z in DbContext.Locations
                                               on y.LocationId equals z.LocationId
                                              join participant in DbContext.Users
                                                 on x.ParticipantId equals participant.UserId
                                              where x.Status == EBookingStatus.Pending && x.ParticipantId == currentLogin
                                              select new ActivityList
                                              {
                                                  Id = y.Id,
                                                  Title = y.Title,
                                                  Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                  Image = participant.ImageUrl,
                                                  Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                  Locations = z.Name,
                                                  StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                  CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                  Type = ReviewType.GroupClass,
                                              }).ToListAsync();

                var individualRequestQuery = await (from x in DbContext.IndividualBookings
                                                    join y in DbContext.IndividualClasses
                                                       on x.ClassId equals y.ClassId
                                                    join participant in DbContext.Users
                                                       on x.TraineeId equals participant.UserId
                                                    where x.Status == EBookingStatus.Pending && x.TraineeId == currentLogin
                                                    select new ActivityList
                                                    {
                                                        Id = y.Id,
                                                        Title = "Individual Class",
                                                        Description = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm tt"),
                                                        Image = participant.ImageUrl,
                                                        Price = decimal.Round(x.TotalAmount, 2, MidpointRounding.AwayFromZero),
                                                        Locations = x.Location,
                                                        StartIn = GetTimeSince(Convert.ToDateTime(x.Date)),
                                                        CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                        Type = ReviewType.IndividualClass,
                                                    }).ToListAsync();

                var requstQuery = individualRequestQuery.Union(groupRequstQuery).Where(x => x.CreatedDate == Convert.ToDateTime(date).ToString("MM/dd/yyyy")).Take(2);

                var friendQuery = (from x in DbContext.UserFriendRequests
                                   join y in DbContext.Users
                                     on x.FriendRequestUserId equals y.Id
                                   where x.UserId == currentIdLogin
                                   select new WhatsNewList
                                   {
                                       Title = "Pending Request from " + y.FirstName,
                                       Description = GetTimeSince(Convert.ToDateTime(x.CreatedDate)),
                                       Image = y.ImageUrl,
                                       CreatedDate = Convert.ToDateTime(x.CreatedDate).ToString("MM/dd/yyyy")
                                   });

                var WhatsNew = await friendQuery.ToListAsync();

                UserActivities response = new UserActivities();
                response.Activities = activityQuery.ToList();
                response.ActivityRequest = requstQuery.ToList();
                response.WhatsNew = WhatsNew.Where(x => x.CreatedDate == Convert.ToDateTime(date).ToString("MM/dd/yyyy")).Take(2).ToList();
                response.Banner = await DbContext.Banners.Where(x => DateTime.Now >= x.StartDate && DateTime.Now < x.EndDate.AddDays(1)).ToListAsync();

                response.BookingDates = new List<string>();
                var ctd = Convert.ToDateTime(date);
                var userGroupBookingDates = activityQuery.ToList().Union(requstQuery.ToList());
                foreach (var userGroupBookingDate in userGroupBookingDates)
                {
                    response.BookingDates.Add(Convert.ToDateTime(userGroupBookingDate.CreatedDate).ToString("dd MMM yyyy"));
                }

                response.BookingDates = response.BookingDates.Any() ? response.BookingDates.Distinct().ToList() : new List<string>();

                return new APIResponse
                {
                    Message = "Retrieved User Updates",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetUserUpdates --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetAllActivities()
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var groupActivityQuery = await (from x in DbContext.GroupBookings
                                                join y in DbContext.GroupClasses
                                                  on x.GroupClassId equals y.GroupClassId
                                                join z in DbContext.Locations
                                                   on y.LocationId equals z.LocationId
                                                join participant in DbContext.Users
                                                   on x.ParticipantId equals participant.UserId
                                                where x.Status == EBookingStatus.Approved && x.ParticipantId == currentLogin
                                                select new ActivityList
                                                {
                                                    Id = y.Id,
                                                    Title = y.Title,
                                                    Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                    Image = participant.ImageUrl,
                                                    Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                    Locations = z.Name,
                                                    StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                    CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                    Type = ReviewType.GroupClass,
                                                }).ToListAsync();

                var groupActivityQueryNoLocation = await (from x in DbContext.GroupBookings
                                                          join y in DbContext.GroupClasses
                                                            on x.GroupClassId equals y.GroupClassId
                                                          join participant in DbContext.Users
                                                             on x.ParticipantId equals participant.UserId
                                                          where x.Status == EBookingStatus.Approved && x.ParticipantId == currentLogin
                                                          select new ActivityList
                                                          {
                                                              Id = y.Id,
                                                              Title = y.Title,
                                                              Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                              Image = participant.ImageUrl,
                                                              Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                              Locations = "",
                                                              StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                              CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                              Type = ReviewType.GroupClass,
                                                          }).ToListAsync();

                var individualActivityQuery = await (from x in DbContext.IndividualBookings
                                                     join y in DbContext.IndividualClasses
                                                       on x.ClassId equals y.ClassId
                                                     join participant in DbContext.Users
                                                        on x.TraineeId equals participant.UserId
                                                     where x.Status == EBookingStatus.Approved && x.TraineeId == currentLogin
                                                     select new ActivityList
                                                     {
                                                         Id = y.Id,
                                                         Title = "Individual Class",
                                                         Description = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm tt"),
                                                         Image = participant.ImageUrl,
                                                         Price = decimal.Round(x.TotalAmount, 2, MidpointRounding.AwayFromZero),
                                                         Locations = x.Location,
                                                         StartIn = GetTimeSince(Convert.ToDateTime(x.Date)),
                                                         CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                         Type = ReviewType.IndividualClass,
                                                     }).ToListAsync();

                var response = groupActivityQuery.Union(groupActivityQueryNoLocation).Union(individualActivityQuery);

                return new APIResponse
                {
                    Message = "Retrieved User Activities",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetAllActivities --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetAllRequest()
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var groupRequstQuery = await (from x in DbContext.GroupBookings
                                              join y in DbContext.GroupClasses
                                                on x.GroupClassId equals y.GroupClassId
                                              join z in DbContext.Locations
                                               on y.LocationId equals z.LocationId
                                              join participant in DbContext.Users
                                                 on x.ParticipantId equals participant.UserId
                                              where x.Status == EBookingStatus.Pending && x.ParticipantId == currentLogin
                                              select new ActivityList
                                              {
                                                  Id = y.Id,
                                                  Title = y.Title,
                                                  Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                  Image = participant.ImageUrl,
                                                  Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                  Locations = z.Name,
                                                  StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                  CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                  Type = ReviewType.GroupClass,
                                              }).ToListAsync();

                var groupRequstQueryNoLocation = await (from x in DbContext.GroupBookings
                                                        join y in DbContext.GroupClasses
                                                          on x.GroupClassId equals y.GroupClassId
                                                        join participant in DbContext.Users
                                                           on x.ParticipantId equals participant.UserId
                                                        where x.Status == EBookingStatus.Pending && x.ParticipantId == currentLogin
                                                        select new ActivityList
                                                        {
                                                            Id = y.Id,
                                                            Title = y.Title,
                                                            Description = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm tt"),
                                                            Image = participant.ImageUrl,
                                                            Price = decimal.Round(y.Price, 2, MidpointRounding.AwayFromZero),
                                                            Locations = "",
                                                            StartIn = GetTimeSince(Convert.ToDateTime(y.Start)),
                                                            CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                            Type = ReviewType.GroupClass,
                                                        }).ToListAsync();

                var individualRequestQuery = await (from x in DbContext.IndividualBookings
                                                    join y in DbContext.IndividualClasses
                                                       on x.ClassId equals y.ClassId
                                                    join participant in DbContext.Users
                                                       on x.TraineeId equals participant.UserId
                                                    where x.Status == EBookingStatus.Pending && x.TraineeId == currentLogin
                                                    select new ActivityList
                                                    {
                                                        Id = y.Id,
                                                        Title = "Individual Class",
                                                        Description = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm tt"),
                                                        Image = participant.ImageUrl,
                                                        Price = decimal.Round(x.TotalAmount, 2, MidpointRounding.AwayFromZero),
                                                        Locations = x.Location,
                                                        StartIn = GetTimeSince(Convert.ToDateTime(x.Date)),
                                                        CreatedDate = Convert.ToDateTime(x.Date).ToString("MM/dd/yyyy"),
                                                        Type = ReviewType.IndividualClass,
                                                    }).ToListAsync();

                var requstQuery = individualRequestQuery.Union(groupRequstQueryNoLocation).Union(groupRequstQuery);

                return new APIResponse
                {
                    Message = "Retrieved User Request",
                    Payload = requstQuery,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetAllRequest --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetAllWhatsNew()
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await (from x in DbContext.UserFriendRequests
                                      join y in DbContext.Users
                                        on x.FriendRequestUserId equals y.Id
                                      where y.UserId == currentLogin
                                      select new WhatsNewList
                                      {
                                          Title = "Pending Request from " + y.FirstName,
                                          Description = GetTimeSince(Convert.ToDateTime(x.CreatedDate)),
                                          Image = y.ImageUrl,
                                      }).ToListAsync();

                return new APIResponse
                {
                    Message = "Retrieved Whats New",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetAllWhatsNew --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> AddUserReviews(UserReviews _model)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::UserRepository::AddUserReviews --");
            LogManager.LogDebugObject(_model);
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            try
            {
                UserReview req = new UserReview();
                req.UserId = currentLogin;
                req.Description = _model.Description;
                req.Ratings = _model.Ratings;
                req.ClassId = _model.ClassId;
                req.CoachId = _model.CoachId;
                req.Type = _model.Type;
                req.IsEnabled = true;
                req.CreatedBy = currentLogin;
                req.CreatedDate = DateTime.Now;

                DbContext.UserReviews.Add(req);
                await DbContext.SaveChangesAsync();

                apiResp.Message = "User Review Added!";
                apiResp.Status = "Success!";
                apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                return apiResp;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::AddUserReviews --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetCoachToReview()
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                List<UserReviews> response = new List<UserReviews>();

                var groupResponse = await (from x in DbContext.GroupBookings
                                           join y in DbContext.GroupClasses
                                             on x.GroupClassId equals y.GroupClassId
                                           join participant in DbContext.Users
                                            on y.CoachId equals participant.UserId
                                           where x.Status == EBookingStatus.Approved && x.ParticipantId == currentLogin
                                           && x.CreatedDate < DateTime.Now
                                           select new UserReviews
                                           {
                                               CoachId = y.CoachId,
                                               ClassId = y.GroupClassId,
                                               Type = ReviewType.GroupClass,
                                               UserId = x.ParticipantId,
                                               Coach = participant.FirstName,
                                               CoachImage = participant.ImageUrl,
                                           }).ToListAsync();

                var individualResponse = await (from x in DbContext.IndividualBookings
                                                join y in DbContext.IndividualClasses
                                                    on x.ClassId equals y.ClassId
                                                join z in DbContext.Users
                                                   on y.CoachId equals z.UserId
                                                where x.Status == EBookingStatus.Approved && x.TraineeId == currentLogin
                                                 && x.CreatedDate < DateTime.Now
                                                select new UserReviews
                                                {
                                                    CoachId = y.CoachId,
                                                    ClassId = y.ClassId,
                                                    Type = ReviewType.IndividualClass,
                                                    UserId = x.TraineeId,
                                                    Coach = z.FirstName,
                                                    CoachImage = z.ImageUrl
                                                }).ToListAsync();

                var activityLists = individualResponse.Union(groupResponse);

                List<UserReview> reviewList = await DbContext.UserReviews.ToListAsync();

                foreach (var item in activityLists)
                {
                    if (response.Count < 1)
                    {
                        var output = reviewList.Where(x => x.CoachId == item.CoachId && x.ClassId == item.ClassId && x.Type == item.Type).ToList();
                        if (output.Count <= 0)
                        {
                            response.Add(item);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return new APIResponse
                {
                    Message = "Retrieved Coach to Reviews",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetCoachToReview --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetUserReviews()
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                List<UserReviewList> response = await (from x in DbContext.UserReviews
                                                       join y in DbContext.Users
                                                         on x.UserId equals y.UserId
                                                       where y.UserId == currentLogin
                                                       select new UserReviewList
                                                       {
                                                           Ratings = x.Ratings,
                                                           Description = x.Description,
                                                           Image = y.ImageUrl,
                                                           Date = x.CreatedDate,
                                                       }).ToListAsync();

                return new APIResponse
                {
                    Message = "Retrieved User Reviews",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }

            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetAllWhatsNew --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<Notation> GetEnumUserReviews(Guid userId)
        {
            var ratings = await (from x in DbContext.UserReviews
                                 join y in DbContext.Users
                                   on x.UserId equals y.UserId
                                 where y.UserId == userId && x.Type == ReviewType.IndividualClass
                                 select new UserReviewList
                                 {
                                     Ratings = x.Ratings,
                                     Description = x.Description,
                                     Name = y.FirstName,
                                     Image = y.ImageUrl,
                                     Date = x.CreatedDate,
                                 }).ToListAsync();

            List<NotationDetails> model = new List<NotationDetails>();
            decimal totalRatings = 0;
            foreach (var item in ratings)
            {
                NotationDetails List = new NotationDetails();
                List.Image = item.Image;
                List.Date = Convert.ToDateTime(item.Date).ToString("dd MMM yyyy");
                List.Ratings = item.Ratings;
                List.Name = item.Name;
                totalRatings += Convert.ToDecimal(((int)item.Ratings)) / 5;
                model.Add(List);
            }

            Notation response = new Notation();
            response.Details = model;
            if (model.Count > 0)
            {
                response.totalRatings = (totalRatings / Convert.ToDecimal(model.Count)) * 5;
            }
            else
            {
                response.totalRatings = 0;
            }

            return response;
        }

        public async Task<Notation> GetGroupUserReviews(Guid userId)
        {
            var ratings = await (from x in DbContext.UserReviews
                                 join y in DbContext.Users
                                   on x.UserId equals y.UserId
                                 where y.UserId == userId && x.Type == ReviewType.IndividualClass
                                 select new UserReviewList
                                 {
                                     Ratings = x.Ratings,
                                     Description = x.Description,
                                     Name = y.FirstName,
                                     Image = y.ImageUrl,
                                     Date = x.CreatedDate,
                                 }).ToListAsync();

            List<NotationDetails> model = new List<NotationDetails>();
            decimal totalRatings = 0;
            foreach (var item in ratings)
            {
                NotationDetails List = new NotationDetails();
                List.Image = item.Image;
                List.Date = Convert.ToDateTime(item.Date).ToString("dd MMM yyyy");
                List.Ratings = item.Ratings;
                List.Name = item.Name;
                totalRatings += Convert.ToDecimal(((int)item.Ratings)) / 5;
                model.Add(List);
            }

            Notation response = new Notation();
            response.Details = model;
            if (model.Count > 0)
            {
                response.totalRatings = (totalRatings / Convert.ToDecimal(model.Count)) * 5;
            }
            else
            {
                response.totalRatings = 0;
            }

            return response;
        }

        public static string GetTimeSince(DateTime objDateTime)
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime().Subtract(objDateTime);
            int intDays = ts.Days;
            int intHours = ts.Hours;
            int intMinutes = ts.Minutes;
            int intSeconds = ts.Seconds;

            if (intDays > 0)
                return string.Format("{0} days ago", intDays);

            if (intHours > 0)
                return string.Format("{0} hours ago", intHours);

            if (intMinutes > 0)
                return string.Format("{0} minutes ago", intMinutes);

            if (intSeconds > 0)
                return string.Format("{0} seconds ago", intSeconds);

            if (intDays < 0)
                return string.Format("{0} days", Math.Abs(intDays));

            if (intHours < 0)
                return string.Format("{0} hours", Math.Abs(intHours));

            if (intMinutes < 0)
                return string.Format("{0} minutes", Math.Abs(intMinutes));

            if (intSeconds < 0)
                return string.Format("{0} seconds", Math.Abs(intSeconds));

            return "a bit";
        }

    }
}
