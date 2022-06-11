using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using Sidekick.Model.Booking;
using Sidekick.Model.Class;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class CoachRepository : APIBaseRepo, ICoachRepository
    {
        readonly APIDBContext DbContext;
        private readonly IUserHelper userHelper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public CoachRepository(APIDBContext _dbCtxt,
            ILoggerManager _logManager,
            APIConfigurationManager _apiCon,
            IUserHelper userHelper,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            this.userHelper = userHelper;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<CoachViewModel>> GetCoaches()
        {
            return await (from x in DbContext.Coaches
                          join y in DbContext.Users
                            on x.CoachUserId equals y.UserId
                          select new CoachViewModel()
                          {
                              UserNo = x.Id,
                              ProfileName = string.Format("{0} {1}", y.FirstName, y.LastName),
                              Email = y.Email,
                              MobileNo = y.MobileNumber,
                              ImageUrl = y.ImageUrl,
                              //LastCoachingDate = null,
                              Status = x.IsEnabled.Value ? "Active" : "Inactive",
                              DateCreated = y.CreatedDate,
                              CoachUserId = x.CoachUserId.Value
                          })
                          .ToListAsync();
        }

        public APIResponse GetPlayers(int id)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("-- Run::CoachRepository::GetPlayers --");
            LogManager.LogDebugObject(id);

            try
            {
                if (id == 0)
                {
                    apiResp = new APIResponse
                    {
                        Message = "User successfully retrieved",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Status = "Success!",
                        Payload = DbContext.Users.ToList()
                    };
                }
                else
                {
                    User FoundUser = DbContext.Users.FirstOrDefault(u => u.Id == id && u.UserType == EUserType.NORMAL);
                    if (FoundUser == null)
                    {
                        apiResp = new APIResponse
                        {
                            Message = "Invalid PlayerId!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            Status = "Failed!"
                        };
                    }
                    else
                    {
                        apiResp = new APIResponse
                        {
                            Message = "GetPlayers successfully retrieved",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Status = "Success!",
                            Payload = new
                            {
                                UserProfile = FoundUser,
                                UserLocation = DbContext.UserAddresses.FirstOrDefault(ua => ua.UserId == FoundUser.UserId)
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::CoachRepository::GetPlayers --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public APIResponse SetPlayerToCoach(string auth, int id)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::CoachRepository::SetPlayerToCoach --");
            LogManager.LogDebugObject(auth);
            LogManager.LogDebugObject(id);

            try
            {
                //get user login creds
                AdminLoginTransaction aLT = DbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (aLT != null)
                {
                    LogManager.LogInfo("Valid Auth token, proceeding to SetPlayerToCoach userid: " + id.ToString());
                    // get user profile
                    User userToUpdateToCoach = DbContext.Users.FirstOrDefault(u => u.Id == id);
                    if (userToUpdateToCoach != null)
                    {
                        if (userToUpdateToCoach.UserType == EUserType.NORMAL)
                        {
                            userToUpdateToCoach.UserType = EUserType.NORMALANDCOACH;
                            userToUpdateToCoach.LastEditedBy = aLT.AdminId;
                            userToUpdateToCoach.LastEditedDate = DateTime.Now;

                            DbContext.Update(userToUpdateToCoach);
                            DbContext.SaveChanges();

                            LogManager.LogInfo("User: " + userToUpdateToCoach.UserId.ToString() + " set to coach by admin " + aLT.AdminId.ToString());
                            apiResp.Message = userToUpdateToCoach.FirstName + " " + userToUpdateToCoach.LastName + " set to coach status!";
                            apiResp.Status = "Success!";
                            apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                        else // user is already a coach
                        {
                            LogManager.LogError("User is already a coach! user id(" + id.ToString() + ") ! by admin " + aLT.AdminId.ToString());
                            apiResp.Message = "User is already a Coach(" + id.ToString() + ")! Cannot set to coach!";
                            apiResp.Status = "Failed!";
                            apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        }
                    }
                    else  // no user found
                    {
                        LogManager.LogError("Invalid user id : " + id.ToString() + "! by admin " + aLT.AdminId.ToString());
                        apiResp.Message = "Invalid User(" + id.ToString() + ")! Cannot set to coach!";
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
                LogManager.LogInfo("-- Error::CoachRepository::SetPlayerToCoach --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;

        }

        public async Task<CoachInfoViewModel> GetCoache(Guid userId)
        {
            return await (from x in DbContext.Users
                          join y in DbContext.Coaches on
                          x.UserId equals y.CoachUserId
                          join userAdd in DbContext.UserAddresses on
                          x.UserId equals userAdd.UserId
                          into UserAddressGroup
                          from UserAddresses in UserAddressGroup.DefaultIfEmpty()
                          where x.UserId == userId
                          select new CoachInfoViewModel()
                          {
                              ImageUrl = x.ImageUrl,
                              FirstName = x.FirstName,
                              LastName = x.LastName,
                              Email = x.Email,
                              mobile = x.MobileNumber,
                              Street = UserAddresses.Street,
                              City = UserAddresses.City,
                              Country = UserAddresses.CountryName,
                              AreaId = UserAddresses.AreaId,
                              countryAlpha3Code = UserAddresses.CountryAlpha3Code,
                              DateOfBirth = x.DateOfBirth,
                              Languages = (from coachLanguage in DbContext.CoachLanguages
                                           join language in DbContext.Languages
                                              on coachLanguage.LanguageId equals language.LanguageId
                                           where coachLanguage.CoachUserId == x.UserId
                                           select new LanguageViewModel
                                           {
                                               LanguageId = language.LanguageId,
                                               Language = language._Language,
                                               Image = language.Icon
                                           })
                       .ToList(),
                              gymAccesses = (from coachGym in DbContext.CoachGyms
                                             join gym in DbContext.Gyms
                                                 on coachGym.GymID equals gym.GymId
                                             where coachGym.CoachUserId == x.UserId
                                             select new GymAccessViewModel
                                             {
                                                 GymId = gym.GymId,
                                                 Gym = gym.GymName,
                                                 Image = gym.Icon
                                             })
                  .ToList(),
                              Expert = (from coachSpeciality in DbContext.CoachSpecialties
                                        join specialty in DbContext.Specialties
                                         on coachSpeciality.SpecialtyId equals specialty.SpecialtyId
                                        where coachSpeciality.CoachUserId == x.UserId
                                        select new ExpertViewModel
                                        {
                                            ExpertId = specialty.SpecialtyId,
                                            Name = specialty.Name
                                        })
                        .ToList(),
                              Description = x.Description
                          }).FirstOrDefaultAsync();
        }

        public APIResponse BecomeACoach(CoachUpdateProfile updateProf)
        {
            APIResponse apiResp = new APIResponse();

            LogManager.LogInfo("-- Run::CoachRepository::BecomeACoach --");
            LogManager.LogDebugObject(updateProf);

            try
            {
                Guid? Updater = userHelper.GetCurrentUserGuidLogin();
                if (Updater == null)
                {
                    return new APIResponse
                    {
                        Message = "Error Becoming a coach!",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                unitOfWork.BeginTransaction();
                User FoundUser = DbContext.Users.FirstOrDefault(u => u.UserId == Updater);

                if (FoundUser != null)
                {
                    FoundUser.LastEditedBy = Updater;
                    FoundUser.LastEditedDate = DateTime.Now;
                    FoundUser.DateOfBirth = updateProf.DateOfBirth;
                    FoundUser.Description = updateProf.Description;
                    FoundUser.Gender = updateProf.Gender;
                    FoundUser.UserType = EUserType.NORMALANDCOACH;

                    // check coach
                    Coach DoesCoachExist = DbContext.Coaches.FirstOrDefault(cid => cid.CoachUserId == FoundUser.UserId);
                    if (DoesCoachExist != null)
                    {
                        unitOfWork.RollbackTransaction();
                        return new APIResponse
                        {
                            Message = Messages.AlreadyACoach,
                            Status = Status.Failed,
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                        };
                    }

                    // setup new coach
                    Coach newC = new Coach
                    {
                        CreatedDate = DateTime.Now,
                        CoachUserId = FoundUser.UserId,
                        CreatedBy = FoundUser.UserId,
                        DateEnabled = DateTime.Now,
                        Description = updateProf.Description,
                        Experience = updateProf.Experience,
                        IsEnabled = true,
                        IsEnabledBy = FoundUser.UserId,
                        IsLocked = false,
                        LastEditedBy = FoundUser.UserId,
                        LastEditedDate = DateTime.Now,
                        LockedDateTime = null,
                        Location = updateProf.Location
                    };
                    DbContext.Coaches.Add(newC);

                    // setup specialties
                    if (updateProf.Specialties != null)
                    {
                        //foreach (string specsStr in updateProf.Specialty)
                        //{
                        //    CoachSpecialty specs = new CoachSpecialty();
                        //    specs.CoachUserId = FoundUser.UserId;
                        //    specs.CreatedBy = FoundUser.UserId;
                        //    specs.CreatedDate = DateTime.Now;
                        //    specs.DateEnabled = DateTime.Now;
                        //    specs.IsEnabled = true;
                        //    specs.IsEnabledBy = FoundUser.UserId;
                        //    specs.IsLocked = false;
                        //    specs.LastEditedBy = FoundUser.UserId;
                        //    specs.LastEditedDate = DateTime.Now;
                        //    specs.LockedDateTime = null;
                        //    DbContext.CoachSpecialties.Add(specs);
                        //}
                    }

                    //setup languages
                    if (updateProf.Languages != null)
                    {
                        List<CoachLanguage> cLang = DbContext.CoachLanguages.Where(cl => cl.CoachUserId == Updater).ToList();
                        DbContext.RemoveRange(cLang);
                        //foreach (string langStr in updateProf.Language)
                        //{

                        //    CoachLanguage lang = new CoachLanguage();
                        //    lang.CoachUserId = FoundUser.UserId;
                        //    lang.CreatedBy = FoundUser.UserId;
                        //    lang.CreatedDate = DateTime.Now;
                        //    lang.DateEnabled = DateTime.Now;
                        //    lang.IsEnabled = true;
                        //    lang.IsEnabledBy = FoundUser.UserId;
                        //    lang.IsLocked = false;
                        //    lang.LastEditedBy = FoundUser.UserId;
                        //    lang.LastEditedDate = DateTime.Now;
                        //    lang.LockedDateTime = null;
                        //    DbContext.CoachLanguages.Add(lang);
                        //}
                    }

                    //setup languages
                    if (updateProf.GymsAccess != null)
                    {
                        List<CoachGymAccess> gymAcc = DbContext.CoachGyms.Where(cg => cg.CoachUserId == Updater).ToList();
                        DbContext.RemoveRange(gymAcc);
                        foreach (Guid gymInt in updateProf.GymsAccess)
                        {

                            CoachGymAccess gym = new CoachGymAccess();
                            gym.CoachUserId = FoundUser.UserId;
                            gym.CreatedBy = FoundUser.UserId;
                            gym.CreatedDate = DateTime.Now;
                            gym.DateEnabled = DateTime.Now;
                            gym.IsEnabled = true;
                            gym.IsEnabledBy = FoundUser.UserId;
                            gym.IsLocked = false;
                            gym.LastEditedBy = FoundUser.UserId;
                            gym.LastEditedDate = DateTime.Now;
                            gym.LockedDateTime = null;
                            gym.GymID = gymInt;
                            DbContext.CoachGyms.Add(gym);
                        }
                    }

                    //sched
                    if (updateProf.CustomSched != null)
                    {
                        List<CoachCustomSchedule> oldCSched = DbContext.CoachCustomSchedules.Where(sss => sss.CoachId == FoundUser.UserId).ToList();
                        DbContext.RemoveRange(oldCSched);
                        foreach (CoachCustomScheduleViewModel sched in updateProf.CustomSched)
                        {
                            CoachCustomSchedule cSched = new CoachCustomSchedule();
                            cSched.CoachId = FoundUser.UserId;
                            cSched.CreatedBy = FoundUser.UserId;
                            cSched.CreatedDate = DateTime.Now;
                            cSched.DateEnabled = DateTime.Now;
                            cSched.IsEnabled = true;
                            cSched.IsEnabledBy = FoundUser.UserId;
                            cSched.IsLocked = false;
                            cSched.LastEditedBy = FoundUser.UserId;
                            cSched.LastEditedDate = DateTime.Now;
                            cSched.LockedDateTime = null;
                            cSched.Day = sched.Day;
                            cSched.StartTime = sched.StartTime;
                            cSched.EndTime = sched.EndTime;
                            DbContext.CoachCustomSchedules.Add(cSched);
                        }
                    }

                    DbContext.Update(FoundUser);
                    DbContext.SaveChanges();

                    apiResp.Message = "Become A Coach Success!";
                    apiResp.Status = "Success!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    apiResp.Payload = updateProf;
                    unitOfWork.RollbackTransaction();
                }
                else
                {
                    apiResp.Message = "Invalid user profile!";
                    apiResp.Status = "Failed!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    apiResp.Payload = updateProf;
                    unitOfWork.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                LogManager.LogInfo("-- Error::CoachRepository::BecomeACoach --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public CoachProfile GetCoachProfile(Guid CoachId)
        {
            Coach GetCoach = DbContext.Coaches.FirstOrDefault(c => c.CoachUserId == CoachId);
            if (GetCoach != null)
            {
                var mappedCoachProfile = mapper.Map<CoachProfile>(GetCoach);
                mappedCoachProfile.Gyms = DbContext.CoachGyms.Where(cg => cg.CoachUserId == CoachId).ToList();
                mappedCoachProfile.Languages = DbContext.CoachLanguages.Where(cl => cl.CoachUserId == CoachId).ToList();
                mappedCoachProfile.Specialties = DbContext.CoachSpecialties.Where(cs => cs.CoachUserId == CoachId).ToList();
                return mappedCoachProfile;
            }
            else
            {
                return new CoachProfile();
            }
        }

        public async Task<APIResponse> GetCoachProfileView(Guid CoachId)
        {
            CoachProfileView response = new CoachProfileView();
            response.Profile = await (from x in DbContext.Coaches
                                      join y in DbContext.Users
                                        on x.CoachUserId equals y.UserId
                                      where x.CoachUserId == CoachId
                                      select new CoachDetail
                                      {
                                          UserID = y.Id,
                                          ImageUrl = y.ImageUrl,
                                          ProfileName = y.FirstName + " " + y.LastName,
                                          Experience = x.Experience,
                                          Location = x.Location,
                                          LocationLat = x.LocationLat,
                                          LocationLong = x.LocationLong,
                                          NationalityId = y.NationalityId,
                                          Description = x.Description,
                                          Age = DateTime.Today.Year - Convert.ToDateTime(y.DateOfBirth).Year
                                      }).FirstOrDefaultAsync();
            if (response.Profile != null)
            {
                var friend = await DbContext.UserFriends.Where(x => x.UserId == response.Profile.UserID).ToListAsync();
                var bookings = await DbContext.IndividualClasses.Where(x => x.CoachId == CoachId && x.Price > 0).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                response.Profile.FriendCount = friend.Count;

                var ratings = await (from x in DbContext.UserReviews
                                     join y in DbContext.Users
                                       on x.UserId equals y.UserId
                                     where x.CoachId == CoachId
                                     select new UserReviewList
                                     {
                                         Ratings = x.Ratings,
                                         Description = x.Description,
                                         Name = y.FirstName,
                                         Image = y.ImageUrl,
                                         Date = x.CreatedDate,
                                     }).ToListAsync();

                var specialties = await (from x in DbContext.CoachSpecialties
                                         join y in DbContext.Specialties
                                           on x.SpecialtyId equals y.SpecialtyId
                                         where x.CoachUserId == CoachId
                                         select new CoachingSpecialties
                                         {
                                             Icon = y.Icon,
                                             Name = y.Name,
                                         }).ToListAsync();

                var badges = await (from x in DbContext.UserTrainBadges
                                    join y in DbContext.Specialties
                                      on x.SpecialtyId equals y.SpecialtyId
                                    where x.UserId == CoachId
                                    select new CoachingBadges
                                    {
                                        Icon = y.Icon,
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
                Notation notation = new Notation();
                response.Profile.CoachingPrice = bookings.Price;
                response.Profile.Duration = 2;
                response.Profile.ParticipantOffer = Convert.ToInt32(bookings.ParticipateToOffer);
                notation.totalRatings = totalRatings;
                response.Profile.Rating = totalRatings;
                notation.Details = model;
                response.Badges = badges;
                response.Specialties = specialties;
                response.Rating = notation;

                return new APIResponse
                {
                    Message = "Retrieved Coach Profile",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }
            else
            {
                return new APIResponse
                {
                    Message = "No Data",
                    Payload = "",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Failed!"
                };
            }
        }

        public async Task<Coach> GetCoach(Guid coachId)
        {
            return await DbContext.Coaches
                .Where(x => x.CoachUserId == coachId)
                .FirstOrDefaultAsync();
        }

        public async Task InsertUpdateCoach(Guid userId,
            CoachUpdateProfile coachProfile)
        {
            var getCoach = await GetCoach(userId);
            if (getCoach != null)
            {
                getCoach.Description = coachProfile.Description;
                getCoach.Experience = coachProfile.Experience;
                getCoach.Location = coachProfile.Location;
                getCoach.IsEnabled = coachProfile.IsActive;
                DbContext.Update(getCoach);
            }
            else
            {
                Coach newC = new Coach
                {
                    CreatedDate = DateTime.Now,
                    CoachUserId = userId,
                    CreatedBy = userId,
                    DateEnabled = DateTime.Now,
                    Description = coachProfile.Description,
                    Experience = coachProfile.Experience,
                    IsEnabled = true,
                    IsEnabledBy = userId,
                    IsLocked = false,
                    LastEditedBy = userId,
                    LastEditedDate = DateTime.Now,
                    Location = coachProfile.Location
                };
                DbContext.Coaches.Add(newC);
            }
            await DbContext.SaveChangesAsync();
        }

        public async Task UpdateUserFromCoachProfile(Guid userId,
            DateTime dateOfBirth,
            string description,
            Genders gender
            /*string mobileNumber*/)
        {
            var getUser = await DbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId);
            if (getUser != null)
            {
                getUser.LastEditedBy = userId;
                getUser.LastEditedDate = DateTime.Now;
                getUser.DateOfBirth = dateOfBirth;
                getUser.Description = description;
                getUser.Gender = gender;
                getUser.UserType = EUserType.NORMALANDCOACH;
                //getUser.MobileNumber = mobileNumber;
                DbContext.Users.Update(getUser);
                await DbContext.SaveChangesAsync();
            }
        }

        // Specialty
        public async Task<IEnumerable<CoachSpecialty>> GetSpecialties(Guid userId)
        {
            return await DbContext.CoachSpecialties
                .Where(x => x.CoachUserId == userId)
                .ToListAsync();
        }

        public async Task InsertSpecialties(Guid userId,
            IEnumerable<Guid> specialties)
        {
            if (specialties.Any())
            {
                foreach (var specialty in specialties)
                {
                    await DbContext.CoachSpecialties.AddAsync(new CoachSpecialty()
                    {
                        CoachUserId = userId,
                        SpecialtyId = specialty,
                        CreatedBy = userId,
                        CreatedDate = Helper.GetDateTime()
                    });
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteSpecialties(IEnumerable<CoachSpecialty> specialties)
        {
            if (specialties.Any())
            {
                DbContext.RemoveRange(specialties);
                await DbContext.SaveChangesAsync();
            }
        }

        // Language
        public async Task<IEnumerable<CoachLanguage>> GetLanguages(Guid userId)
        {
            return await DbContext.CoachLanguages
                .Where(x => x.CoachUserId == userId)
                .ToListAsync();
        }

        public async Task InsertLanguages(Guid userId,
            IEnumerable<Guid> languages)
        {
            if (languages.Any())
            {
                foreach (var language in languages)
                {
                    await DbContext.CoachLanguages.AddAsync(new CoachLanguage()
                    {
                        CoachUserId = userId,
                        LanguageId = language,
                        CreatedBy = userId,
                        CreatedDate = Helper.GetDateTime()
                    });
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteLanguages(IEnumerable<CoachLanguage> languages)
        {
            if (languages.Any())
            {
                DbContext.RemoveRange(languages);
                await DbContext.SaveChangesAsync();
            }
        }
        // Gyms
        public async Task<IEnumerable<CoachGymAccess>> GetGymsAccess(Guid userId)
        {
            return await DbContext.CoachGyms
                .Where(x => x.CoachUserId == userId)
                .ToListAsync();
        }

        public async Task InsertGymsAccess(Guid userId,
            IEnumerable<Guid> coachGymsAccess)
        {
            if (coachGymsAccess.Any())
            {
                var list = new List<CoachGymAccess>();
                foreach (var coachGymAccess in coachGymsAccess)
                {
                    await DbContext.CoachGyms.AddAsync(new CoachGymAccess()
                    {
                        CoachUserId = userId,
                        GymID = coachGymAccess,
                        CreatedBy = userId,
                        CreatedDate = Helper.GetDateTime()
                    });
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteGymsAccess(IEnumerable<CoachGymAccess> coachGymsAccess)
        {
            if (coachGymsAccess.Any())
            {
                DbContext.RemoveRange(coachGymsAccess);
                await DbContext.SaveChangesAsync();
            }
        }

        // Schedule
        public async Task<CoachEverydaySchedule> GetEverydaySchedule(Guid userId)
        {
            return await DbContext.CoachEverydaySchedules
                .Where(x => x.CoachId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task InsertEverydaySchedule(Guid userId,
            CoachEverydayScheduleViewModel schedule)
        {
            if (schedule != null)
            {
                DbContext.Add(new CoachEverydaySchedule()
                {
                    CoachId = userId,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    CreatedBy = userId,
                    CreatedDate = Helper.GetDateTime(),
                    //PricePerHour = schedule.PricePerHour
                });
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteEverydaySchedule(CoachEverydaySchedule schedule)
        {
            if (schedule != null)
            {
                DbContext.Remove(schedule);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CoachCustomSchedule>> GetCustomSchedule(Guid userId)
        {
            return await DbContext
                .CoachCustomSchedules
                .Where(x => x.CoachId == userId)
                .ToListAsync();
        }

        public async Task InsertCustomSchedule(Guid userId,
            IEnumerable<CoachCustomScheduleViewModel> schedules)
        {
            if (schedules.Any())
            {
                foreach (var schedule in schedules)
                {
                    DbContext.CoachCustomSchedules.Add(new CoachCustomSchedule()
                    {
                        CoachId = userId,
                        Day = schedule.Day,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        CreatedBy = userId,
                        CreatedDate = Helper.GetDateTime(),
                        //PricePerHour = schedule.PricePerHour
                    });
                }
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteCustomSchedule(IEnumerable<CoachCustomSchedule> schedules)
        {
            if (schedules.Any())
            {
                DbContext.RemoveRange(schedules);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<TrainingBooking>> GetTrainingBookings()
        {
            var trainingBookings = await DbContext.TrainingBookings/*Include(t => t.TrainingTiming)*/.ToListAsync();

            if (trainingBookings.Any())
            {
                foreach (var trainingBooking in trainingBookings)
                {
                    var coachName = DbContext.Coaches.Where(c => c.CoachUserId == trainingBooking.CoachId).FirstOrDefault();
                    if (coachName != null)
                    {
                        trainingBooking.CoachName = coachName.Name;
                    }
                }
            }

            return trainingBookings;
        }

        public async Task<CoachScheduleViewModel> GetCoachSchedule(Guid userId)
        {
            var customSched = await GetCustomSchedule(userId);
            var everydaySched = await GetEverydaySchedule(userId);
            var mappedCustomSched = mapper.Map<IEnumerable<CoachCustomScheduleViewModel>>(customSched);
            var mappedEverydaySched = mapper.Map<CoachEverydayScheduleViewModel>(everydaySched);
            var schedule = new CoachScheduleViewModel()
            {
                CustomSchedule = mappedCustomSched,
                EverydaySchedule = mappedEverydaySched,
                IsWeekPersonalized = mappedCustomSched.Any()
            };
            return schedule;
        }

        public async Task<CoachProfileViewModel> GetCoacheProfile(Guid userId)
        {
            var CoachLanguages = DbContext.Languages.Where(item => DbContext.CoachLanguages.Where(item => item.CoachUserId == userId).Select(item => item.LanguageId).Contains(item.LanguageId)).Select(item => item.Icon).ToList();
            var PriceHours = DbContext.IndividualClassDetails.Where(item => DbContext.IndividualClasses.Where(item => item.CoachId == userId).Select(item => item.ClassId).Contains(item.IndividualClassId)).Select(item => item.Price).FirstOrDefault();
            var Duration = (Convert.ToDateTime(DbContext.CoachEverydaySchedules.Where(item => item.CoachId == userId).Select(item => item.StartTime).FirstOrDefault()) - Convert.ToDateTime(DbContext.CoachEverydaySchedules.Where(item => item.CoachId == userId).Select(item => item.EndTime).FirstOrDefault())).ToString(@"hh\hmm");
            var Expert = DbContext.Specialties.Where(item => DbContext.CoachSpecialties.Where(item => item.CoachUserId == userId).Select(item => item.SpecialtyId).Contains(item.SpecialtyId)).Select(item => item.Name).ToList();
            var Badges = (from coachBadges in DbContext.UserPlayBadges
                          join playBadges in DbContext.Sports
                            on coachBadges.SportId equals playBadges.SportId
                          where coachBadges.UserId == userId
                          select playBadges.Name).ToList();
            return await (from x in DbContext.Users
                          join y in DbContext.Coaches
                 on x.UserId equals y.CoachUserId
                          join UserAddress in DbContext.UserAddresses
                            on x.UserId equals UserAddress.UserId
                          where x.UserId == userId
                          && x.IsEnabled == true
                          && x.IsLocked == false
                          select new CoachProfileViewModel()
                          {
                              ImageUrl = x.ImageUrl,
                              City = UserAddress.City,
                              FirstName = x.FirstName,
                              LastName = x.LastName,
                              Age = (x.DateOfBirth.HasValue ? DateTime.Now.Subtract(x.DateOfBirth.Value).Days / 365 : 0),
                              AreaId = UserAddress.AreaId,
                              CoachLanguages = CoachLanguages,
                              PriceHours = PriceHours,
                              Duration = Duration,
                              Description = x.Description,
                              Expert = Expert,
                              Badges = Badges
                          }).FirstOrDefaultAsync();
        }

        public APIResponse EditCoachProfile(CoachEditProfileFormModel _coach)
        {
            APIResponse apiResp = new APIResponse();
            DateTime TodaysDate = DateTime.Now;
            LogManager.LogInfo("-- Run::CoachRepository::Update --");
            LogManager.LogDebugObject(_coach);

            try
            {
                Guid? Updater = userHelper.GetCurrentUserGuidLogin();
                if (Updater == null)
                {
                    return new APIResponse
                    {
                        Message = "Error Edit Profile!",
                        Status = "Failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                unitOfWork.BeginTransaction();
                User FoundUser = DbContext.Users.FirstOrDefault(u => u.UserId == Updater);
                if (FoundUser != null)
                {
                    FoundUser.LastEditedBy = Updater;
                    FoundUser.LastEditedDate = DateTime.Now;
                    FoundUser.DateOfBirth = _coach.DateOfBirth;
                    FoundUser.Description = _coach.Description;
                    FoundUser.FirstName = _coach.FirstName;
                    FoundUser.LastName = _coach.LastName;
                    FoundUser.Email = _coach.Email;
                    FoundUser.ImageUrl = _coach.ImageUrl;
                    FoundUser.MobileNumber = _coach.mobile;
                    FoundUser.UserType = EUserType.NORMALANDCOACH;

                    UserAddress userAddress = DbContext.UserAddresses.FirstOrDefault(u => u.UserId == Updater);
                    if (userAddress != null)
                    {
                        userAddress.LastEditedBy = Updater;
                        userAddress.LastEditedDate = DateTime.Now;
                        userAddress.City = _coach.City;
                        userAddress.CountryAlpha3Code = _coach.CountryAlpha3Code;
                        userAddress.CountryName = _coach.Country;
                        userAddress.AreaId = _coach.AreaId;
                    }
                    // check coach
                    //Coach DoesCoachExist = DbContext.Coaches.FirstOrDefault(cid => cid.CoachUserId == FoundUser.UserId);
                    //if (DoesCoachExist != null)
                    //{
                    //    unitOfWork.RollbackTransaction();
                    //    return new APIResponse
                    //    {
                    //        Message = Messages.AlreadyACoach,
                    //        Status = Status.Failed,
                    //        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    //    };
                    //}

                    // setup new coach
                    Coach newC = new Coach
                    {
                        Description = _coach.Description,
                    };
                    if (FoundUser != null)
                        DbContext.Users.Update(FoundUser);
                    if (userAddress != null)
                        DbContext.UserAddresses.Update(userAddress);
                    if (newC != null)
                        DbContext.Coaches.Update(newC);

                    // setup specialties
                    if (_coach.Expert != null)
                    {
                        List<CoachSpecialty> expert = DbContext.CoachSpecialties.Where(cl => cl.CoachUserId == Updater).ToList();
                        DbContext.RemoveRange(expert);
                        foreach (var specsStr in _coach.Expert)
                        {
                            CoachSpecialty specs = new CoachSpecialty();
                            specs.CoachUserId = FoundUser.UserId;
                            specs.SpecialtyId = specsStr.ExpertId;
                            specs.CreatedBy = FoundUser.UserId;
                            specs.CreatedDate = DateTime.Now;
                            specs.DateEnabled = DateTime.Now;
                            specs.IsEnabled = true;
                            specs.IsEnabledBy = FoundUser.UserId;
                            specs.IsLocked = false;
                            specs.LastEditedBy = FoundUser.UserId;
                            specs.LastEditedDate = DateTime.Now;
                            specs.LockedDateTime = null;
                            DbContext.CoachSpecialties.Add(specs);
                        }
                    }

                    //setup languages
                    if (_coach.Languages != null)
                    {
                        List<CoachLanguage> cLang = DbContext.CoachLanguages.Where(cl => cl.CoachUserId == Updater).ToList();
                        DbContext.RemoveRange(cLang);
                        foreach (var langStr in _coach.Languages)
                        {

                            CoachLanguage lang = new CoachLanguage();
                            lang.CoachUserId = FoundUser.UserId;
                            lang.LanguageId = langStr.LanguageId;
                            lang.CreatedBy = FoundUser.UserId;
                            lang.CreatedDate = DateTime.Now;
                            lang.DateEnabled = DateTime.Now;
                            lang.IsEnabled = true;
                            lang.IsEnabledBy = FoundUser.UserId;
                            lang.IsLocked = false;
                            lang.LastEditedBy = FoundUser.UserId;
                            lang.LastEditedDate = DateTime.Now;
                            lang.LockedDateTime = null;
                            DbContext.CoachLanguages.Add(lang);
                        }
                    }

                    //setup GymAccesses
                    if (_coach.gymAccesses != null)
                    {
                        List<CoachGymAccess> gymAcc = DbContext.CoachGyms.Where(cg => cg.CoachUserId == Updater).ToList();
                        DbContext.RemoveRange(gymAcc);
                        foreach (var gymInt in _coach.gymAccesses)
                        {

                            CoachGymAccess gym = new CoachGymAccess();
                            gym.CoachUserId = FoundUser.UserId;
                            gym.CreatedBy = FoundUser.UserId;
                            gym.CreatedDate = DateTime.Now;
                            gym.DateEnabled = DateTime.Now;
                            gym.IsEnabled = true;
                            gym.IsEnabledBy = FoundUser.UserId;
                            gym.IsLocked = false;
                            gym.LastEditedBy = FoundUser.UserId;
                            gym.LastEditedDate = DateTime.Now;
                            gym.LockedDateTime = null;
                            gym.GymID = gymInt.GymId;
                            DbContext.CoachGyms.Add(gym);
                        }
                    }


                    DbContext.Update(FoundUser);
                    DbContext.SaveChanges();

                    apiResp.Message = "Edit Profile Success!";
                    apiResp.Status = "Success!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    apiResp.Payload = _coach;
                    unitOfWork.CommitTransaction();
                }
                else
                {
                    apiResp.Message = "Invalid user profile!";
                    apiResp.Status = "Failed!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    apiResp.Payload = _coach;
                    unitOfWork.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                LogManager.LogInfo("-- Error::CoachRepository::BecomeACoach --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<CoachProfileViewModel> MyCoachingGroupListViewModel(Guid userId)
        {
            return await (from x in DbContext.Users
                          join y in DbContext.Coaches
                 on x.UserId equals y.CoachUserId
                          join UserAddress in DbContext.UserAddresses
                            on x.UserId equals UserAddress.UserId
                          where x.UserId == userId
                          && x.IsEnabled == true
                          && x.IsLocked == false
                          select new CoachProfileViewModel()
                          {
                              ImageUrl = x.ImageUrl,
                              City = UserAddress.City,
                              FirstName = x.FirstName,
                              LastName = x.LastName,
                              Age = (x.DateOfBirth.HasValue ? DateTime.Now.Subtract(x.DateOfBirth.Value).Days / 365 : 0),
                              CoachLanguages = (DbContext.Languages.Where(item => DbContext.CoachLanguages.Where(item => item.CoachUserId == x.UserId).Select(item => item.LanguageId).Contains(item.LanguageId)).Select(item => item.Icon)).ToList(),
                              PriceHours = DbContext.IndividualClassDetails.Where(item => DbContext.IndividualClasses.Where(item => item.CoachId == x.UserId).Select(item => item.ClassId).Contains(item.IndividualClassId)).Select(item => item.Price).FirstOrDefault(),
                              Duration = (Convert.ToDateTime(DbContext.CoachEverydaySchedules.Where(item => item.CoachId == x.UserId).Select(item => item.StartTime).FirstOrDefault()) - Convert.ToDateTime(DbContext.CoachEverydaySchedules.Where(item => item.CoachId == x.UserId).Select(item => item.EndTime).FirstOrDefault())).ToString(@"hh\hmm"),
                              Description = x.Description,
                              Expert = DbContext.Specialties.Where(item => DbContext.CoachSpecialties.Where(item => item.CoachUserId == x.UserId).Select(item => item.SpecialtyId).Contains(item.SpecialtyId)).Select(item => item.Name).ToList(),
                              Badges = (from coachBadges in DbContext.UserPlayBadges
                                        join playBadges in DbContext.Sports
                                          on coachBadges.SportId equals playBadges.SportId
                                        where coachBadges.UserId == x.UserId
                                        select playBadges.Name).ToList()
                          }).FirstOrDefaultAsync();
        }

        public async Task<MyCoachingGroupListViewModel> GetMyCoachingGroupList(Guid userId, string title)
        {
            try
            {
                MyCoachingGroupListViewModel myCoachingGroupListViewModel = new MyCoachingGroupListViewModel();
                var upcomingClasse = await (from x in DbContext.GroupClasses
                                            join y in DbContext.GroupBookings
                                            on x.GroupClassId equals y.GroupClassId
                                            where x.CoachId == userId && x.Start.Value.Date >= DateTime.Now.Date
                                            && (title == null || x.Title.ToLower().Contains(title.ToLower()))
                                            select new UpcomingClassesViewModel()
                                            {
                                                Id = x.Id,
                                                BookingId = y.GroupBookingId,
                                                BookingType = ReviewType.GroupClass,
                                                price = x.Price,
                                                title = x.Title,
                                                createdDate = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("dd MMM, yyyy") : "",
                                                time = x.Start.Value.ToString("HH:mm") + '-' + x.End.Value.ToString("HH:mm")
                                            })
                          .ToListAsync();
                var coachingHistory = await (from x in DbContext.GroupClasses
                                             join y in DbContext.GroupBookings
                                             on x.GroupClassId equals y.GroupClassId
                                             where x.CoachId == userId && x.Start.Value.Date < DateTime.Now.Date
                                              && (title == null || x.Title.ToLower().Contains(title.ToLower()))
                                             select new CoachingHistoryViewModel()
                                             {
                                                 Id = x.Id,
                                                 BookingId = y.GroupBookingId,
                                                 BookingType = ReviewType.GroupClass,
                                                 price = x.Price,
                                                 title = x.Title,
                                                 createdDate = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("dd MMM, yyyy") : "",
                                                 time = x.Start.Value.ToString("HH:mm") + '-' + x.End.Value.ToString("HH:mm")
                                             })
                          .ToListAsync();
                myCoachingGroupListViewModel.upcomingClasses = mapper.Map<List<UpcomingClassesViewModel>>(upcomingClasse);
                myCoachingGroupListViewModel.coachingHistory = mapper.Map<List<CoachingHistoryViewModel>>(coachingHistory);
                return myCoachingGroupListViewModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MyIndividualGroupListViewModel> GetMyIndividualGroupList(Guid userId, string name)
        {
            try
            {
                MyIndividualGroupListViewModel myCoachingGroupListViewModel = new MyIndividualGroupListViewModel();
                var upcomingClasse = await (from x in DbContext.CoachEverydaySchedules
                                            join y in DbContext.Users
                                            on x.CoachId equals y.UserId
                                            join a in DbContext.IndividualClasses on x.CoachId equals a.CoachId
                                            join b in DbContext.IndividualBookings on a.ClassId equals b.ClassId
                                            where x.CoachId == userId && x.CreatedDate > DateTime.Now
                                            && (name == null || (y.FirstName.Trim() + ' ' + y.LastName.Trim()).ToLower().Trim().Contains(name.ToLower()))
                                            select new IndividualUpcomingClassesViewModel()
                                            {
                                                Id = a.Id,
                                                BookingId = b.BookingId,
                                                BookingType = ReviewType.IndividualClass,
                                                title = y.FirstName + ' ' + y.LastName,
                                                createdDate = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("dd MMM, yyyy") : "",
                                                time = !string.IsNullOrEmpty(x.StartTime) && !string.IsNullOrEmpty(x.EndTime) ? x.StartTime.Substring(0, 5) + '-' + x.EndTime.Substring(0, 5) : ""
                                            })
                          .ToListAsync();
                var coachingHistory = await (from x in DbContext.CoachEverydaySchedules
                                             join y in DbContext.Users
                                             on x.CoachId equals y.UserId
                                             join a in DbContext.IndividualClasses on x.CoachId equals a.CoachId
                                             join b in DbContext.IndividualBookings on a.ClassId equals b.ClassId
                                             where x.CoachId == userId && x.CreatedDate < DateTime.Now
                                             && (name == null || (y.FirstName.Trim() + ' ' + y.LastName.Trim()).ToLower().Trim().Contains(name.ToLower()))
                                             select new IndividualCoachingHistoryViewModel()
                                             {
                                                 Id = a.Id,
                                                 BookingId = b.BookingId,
                                                 BookingType = ReviewType.IndividualClass,
                                                 title = y.FirstName + ' ' + y.LastName,
                                                 createdDate = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("dd MMM, yyyy") : "",
                                                 time = !string.IsNullOrEmpty(x.StartTime) && !string.IsNullOrEmpty(x.EndTime) ? x.StartTime.Substring(0, 5) + '-' + x.EndTime.Substring(0, 5) : ""
                                             })
                          .ToListAsync();
                myCoachingGroupListViewModel.upcomingClasses = mapper.Map<List<IndividualUpcomingClassesViewModel>>(upcomingClasse);
                myCoachingGroupListViewModel.coachingHistory = mapper.Map<List<IndividualCoachingHistoryViewModel>>(coachingHistory);
                return myCoachingGroupListViewModel;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<CoachAbsentSlotViewModel>> GetCoachAbsentSlot(Guid userId)
        {
            try
            {
                var noLocationCoachAbsences = await (from x in DbContext.CoachAbsences
                                                     join y in DbContext.Users
                                                     on x.CoachUserId equals y.UserId
                                                     //where x.CoachUserId == userId && x.StartDate.Date >= DateTime.Now.Date.AddDays(-7) && x.StartDate.Date <= DateTime.Now.Date && x.LocationId == null
                                                     where x.CoachUserId == userId && x.LocationId == null
                                                     select new GetCoachAbsentSlotViewModel()
                                                     {
                                                         id = x.Id,
                                                         title = x.Title,
                                                         time = x.StartTime != "" ? x.StartTime + "-" + x.EndTime : "",
                                                         startdate = x.StartDate,
                                                         enddate = x.EndDate,
                                                         city = "",
                                                         day = x.StartDate.DayOfWeek.ToString(),
                                                         type = (x.RepeatWeek == 0 ? "Once" : "Punctual")
                                                     }).ToListAsync();

                var hasLocationCoachAbsences = await (from x in DbContext.CoachAbsences
                                                      join y in DbContext.Users
                                                      on x.CoachUserId equals y.UserId
                                                      join z in DbContext.Gyms
                                                      on x.LocationId equals z.GymId
                                                      //where x.CoachUserId == userId && x.StartDate.Date >= DateTime.Now.Date.AddDays(-7) && x.StartDate.Date <= DateTime.Now.Date
                                                      where x.CoachUserId == userId
                                                      select new GetCoachAbsentSlotViewModel()
                                                      {
                                                          id = x.Id,
                                                          title = x.Title,
                                                          time = x.StartTime != "" ? x.StartTime + "-" + x.EndTime : "",
                                                          startdate = x.StartDate,
                                                          enddate = x.EndDate,
                                                          //city = z.GymName,
                                                          city = "",
                                                          day = x.StartDate.DayOfWeek.ToString(),
                                                          type = (x.RepeatWeek == 0 ? "Once" : "Punctual")
                                                      }).ToListAsync();
                var listAbsentSlot = noLocationCoachAbsences.Union(hasLocationCoachAbsences);

                List<CoachAbsentSlotViewModel> coachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> sundayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> mondayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> tuesdayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> wednesdayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> thusdayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> fridayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                List<CoachAbsentSlotViewModel> saturdayCoachAbsentSlotViewModels = new List<CoachAbsentSlotViewModel>();
                CoachAbsentSlotViewModel thusdayInnercoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                CoachAbsentSlotViewModel sundayInnerCoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                CoachAbsentSlotViewModel mondayInnercoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                CoachAbsentSlotViewModel tuesdayInnercoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                CoachAbsentSlotViewModel wednesdayInnercoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                CoachAbsentSlotViewModel fridayInnercoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                CoachAbsentSlotViewModel saturdayInnercoachAbsentSlotViewModels = new CoachAbsentSlotViewModel();
                foreach (var item in listAbsentSlot)
                {
                    if (item.day == "Sunday")
                    {
                        sundayInnerCoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            sundayInnerCoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            sundayInnerCoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        sundayCoachAbsentSlotViewModels.Add(sundayInnerCoachAbsentSlotViewModels);
                    }
                    else if (item.day == "Monday")
                    {
                        mondayInnercoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            mondayInnercoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            mondayInnercoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        mondayCoachAbsentSlotViewModels.Add(mondayInnercoachAbsentSlotViewModels);
                    }
                    else if (item.day == "Tuesday")
                    {

                        tuesdayInnercoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            tuesdayInnercoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            tuesdayInnercoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        tuesdayCoachAbsentSlotViewModels.Add(tuesdayInnercoachAbsentSlotViewModels);
                    }
                    else if (item.day == "Wednesday")
                    {
                        wednesdayInnercoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            wednesdayInnercoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            wednesdayInnercoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        wednesdayCoachAbsentSlotViewModels.Add(wednesdayInnercoachAbsentSlotViewModels);
                    }
                    else if (item.day == "Thursday")
                    {
                        thusdayInnercoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            thusdayInnercoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            thusdayInnercoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        thusdayCoachAbsentSlotViewModels.Add(thusdayInnercoachAbsentSlotViewModels);
                    }
                    else if (item.day == "Friday")
                    {
                        fridayInnercoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            fridayInnercoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            fridayInnercoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        fridayCoachAbsentSlotViewModels.Add(fridayInnercoachAbsentSlotViewModels);
                    }
                    else if (item.day == "Saturday")
                    {
                        saturdayInnercoachAbsentSlotViewModels.day = item.day;
                        if (item.type == "Once")
                        {
                            OnceViewModel onceViewModel = new OnceViewModel();
                            onceViewModel.id = item.id;
                            onceViewModel.title = item.title;
                            onceViewModel.startdate = item.startdate;
                            onceViewModel.enddate = item.enddate;
                            onceViewModel.city = item.city;
                            onceViewModel.time = item.time;
                            saturdayInnercoachAbsentSlotViewModels.once.Add(onceViewModel);
                        }
                        else
                        {
                            PunctualViewModel punctualViewModel = new PunctualViewModel();
                            punctualViewModel.id = item.id;
                            punctualViewModel.title = item.title;
                            punctualViewModel.startdate = item.startdate;
                            punctualViewModel.enddate = item.enddate;
                            punctualViewModel.city = item.city;
                            punctualViewModel.time = item.time;
                            saturdayInnercoachAbsentSlotViewModels.punctual.Add(punctualViewModel);
                        }
                        saturdayCoachAbsentSlotViewModels.Add(saturdayInnercoachAbsentSlotViewModels);
                    }
                }
                // coachAbsentSlotViewModels = sundayCoachAbsentSlotViewModels.Distinct().Concat(mondayCoachAbsentSlotViewModels).Concat(tuesdayCoachAbsentSlotViewModels).Concat(thusdayCoachAbsentSlotViewModels).Concat(wednesdayCoachAbsentSlotViewModels).Concat(fridayCoachAbsentSlotViewModels).Concat(saturdayCoachAbsentSlotViewModels).ToList();
                if (sundayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(sundayCoachAbsentSlotViewModels.FirstOrDefault());
                if (mondayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(mondayCoachAbsentSlotViewModels.FirstOrDefault());
                if (tuesdayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(tuesdayCoachAbsentSlotViewModels.FirstOrDefault());
                if (wednesdayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(wednesdayCoachAbsentSlotViewModels.FirstOrDefault());
                if (thusdayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(thusdayCoachAbsentSlotViewModels.FirstOrDefault());
                if (fridayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(fridayCoachAbsentSlotViewModels.FirstOrDefault());
                if (saturdayCoachAbsentSlotViewModels.Count() > 0)
                    coachAbsentSlotViewModels.Add(saturdayCoachAbsentSlotViewModels.FirstOrDefault());
                return coachAbsentSlotViewModels;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<GetCoachAbsentSlotDetailsViewModel> GetCoachAbsentSlotDetails(int id)
        {
            try
            {
                var uniqueId = DbContext.CoachAbsences.Where(item => item.Id == id).Select(item => item.UniqueID).FirstOrDefault();

                var coachAbsences = await DbContext.CoachAbsences.Where(x => x.Id == id).FirstOrDefaultAsync();

                var MinDate = (from d in DbContext.CoachAbsences where d.UniqueID == uniqueId select d.StartDate).Min();
                var MaxDate = (from d in DbContext.CoachAbsences where d.UniqueID == uniqueId select d.EndDate).Max();

                if (coachAbsences.LocationId != null)
                {
                    var listAbsentSlot = await (from x in DbContext.CoachAbsences
                                                join y in DbContext.Gyms on
                                                x.LocationId equals y.GymId
                                                where x.Id == id
                                                select new GetCoachAbsentSlotDetailsViewModel()
                                                {
                                                    id = x.Id,
                                                    allDays = x.AllDay,
                                                    startdate = MinDate,
                                                    enddate = MaxDate,
                                                    starttime = x.StartTime,
                                                    endtime = x.EndTime,
                                                    type = x.Type,
                                                    title = x.Title,
                                                    locationId = x.LocationId.Value,
                                                    location = y.GymName,
                                                    note = x.Note,
                                                    repeatWeek = x.RepeatWeek
                                                }).FirstOrDefaultAsync();
                    return listAbsentSlot;
                }
                else
                {
                    var listAbsentSlot = await (from x in DbContext.CoachAbsences
                                                where x.Id == id
                                                select new GetCoachAbsentSlotDetailsViewModel()
                                                {
                                                    id = x.Id,
                                                    allDays = x.AllDay,
                                                    startdate = MinDate,
                                                    enddate = MaxDate,
                                                    starttime = x.StartTime,
                                                    endtime = x.EndTime,
                                                    type = x.Type,
                                                    title = x.Title,
                                                    note = x.Note,
                                                    repeatWeek = x.RepeatWeek
                                                }).FirstOrDefaultAsync();
                    return listAbsentSlot;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCoachAbsentSlot(Guid userId, CoachCoachAbsentSlotViewModel coachAbsentSlot, int id)
        {
            try
            {
                Guid obj = Guid.NewGuid();
                if (coachAbsentSlot != null && id > 0)
                {
                    if (id > 0)
                    {
                        var uniqueId = DbContext.CoachAbsences.Where(item => item.Id == id).Select(item => item.UniqueID).FirstOrDefault();
                        IEnumerable<CoachAbsences> coachAbsences = DbContext.CoachAbsences.AsNoTracking().Where(item => item.UniqueID.ToString().ToLower() == uniqueId.ToString().ToLower() && item.StartDate.Date >= DateTime.Now.Date).ToList();
                        //DbContext.RemoveRange(coachAbsences);
                        foreach (var item in coachAbsences)
                        {
                            DbContext.Remove<CoachAbsences>(item);
                            await DbContext.SaveChangesAsync();
                        }
                    }
                    //DateTime now = DateTime.Now;
                    //DateTime now = coachAbsentSlot.StartDate.Date.AddDays(-7);
                    string[] dayAsString = new string[7];
                    var startDate = coachAbsentSlot.StartDate.Date.AddDays(-30);
                    var endDate = coachAbsentSlot.EndDate.AddMonths(1);
                    if (Convert.ToInt32(coachAbsentSlot.Type) == 2)
                    {
                        int year = DateTime.Now.Year;
                        startDate = new DateTime(year, 1, 1);
                        endDate = new DateTime(year, 12, 31);
                    }
                    int i = 0;

                    for (DateTime date = coachAbsentSlot.StartDate; date.Date <= coachAbsentSlot.EndDate; date = date.AddDays(1))
                    {
                        if (6 > i)
                        {
                            dayAsString[i] = date.Date.DayOfWeek.ToString();
                            i++;
                        }
                        if (6 == i)
                            break;
                    }
                    for (DateTime date = startDate; date.Date <= endDate; date = date.AddDays(1))
                    {
                        CoachAbsences coachAbsences = new CoachAbsences();
                        //if (dayAsString.Contains(date.DayOfWeek.ToString()) && coachAbsentSlot.StartDate.AddDays(1).Date < date.Date)
                        if (date.Date >= coachAbsentSlot.StartDate.Date && date.Date <= coachAbsentSlot.EndDate.Date)
                        {
                            coachAbsences.CoachUserId = userId;
                            coachAbsences.StartDate = date;
                            coachAbsences.EndDate = date;
                            coachAbsences.AllDay = coachAbsentSlot.AllDay;
                            coachAbsences.Type = coachAbsentSlot.Type;
                            coachAbsences.Title = coachAbsentSlot.Title;
                            coachAbsences.LocationId = coachAbsentSlot.LocationId;
                            coachAbsences.CreatedBy = userId;
                            coachAbsences.RepeatWeek = coachAbsentSlot.RepeatWeek;
                            coachAbsences.UniqueID = obj;
                            coachAbsences.Note = coachAbsentSlot.Note;
                            coachAbsences.CreatedDate = DateTime.Now;
                            coachAbsences.StartTime = coachAbsentSlot.AllDay == 0 ? coachAbsentSlot.StartTime : "";
                            coachAbsences.EndTime = coachAbsentSlot.AllDay == 0 ? coachAbsentSlot.EndTime : "";
                            DbContext.Add<CoachAbsences>(coachAbsences);
                            var resultQuery = await DbContext.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    //DateTime now = coachAbsentSlot.StartDate.Date.AddDays(-7);
                    string[] dayAsString = new string[7];
                    var startDate = coachAbsentSlot.StartDate.Date.AddDays(-30);
                    var endDate = coachAbsentSlot.EndDate.AddMonths(1);
                    if (Convert.ToInt32(coachAbsentSlot.Type) == 2)
                    {
                        int year = DateTime.Now.Year;
                        startDate = new DateTime(year, 1, 1);
                        endDate = new DateTime(year, 12, 31);
                    }
                    int i = 0;
                    for (DateTime date = coachAbsentSlot.StartDate; date.Date <= coachAbsentSlot.EndDate; date = date.AddDays(1))
                    {
                        if (6 > i)
                        {
                            dayAsString[i] = date.Date.DayOfWeek.ToString();
                            i++;
                        }
                        if (6 == i)
                            break;
                    }
                    for (DateTime date = startDate; date.Date <= endDate; date = date.AddDays(1))
                    {
                        CoachAbsences coachAbsences = new CoachAbsences();
                        //if (dayAsString.Contains(date.DayOfWeek.ToString()) && coachAbsentSlot.StartDate.Date <= date.Date)
                        if (date.Date >= coachAbsentSlot.StartDate.Date && date.Date <= coachAbsentSlot.EndDate.Date)
                        {
                            coachAbsences.CoachUserId = userId;
                            coachAbsences.StartDate = date;
                            coachAbsences.EndDate = date;
                            coachAbsences.AllDay = coachAbsentSlot.AllDay;
                            coachAbsences.Type = coachAbsentSlot.Type;
                            coachAbsences.Title = coachAbsentSlot.Title;
                            coachAbsences.LocationId = coachAbsentSlot.LocationId;
                            coachAbsences.RepeatWeek = coachAbsentSlot.RepeatWeek;
                            coachAbsences.CreatedBy = userId;
                            coachAbsences.UniqueID = obj;
                            coachAbsences.Note = coachAbsentSlot.Note;
                            coachAbsences.CreatedDate = DateTime.Now;
                            coachAbsences.StartTime = coachAbsentSlot.AllDay == 0 ? coachAbsentSlot.StartTime : "";
                            coachAbsences.EndTime = coachAbsentSlot.AllDay == 0 ? coachAbsentSlot.EndTime : "";
                            DbContext.Add<CoachAbsences>(coachAbsences);
                            var resultQuery = await DbContext.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteCoachAbsentSlot(int id)
        {
            try
            {
                var coachAbsentSlotDetails = DbContext.CoachAbsences.Where(item => item.Id == id).FirstOrDefault();
                if (id > 0)
                {
                    DbContext.Remove<CoachAbsences>(coachAbsentSlotDetails);
                    await DbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<GetInsightActivitiesViewModel> GetCoachInsightActivities(int searchType, Guid userId)
        {
            try
            {
                DateTime baseDate = DateTime.Today;
                var startDate = baseDate.AddDays(-(int)baseDate.DayOfWeek);
                var endDate = startDate.AddDays(7).AddSeconds(-1);
                if (searchType == 2)
                {
                    startDate = baseDate.AddDays(1 - baseDate.Day);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                }
                else if (searchType == 3)
                {
                    int year = DateTime.Now.Year;
                    startDate = new DateTime(year, 1, 1);
                    endDate = new DateTime(year, 12, 31);
                }

                var individualUserIds = await (from x in DbContext.IndividualClasses
                                               join y in DbContext.IndividualBookings
                                               on x.ClassId equals y.ClassId
                                               where x.CoachId == userId && y.IsPaid == true && y.Status == EBookingStatus.Approved
                                               select y.TraineeId).ToListAsync();

                var groupUserIds = await (from x in DbContext.GroupClasses
                                          join y in DbContext.GroupBookings
                                          on x.GroupClassId equals y.GroupClassId
                                          where x.CoachId == userId && y.IsPaid == true && y.Status == EBookingStatus.Approved
                                          select y.ParticipantId).ToListAsync();


                var userList = await (from x in DbContext.Users
                                      where (individualUserIds.Contains(x.UserId) || groupUserIds.Contains(x.UserId))
                                        && (x.CreatedDate >= startDate && x.CreatedDate <= endDate)
                                      select new CoachCustomerListCountViewModel()
                                      {
                                          id = x.UserId,
                                          day = x.CreatedDate.HasValue ? x.CreatedDate.Value.DayOfWeek.ToString() : "",
                                          createdDate = x.CreatedDate.HasValue ? x.CreatedDate.Value : null,
                                          month = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("MMMM", CultureInfo.InvariantCulture) : ""
                                      }).ToListAsync();

                List<CoachCustomerChartCountViewModel> coachCustomerChartCountViewModels = new List<CoachCustomerChartCountViewModel>();

                CoachCustomerChartCountViewModel coachCustomerChartSundayViewModels = new CoachCustomerChartCountViewModel();
                CoachCustomerChartCountViewModel coachCustomerChartMondayViewModels = new CoachCustomerChartCountViewModel();
                CoachCustomerChartCountViewModel coachCustomerChartTuesDayViewModels = new CoachCustomerChartCountViewModel();
                CoachCustomerChartCountViewModel coachCustomerChartWednesDayViewModels = new CoachCustomerChartCountViewModel();
                CoachCustomerChartCountViewModel coachCustomerChartThusDayViewModels = new CoachCustomerChartCountViewModel();
                CoachCustomerChartCountViewModel coachCustomerChartFridayViewModels = new CoachCustomerChartCountViewModel();
                CoachCustomerChartCountViewModel coachCustomerChartSaturDayViewModels = new CoachCustomerChartCountViewModel();

                coachCustomerChartSundayViewModels.dayWeekMonth = "Sunday";
                coachCustomerChartSundayViewModels.customerCount = userList.Where(item => item.day == "Sunday").Count();
                coachCustomerChartMondayViewModels.dayWeekMonth = "Monday";
                coachCustomerChartMondayViewModels.customerCount = userList.Where(item => item.day == "Monday").Count();
                coachCustomerChartTuesDayViewModels.dayWeekMonth = "Tuesday";
                coachCustomerChartTuesDayViewModels.customerCount = userList.Where(item => item.day == "TuesDay").Count();
                coachCustomerChartWednesDayViewModels.dayWeekMonth = "Wednesday";
                coachCustomerChartWednesDayViewModels.customerCount = userList.Where(item => item.day == "WednesDay").Count();
                coachCustomerChartThusDayViewModels.dayWeekMonth = "Thusday";
                coachCustomerChartThusDayViewModels.customerCount = userList.Where(item => item.day == "Thusday").Count();
                coachCustomerChartFridayViewModels.dayWeekMonth = "Friday";
                coachCustomerChartFridayViewModels.customerCount = userList.Where(item => item.day == "Friday").Count();
                coachCustomerChartSaturDayViewModels.dayWeekMonth = "SaturDay";
                coachCustomerChartSaturDayViewModels.customerCount = userList.Where(item => item.day == "SaturDay").Count();

                coachCustomerChartCountViewModels.Add(coachCustomerChartSundayViewModels);
                coachCustomerChartCountViewModels.Add(coachCustomerChartMondayViewModels);
                coachCustomerChartCountViewModels.Add(coachCustomerChartTuesDayViewModels);
                coachCustomerChartCountViewModels.Add(coachCustomerChartWednesDayViewModels);
                coachCustomerChartCountViewModels.Add(coachCustomerChartThusDayViewModels);
                coachCustomerChartCountViewModels.Add(coachCustomerChartFridayViewModels);
                coachCustomerChartCountViewModels.Add(coachCustomerChartSaturDayViewModels);

                if (searchType == 2)
                {
                    coachCustomerChartCountViewModels.Clear();
                    CoachCustomerChartCountViewModel weekOneViewModel = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel weekTwoViewModel = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel weekThreeViewModel = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel weekFourViewModel = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel weekFiveViewModel = new CoachCustomerChartCountViewModel();

                    var firststartDateMonth = baseDate.AddDays(1 - baseDate.Day); ;
                    var firstendDateMonth = firststartDateMonth.AddDays(7).AddSeconds(-1);
                    var secondstartDateMonth = firstendDateMonth.AddDays(1).AddSeconds(-1);
                    var secondendDateMonth = secondstartDateMonth.AddDays(7).AddSeconds(-1);
                    var thirdstartDateMonth = secondendDateMonth.AddDays(1).AddSeconds(-1);
                    var thirdendDateMonth = thirdstartDateMonth.AddDays(7).AddSeconds(-1);
                    var forthstartDateMonth = thirdendDateMonth.AddDays(1).AddSeconds(-1);
                    var forthendDateMonth = forthstartDateMonth.AddDays(7).AddSeconds(-1);
                    var fifthstartDateMonth = forthendDateMonth.AddDays(1).AddSeconds(-1);

                    weekOneViewModel.dayWeekMonth = "Week 1";
                    weekOneViewModel.customerCount = userList.Where(item => item.createdDate.Value.Date >= firststartDateMonth.Date && item.createdDate.Value.Date <= firstendDateMonth).Count();
                    weekTwoViewModel.dayWeekMonth = "Week 2";
                    weekTwoViewModel.customerCount = userList.Where(item => item.createdDate.Value.Date >= secondstartDateMonth.Date && item.createdDate.Value.Date <= secondendDateMonth).Count();
                    weekThreeViewModel.dayWeekMonth = "Week 3";
                    weekThreeViewModel.customerCount = userList.Where(item => item.createdDate.Value.Date >= thirdstartDateMonth.Date && item.createdDate.Value.Date <= thirdendDateMonth).Count();
                    weekFourViewModel.dayWeekMonth = "Week 4";
                    weekFourViewModel.customerCount = userList.Where(item => item.createdDate.Value.Date >= forthstartDateMonth.Date && item.createdDate.Value.Date <= forthstartDateMonth).Count();
                    weekFiveViewModel.dayWeekMonth = "Week 5";
                    weekFiveViewModel.customerCount = userList.Where(item => item.createdDate.Value.Date >= fifthstartDateMonth.Date && item.createdDate.Value.Date <= endDate).Count();

                    coachCustomerChartCountViewModels.Add(weekOneViewModel);
                    coachCustomerChartCountViewModels.Add(weekTwoViewModel);
                    coachCustomerChartCountViewModels.Add(weekThreeViewModel);
                    coachCustomerChartCountViewModels.Add(weekFourViewModel);
                    coachCustomerChartCountViewModels.Add(weekFiveViewModel);
                }
                else if (searchType == 3)
                {
                    coachCustomerChartCountViewModels.Clear();
                    CoachCustomerChartCountViewModel coachCustomerChartJanuaryViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartFebruaryViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartMarchViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartAprilViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartMayViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartJuneViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartJulyViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartAugustViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartSeptemberViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartOctoberViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartNovemberViewModels = new CoachCustomerChartCountViewModel();
                    CoachCustomerChartCountViewModel coachCustomerChartDecemberViewModels = new CoachCustomerChartCountViewModel();

                    coachCustomerChartJanuaryViewModels.dayWeekMonth = "January";
                    coachCustomerChartJanuaryViewModels.customerCount = userList.Where(item => item.month == "January").Count();
                    coachCustomerChartFebruaryViewModels.dayWeekMonth = "February";
                    coachCustomerChartFebruaryViewModels.customerCount = userList.Where(item => item.month == "February").Count();
                    coachCustomerChartMarchViewModels.dayWeekMonth = "March";
                    coachCustomerChartMarchViewModels.customerCount = userList.Where(item => item.month == "March").Count();
                    coachCustomerChartAprilViewModels.dayWeekMonth = "April";
                    coachCustomerChartAprilViewModels.customerCount = userList.Where(item => item.month == "April").Count();
                    coachCustomerChartMayViewModels.dayWeekMonth = "May";
                    coachCustomerChartMayViewModels.customerCount = userList.Where(item => item.month == "May").Count();
                    coachCustomerChartJuneViewModels.dayWeekMonth = "June";
                    coachCustomerChartJuneViewModels.customerCount = userList.Where(item => item.month == "June").Count();
                    coachCustomerChartJulyViewModels.dayWeekMonth = "July";
                    coachCustomerChartJulyViewModels.customerCount = userList.Where(item => item.month == "July").Count();
                    coachCustomerChartAugustViewModels.dayWeekMonth = "August";
                    coachCustomerChartAugustViewModels.customerCount = userList.Where(item => item.month == "August").Count();
                    coachCustomerChartSeptemberViewModels.dayWeekMonth = "September";
                    coachCustomerChartSeptemberViewModels.customerCount = userList.Where(item => item.month == "September").Count();
                    coachCustomerChartOctoberViewModels.dayWeekMonth = "October";
                    coachCustomerChartOctoberViewModels.customerCount = userList.Where(item => item.month == "October").Count();
                    coachCustomerChartNovemberViewModels.dayWeekMonth = "November";
                    coachCustomerChartNovemberViewModels.customerCount = userList.Where(item => item.month == "November").Count();
                    coachCustomerChartDecemberViewModels.dayWeekMonth = "December";
                    coachCustomerChartDecemberViewModels.customerCount = userList.Where(item => item.month == "December").Count();

                    coachCustomerChartCountViewModels.Add(coachCustomerChartJanuaryViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartFebruaryViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartMarchViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartAprilViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartMayViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartJuneViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartJulyViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartAugustViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartSeptemberViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartOctoberViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartNovemberViewModels);
                    coachCustomerChartCountViewModels.Add(coachCustomerChartDecemberViewModels);
                }

                GetInsightActivitiesViewModel getInsightActivitiesViewModel = new GetInsightActivitiesViewModel();
                var individualIncome = DbContext.IndividualClasses.Where(item => item.CoachId == userId && item.IsEnabled == true && item.LastEditedDate == null ? (item.CreatedDate >= startDate && item.CreatedDate <= endDate) : (item.LastEditedDate >= startDate && item.LastEditedDate <= endDate)).Select(item => item.Price).Sum();
                var individualCustomer = (from x in DbContext.IndividualBookings
                                          join y in DbContext.IndividualClasses
                                          on x.ClassId equals y.ClassId
                                          where y.CoachId == userId && x.IsEnabled == true && x.LastEditedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.LastEditedDate >= startDate && x.LastEditedDate <= endDate)
                                          select new
                                          {
                                              x.TraineeId
                                          }).Distinct().Count();
                var groupIncome = DbContext.GroupClasses.Where(item => item.CoachId == userId && item.IsEnabled == true && item.LastEditedDate == null ? (item.CreatedDate >= startDate && item.CreatedDate <= endDate) : (item.LastEditedDate >= startDate && item.LastEditedDate <= endDate)).Select(item => item.Price).Sum();
                var groupCustomer = (from x in DbContext.GroupBookings
                                     join y in DbContext.GroupClasses
                                     on x.GroupClassId equals y.GroupClassId
                                     where y.CoachId == userId && x.IsEnabled == true && x.LastEditedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.LastEditedDate >= startDate && x.LastEditedDate <= endDate)
                                     select new
                                     {
                                         x.ParticipantId
                                     }).Distinct().Count();

                var completedGroupBooking = (from x in DbContext.GroupBookings
                                             join y in DbContext.GroupClasses
                                             on x.GroupClassId equals y.GroupClassId
                                             where y.CoachId == userId && x.IsEnabled == true && x.Status == EBookingStatus.Approved && x.LastEditedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.LastEditedDate >= startDate && x.LastEditedDate <= endDate)
                                             select new
                                             {
                                                 x.Status
                                             }).Distinct().Count();
                var completedIndividualBooking = (from x in DbContext.IndividualBookings
                                                  join y in DbContext.IndividualClasses
                                                  on x.ClassId equals y.ClassId
                                                  where y.CoachId == userId && x.IsEnabled == true && x.Status == EBookingStatus.Approved && x.LastEditedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.LastEditedDate >= startDate && x.LastEditedDate <= endDate)
                                                  select new
                                                  {
                                                      x.TraineeId
                                                  }).Distinct().Count();


                var cancelGroupBooking = (from x in DbContext.GroupBookings
                                          join y in DbContext.GroupClasses
                                          on x.GroupClassId equals y.GroupClassId
                                          where y.CoachId == userId && x.IsEnabled == true && x.Status == EBookingStatus.Cancelled && x.LastEditedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.LastEditedDate >= startDate && x.LastEditedDate <= endDate)
                                          select new
                                          {
                                              x.Status
                                          }).Distinct().Count();

                var cancelIndividualBooking = (from x in DbContext.IndividualBookings
                                               join y in DbContext.IndividualClasses
                                               on x.ClassId equals y.ClassId
                                               where y.CoachId == userId && x.IsEnabled == true && x.Status == EBookingStatus.Approved && x.LastEditedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.LastEditedDate >= startDate && x.LastEditedDate <= endDate)
                                               select new
                                               {
                                                   x.TraineeId
                                               }).Distinct().Count();

                getInsightActivitiesViewModel.newCustomerCount = userList.Count();
                getInsightActivitiesViewModel.individualCoachingIncome = individualIncome;
                getInsightActivitiesViewModel.individualBooking = individualCustomer;
                getInsightActivitiesViewModel.individualCoachingIncome = individualIncome;
                getInsightActivitiesViewModel.groupBooking = groupCustomer;
                getInsightActivitiesViewModel.completedBooking = completedIndividualBooking + completedGroupBooking;
                getInsightActivitiesViewModel.cancelBooking = cancelGroupBooking + cancelIndividualBooking;
                getInsightActivitiesViewModel.coachCustomerChartCountViewModels = coachCustomerChartCountViewModels;

                return getInsightActivitiesViewModel;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<CoachCustomerListViewModel>> GetCoachCustomer(Guid userId, string name)
        {
            try
            {
                var individualUserIds = await (from x in DbContext.IndividualClasses
                                               join y in DbContext.IndividualBookings
                                               on x.ClassId equals y.ClassId
                                               where x.CoachId == userId && y.IsPaid == true && y.Status == EBookingStatus.Approved
                                               select y.TraineeId).ToListAsync();

                var groupUserIds = await (from x in DbContext.GroupClasses
                                          join y in DbContext.GroupBookings
                                          on x.GroupClassId equals y.GroupClassId
                                          where x.CoachId == userId && y.IsPaid == true && y.Status == EBookingStatus.Approved
                                          select y.ParticipantId).ToListAsync();

                var userList = await (from x in DbContext.Users
                                      where (individualUserIds.Contains(x.UserId) || groupUserIds.Contains(x.UserId))
                                       && (name == null || (x.FirstName + " " + x.LastName).Contains(name))
                                      select new CoachCustomerListViewModel()
                                      {
                                          id = x.UserId,
                                          name = x.FirstName + " " + x.LastName,
                                          profilePicture = x.ImageUrl
                                      }).ToListAsync();

                return userList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<APIResponse> GetCoachConfirmation(ReviewType type, int id)
        {
            var apiResp = new APIResponse();
            try
            {
                CoachingConfirmation response = new CoachingConfirmation();
                if (type == ReviewType.GroupClass)
                {
                    response.Detail = await (from x in DbContext.GroupBookings
                                             join y in DbContext.GroupClasses
                                               on x.GroupClassId equals y.GroupClassId
                                             join participant in DbContext.Users
                                             on x.ParticipantId equals participant.UserId
                                             where y.Id == id
                                             select new CoachingDetail
                                             {
                                                 CoachId = y.CoachId,
                                                 Date = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy"),
                                                 Slot = Convert.ToDateTime(y.Start).ToString("hh:mm") + " to " + Convert.ToDateTime(y.End).ToString("hh:mm"),
                                                 Price = y.Price + " AED",
                                                 Commision = x.SideKickCommission + " AED",
                                                 PriceHour = y.Price + " AED",
                                                 TotalPrice = x.TotalAmount + " AED",
                                                 Status = x.Status,
                                                 BookingId = x.GroupBookingId,
                                             }).FirstOrDefaultAsync();

                    var coach = await (from x in DbContext.Coaches
                                       join y in DbContext.Users on x.CoachUserId equals y.UserId
                                       where y.UserId == response.Detail.CoachId
                                       select new CoachDetailedViewModel()
                                       {
                                           UserID = y.Id,
                                           UserNo = x.Id,
                                           ProfileName = string.Format("{0} {1}", y.FirstName, y.LastName),
                                           Email = y.Email,
                                           MobileNo = y.MobileNumber,
                                           ImageUrl = y.ImageUrl,
                                           //LastCoachingDate = null,
                                           Status = x.IsEnabled.Value ? "Active" : "Inactive",
                                           DateCreated = y.CreatedDate,
                                           CoachUserId = x.CoachUserId.Value,
                                           Experience = x.Experience,
                                           Location = x.Location,
                                           LocationLat = x.LocationLat,
                                           LocationLong = x.LocationLong,
                                           NationalityId = y.NationalityId,
                                           Description = x.Description
                                       })
                          .FirstOrDefaultAsync();

                    var friend = await DbContext.UserFriends.Where(x => x.UserId == coach.UserID).ToListAsync();

                    var ratings = await (from x in DbContext.UserReviews
                                         join y in DbContext.Users
                                           on x.UserId equals y.UserId
                                         where x.CoachId == response.Detail.CoachId && x.Type == ReviewType.GroupClass
                                         select new UserReviewList
                                         {
                                             Ratings = x.Ratings,
                                             Description = x.Description,
                                             Name = y.FirstName,
                                             Image = y.ImageUrl,
                                             Date = x.CreatedDate,
                                         }).ToListAsync();

                    var specialties = await (from x in DbContext.CoachSpecialties
                                             join y in DbContext.Specialties
                                               on x.SpecialtyId equals y.SpecialtyId
                                             where x.CoachUserId == response.Detail.CoachId
                                             select new CoachingSpecialties
                                             {
                                                 Icon = y.Icon,
                                                 Name = y.Name,
                                             }).ToListAsync();

                    var badges = await (from x in DbContext.UserTrainBadges
                                        join y in DbContext.Specialties
                                          on x.SpecialtyId equals y.SpecialtyId
                                        where x.UserId == response.Detail.CoachId
                                        select new CoachingBadges
                                        {
                                            Icon = y.Icon,
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

                    Notation notation = new Notation();
                    notation.Details = model;
                    if (model.Count > 0)
                    {
                        notation.totalRatings = (totalRatings / Convert.ToDecimal(model.Count)) * 5;
                    }
                    else
                    {
                        notation.totalRatings = 0;
                    }
                    response.BookingID = id;
                    response.Type = type;
                    response.ImageUrl = coach.ImageUrl;
                    response.ProfileName = coach.ProfileName;
                    response.Experience = coach.Experience;
                    response.Location = coach.Location;
                    response.LocationLat = coach.LocationLat;
                    response.LocationLong = coach.LocationLong;
                    response.NationalityId = coach.NationalityId;
                    response.Description = coach.Description;
                    response.Specialties = specialties;
                    response.Badges = badges;
                    response.Rating = notation.totalRatings;
                    response.Notation = notation;
                    response.FriendCount = friend.Count;
                    response.Status = response.Detail.Status.ToString();
                }
                else
                {
                    response.Detail = await (from x in DbContext.IndividualBookings
                                             join y in DbContext.IndividualClasses
                                               on x.ClassId equals y.ClassId
                                             join participant in DbContext.Users
                                             on x.TraineeId equals participant.UserId
                                             where y.Id == id
                                             select new CoachingDetail
                                             {
                                                 CoachId = y.CoachId,
                                                 Date = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy"),
                                                 Slot = Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " to " + Convert.ToDateTime(x.EndTime).ToString("hh:mm"),
                                                 Price = y.Price + " AED",
                                                 Commision = x.SideKickCommission + " AED",
                                                 PriceHour = x.AmountPerHour + " AED",
                                                 TotalPrice = x.TotalAmount + " AED",
                                                 Status = x.Status,
                                                 BookingId = x.BookingId
                                             }).FirstOrDefaultAsync();

                    var coach = await (from x in DbContext.Coaches
                                       join y in DbContext.Users
                                         on x.CoachUserId equals y.UserId
                                       where y.UserId == response.Detail.CoachId
                                       select new CoachDetailedViewModel()
                                       {
                                           UserID = y.Id,
                                           UserNo = x.Id,
                                           ProfileName = string.Format("{0} {1}", y.FirstName, y.LastName),
                                           Email = y.Email,
                                           MobileNo = y.MobileNumber,
                                           ImageUrl = y.ImageUrl,
                                           //LastCoachingDate = null,
                                           Status = x.IsEnabled.Value ? "Active" : "Inactive",
                                           DateCreated = y.CreatedDate,
                                           CoachUserId = x.CoachUserId.Value,
                                           Experience = x.Experience,
                                           Location = x.Location,
                                           LocationLat = x.LocationLat,
                                           LocationLong = x.LocationLong,
                                           NationalityId = y.NationalityId,
                                           Description = x.Description,
                                           Birthday = y.DateOfBirth
                                       })
                          .FirstOrDefaultAsync();

                    var friend = await DbContext.UserFriends.Where(x => x.UserId == coach.UserID).ToListAsync();

                    var ratings = await (from x in DbContext.UserReviews
                                         join y in DbContext.Users
                                           on x.UserId equals y.UserId
                                         where x.CoachId == response.Detail.CoachId && x.Type == ReviewType.IndividualClass
                                         select new UserReviewList
                                         {
                                             Ratings = x.Ratings,
                                             Description = x.Description,
                                             Name = y.FirstName,
                                             Image = y.ImageUrl,
                                             Date = x.CreatedDate,
                                         }).ToListAsync();

                    var specialties = await (from x in DbContext.CoachSpecialties
                                             join y in DbContext.Specialties
                                               on x.SpecialtyId equals y.SpecialtyId
                                             where x.CoachUserId == response.Detail.CoachId
                                             select new CoachingSpecialties
                                             {
                                                 Icon = y.Icon,
                                                 Name = y.Name,
                                             }).ToListAsync();

                    var badges = await (from x in DbContext.UserTrainBadges
                                        join y in DbContext.Specialties
                                          on x.SpecialtyId equals y.SpecialtyId
                                        where x.UserId == response.Detail.CoachId
                                        select new CoachingBadges
                                        {
                                            Icon = y.Icon,
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

                    Notation notation = new Notation();
                    notation.Details = model;
                    if (model.Count > 0)
                    {
                        notation.totalRatings = (totalRatings / Convert.ToDecimal(model.Count)) * 5;
                    }
                    else
                    {
                        notation.totalRatings = 0;
                    }

                    response.BookingID = id;
                    response.Type = type;
                    response.ImageUrl = coach.ImageUrl;
                    response.ProfileName = coach.ProfileName;
                    response.Experience = coach.Experience;
                    response.Location = coach.Location;
                    response.LocationLat = coach.LocationLat;
                    response.LocationLong = coach.LocationLong;
                    response.NationalityId = coach.NationalityId;
                    response.Description = coach.Description;
                    response.Specialties = specialties;
                    response.Badges = badges;
                    response.Age = DateTime.Today.Year - Convert.ToDateTime(coach.Birthday).Year;
                    response.Rating = notation.totalRatings;
                    response.Notation = notation;
                    response.FriendCount = friend.Count;
                    response.Status = response.Detail.Status.ToString();
                }

                return new APIResponse
                {
                    Message = "Retrieved Coach Confirmation",
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Status = "Success!"
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::CoachRepository::GetCoachConfirmation --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
                return apiResp;
            }
        }

        public async Task<APIResponse> CancelCoachConfirmation(ReviewType type, int id)
        {
            APIResponse apiResp = new APIResponse();

            try
            {
                if (type == ReviewType.GroupClass)
                {

                    var detail = await DbContext.GroupClasses.Where(x => x.Id == id).FirstOrDefaultAsync();
                    GroupBooking response = await DbContext.GroupBookings.Where(x => x.GroupClassId == detail.GroupClassId).FirstOrDefaultAsync();
                    response.Status = EBookingStatus.Cancelled;
                    DbContext.Update(response);
                    DbContext.SaveChanges();

                    LogManager.LogInfo("Group Bookings Cancelled Id " + response.Id.ToString());
                    apiResp.Message = "Cancelled Coach Confirmation";
                    apiResp.Status = "Success!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    var detail = await DbContext.IndividualClasses.Where(x => x.Id == id).FirstOrDefaultAsync();
                    IndividualBooking response = await DbContext.IndividualBookings.Where(x => x.ClassId == detail.ClassId).FirstOrDefaultAsync();
                    response.Status = EBookingStatus.Cancelled;
                    DbContext.Update(response);
                    DbContext.SaveChanges();

                    LogManager.LogInfo("Individual Bookings Cancelled Id " + response.Id.ToString());
                    apiResp.Message = "Cancelled Coach Confirmation";
                    apiResp.Status = "Success!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::CoachRepository::CancelCoachConfirmation --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetCoachHome(string dateFrom, string dateTo)
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var groupBookings = await (from x in DbContext.GroupBookings
                                           join y in DbContext.GroupClasses
                                             on x.GroupClassId equals y.GroupClassId
                                           join z in DbContext.Gyms
                                                on y.GymId equals z.GymId
                                           join participant in DbContext.Users
                                                   on x.ParticipantId equals participant.UserId
                                           where (x.Status == EBookingStatus.Confirmed || x.Status == EBookingStatus.Approved) && y.CoachId == currentLogin
                                           select new CoachHome
                                           {
                                               Id = y.Id,
                                               ProfileName = participant.FirstName + " " + participant.LastName,
                                               BookingId = x.GroupBookingId,
                                               Description = y.Title,
                                               Time = Convert.ToDateTime(y.Start).ToString("hh:mm") + " - " + Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).ToString("hh:mm"),
                                               Type = "Group Class",
                                               BookingType = ReviewType.GroupClass,
                                               Status = ReviewType.GroupClass,
                                               Date = Convert.ToDateTime(Convert.ToDateTime(x.Date).ToShortDateString()),
                                               Location = z.GymName,
                                               StartDate = new DateTime(x.Date.Value.Year, x.Date.Value.Month, x.Date.Value.Day, Convert.ToDateTime(y.Start).Hour, Convert.ToDateTime(y.Start).Minute, Convert.ToDateTime(y.Start).Second),
                                               EndDate = new DateTime(x.Date.Value.Year, x.Date.Value.Month, x.Date.Value.Day, Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).Hour, Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).Minute, Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).Second),
                                           }).ToListAsync();

                var groupBookingsPending = await (from x in DbContext.GroupBookings
                                                  join y in DbContext.GroupClasses
                                                    on x.GroupClassId equals y.GroupClassId
                                                  join z in DbContext.Locations
                                                   on y.LocationId equals z.LocationId
                                                  join participant in DbContext.Users
                                                     on x.ParticipantId equals participant.UserId
                                                  where x.Status == EBookingStatus.Pending && y.CoachId == currentLogin
                                                  select new CoachHome
                                                  {
                                                      Id = y.Id,
                                                      ProfileName = participant.FirstName + " " + participant.LastName,
                                                      BookingId = x.GroupBookingId,
                                                      Description = y.Title,
                                                      Time = Convert.ToDateTime(y.Start).ToString("hh:mm") + " - " + Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).ToString("hh:mm"),
                                                      Type = "Tap to confirm",
                                                      BookingType = ReviewType.GroupClass,
                                                      Status = ReviewType.Pending,
                                                      Date = Convert.ToDateTime(Convert.ToDateTime(x.Date).ToShortDateString()),
                                                      Location = z.Name,
                                                      StartDate = new DateTime(x.Date.Value.Year, x.Date.Value.Month, x.Date.Value.Day, Convert.ToDateTime(y.Start).Hour, Convert.ToDateTime(y.Start).Minute, Convert.ToDateTime(y.Start).Second),
                                                      EndDate = new DateTime(x.Date.Value.Year, x.Date.Value.Month, x.Date.Value.Day, Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).Hour, Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).Minute, Convert.ToDateTime(y.Start).AddHours(y.Duration.GetValueOrDefault()).Second),
                                                  }).ToListAsync();

                var individualBookings = await (from x in DbContext.IndividualBookings
                                                join y in DbContext.IndividualClasses
                                                  on x.ClassId equals y.ClassId
                                                join participant in DbContext.Users
                                                   on x.TraineeId equals participant.UserId
                                                where (x.Status == EBookingStatus.Confirmed || x.Status == EBookingStatus.Approved) && y.CoachId == currentLogin
                                                select new CoachHome
                                                {
                                                    Id = y.Id,
                                                    ProfileName = participant.FirstName + " " + participant.LastName,
                                                    BookingId = x.BookingId,
                                                    Description = x.Coaching,
                                                    Time = Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " - " + Convert.ToDateTime(x.StartTime).ToString("hh:mm"),
                                                    Type = "Individual Class",
                                                    BookingType = ReviewType.IndividualClass,
                                                    Status = ReviewType.IndividualClass,
                                                    Date = Convert.ToDateTime(Convert.ToDateTime(x.Date).ToShortDateString()),
                                                    StartDate = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, Convert.ToDateTime(x.StartTime).Hour, Convert.ToDateTime(x.StartTime).Minute, Convert.ToDateTime(x.StartTime).Second),
                                                    EndDate = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, Convert.ToDateTime(x.EndTime).Hour, Convert.ToDateTime(x.EndTime).Minute, Convert.ToDateTime(x.EndTime).Second),
                                                    Location = x.Location
                                                }).ToListAsync();

                var individualBookingsPending = await (from x in DbContext.IndividualBookings
                                                       join y in DbContext.IndividualClasses
                                                         on x.ClassId equals y.ClassId
                                                       join participant in DbContext.Users
                                                          on x.TraineeId equals participant.UserId
                                                       where x.Status == EBookingStatus.Pending && y.CoachId == currentLogin
                                                       select new CoachHome
                                                       {
                                                           Id = y.Id,
                                                           ProfileName = participant.FirstName + " " + participant.LastName,
                                                           BookingId = x.BookingId,
                                                           Description = x.Coaching,
                                                           Time = Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " - " + Convert.ToDateTime(x.EndTime).ToString("hh:mm"),
                                                           Type = "Tap to confirm",
                                                           BookingType = ReviewType.IndividualClass,
                                                           Status = ReviewType.Pending,
                                                           Date = Convert.ToDateTime(Convert.ToDateTime(x.Date).ToShortDateString()),
                                                           Location = x.Location,
                                                           StartDate = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, Convert.ToDateTime(x.StartTime).Hour, Convert.ToDateTime(x.StartTime).Minute, Convert.ToDateTime(x.StartTime).Second),
                                                           EndDate = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, Convert.ToDateTime(x.EndTime).Hour, Convert.ToDateTime(x.EndTime).Minute, Convert.ToDateTime(x.EndTime).Second),
                                                       }).ToListAsync();

                var dtFrom = dateFrom == null ? Convert.ToDateTime("01/01/1900") : Convert.ToDateTime(dateFrom);
                var dtTo = dateTo == null ? Convert.ToDateTime("01/01/2300") : Convert.ToDateTime(dateTo).AddDays(1);

                var response = new CoachHomeResponse();
                var bookings = groupBookings
                    .Union(groupBookingsPending)
                    .Union(individualBookings)
                    .Union(individualBookingsPending).ToList();
                response.CoachHomes = bookings.Where(x => x.Date >= Convert.ToDateTime(dtFrom) && x.Date < Convert.ToDateTime(dtTo).AddDays(1)).ToList();

                response.BookingDates = new List<string>();

                foreach (var userIndividualBookingDate in bookings)
                {
                    response.BookingDates.Add(userIndividualBookingDate.Date.ToString("dd MMM yyyy"));
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
                LogManager.LogInfo("-- Error::UserRepository::GetCoachHome --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public async Task<APIResponse> GetCoachHomeDetail(ReviewType Type, Guid BookingId)
        {
            var apiResp = new APIResponse();
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                if (Type == ReviewType.GroupClass)
                {
                    
                    var response = await (from x in DbContext.GroupBookings
                                          join y in DbContext.GroupClasses
                                          on x.GroupClassId equals y.GroupClassId
                                          join z in DbContext.Gyms
                                          on y.GymId equals z.GymId
                                          join participant in DbContext.Users
                                              on x.ParticipantId equals participant.UserId
                                          where x.GroupBookingId == BookingId
                                          select new BookingDetail
                                          {
                                              Id = y.Id,
                                              BookingId = x.GroupBookingId,
                                              Activity = y.Title,
                                              GroupClassId = y.GroupClassId,
                                              ProfileName = participant.FirstName + " " + participant.LastName,
                                              ImageUrl = participant.ImageUrl,
                                              Date = Convert.ToDateTime(y.Start).ToString("dd MMM yyyy"),
                                              Slot = Convert.ToDateTime(y.Start).ToString("hh:mm") + " - " + Convert.ToDateTime(y.End).ToString("hh:mm"),
                                              Coaching = y.Title,
                                              Location = z.GymName,
                                              Frequency = y.RepeatEveryWeek,
                                              Duration = y.Duration.ToString() + " " + y.During.ToString(),
                                              Notes = y.Notes,
                                              Total = x.TotalAmount,
                                              Participants = y.Participants
                                          }).FirstOrDefaultAsync();

                    if (response != null)
                    {
                        response.Participant = await (from x in DbContext.GroupBookings
                                                      join y in DbContext.Users
                                                  on x.ParticipantId equals y.UserId
                                                      where x.GroupClassId == response.GroupClassId
                                                      select new Participant
                                                      {
                                                          UserId = y.UserId,
                                                          ImageURL = y.ImageUrl,
                                                          ProfileName = y.FirstName + " " + y.LastName,
                                                          Status = x.IsPaid == true ? "Paid" : "Pending"
                                                      }).ToListAsync();

                        response.TotalParticipant = response.Participant.Count.ToString() + "/" + response.Participants.ToString();
                    }

                    return new APIResponse
                    {
                        Message = "Retrieved GetCoachHomeDetail",
                        Payload = response,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Status = "Success!"
                    };
                }
                else
                {
                    var response = await (from x in DbContext.IndividualBookings
                                          join y in DbContext.IndividualClasses
                                             on x.ClassId equals y.ClassId
                                          join participant in DbContext.Users
                                             on x.TraineeId equals participant.UserId
                                          join Coaching in DbContext.Users
                                          on y.CoachId equals Coaching.UserId
                                          where x.BookingId == BookingId
                                          select new BookingDetail
                                          {
                                              Id = y.Id,
                                              BookingId = x.BookingId,
                                              ProfileName = participant.FirstName + " " + participant.LastName,
                                              ImageUrl = participant.ImageUrl,
                                              Date = Convert.ToDateTime(x.Date).ToString("dd MMM yyyy"),
                                              Slot = Convert.ToDateTime(x.StartTime).ToString("hh:mm") + " - " + Convert.ToDateTime(x.EndTime).ToString("hh:mm"),
                                              Coaching = Coaching.FirstName + " " + Coaching.LastName,
                                              Location = x.Location,
                                              Notes = x.Notes.ToString(),
                                              Total = x.TotalAmount,
                                          }).FirstOrDefaultAsync();

                    return new APIResponse
                    {
                        Message = "Retrieved GetCoachHomeDetail",
                        Payload = response,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Status = "Success!"
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::UserRepository::GetCoachHomeDetail --");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }
    }
}
