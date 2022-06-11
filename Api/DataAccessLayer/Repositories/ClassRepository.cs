using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Class;
using Sidekick.Model.Gym;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly APIDBContext context;
        private readonly IUserRepository _userRepository;
        private readonly ICommissionRepository _commissionRepository;

        public ClassRepository(APIDBContext context,
            IUserRepository userRepository,
            ICommissionRepository commissionRepository)
        {
            this.context = context;
            _userRepository = userRepository;
            _commissionRepository = commissionRepository;
        }

        public async Task<IEnumerable<IndividualClass>> GetIndividualClasses()
        {
            return await context.IndividualClasses
                .Where(x => x.IsEnabled == true)
                .ToListAsync();
        }

        public async Task<IndividualClass> GetIndividualClass(Guid classId)
        {
            return await context.IndividualClasses
                .FirstOrDefaultAsync(x => x.ClassId == classId);
        }

        public async Task<IndividualClass> GetLatestIndividualClassByCoach(Guid userId)
        {
            return await context.IndividualClasses
                .FirstOrDefaultAsync(x => x.CoachId == userId
                                       && x.IsEnabled == true);
        }


        private IQueryable<IndividualClassByFilterViewModel> GetQuery_IndividualClass()
        {
            var dateNow = Helper.GetDateTime();
            return (from individualClass in context.IndividualClasses
                    join coach in context.Coaches
                        on individualClass.CoachId equals coach.CoachUserId
                    join profile in context.Users
                        on coach.CoachUserId equals profile.UserId
                    where individualClass.IsEnabled == true
                    select new IndividualClassByFilterViewModel
                    {
                        ClassId = individualClass.ClassId,
                        CoachId = individualClass.CoachId,
                        CoachFirstName = profile.FirstName,
                        CoachLastName = profile.LastName,
                        CoachGender = profile.Gender,
                        ChatReceiverId = profile.Id,
                        CoachAge = dateNow.Year - profile.DateOfBirth.Value.Year,
                        CoachDescrption = coach.Description,
                        CoachImage = profile.ImageUrl,
                        Location = coach.Location,
                        Price = individualClass.Price,

                        ParticipateToOffer = individualClass.ParticipateToOffer,
                        Ratings = (from userReviews in context.UserReviews
                                   where userReviews.CoachId == individualClass.CoachId
                                   select ((double)userReviews.Ratings)).Average(),
                        Specialties = (from coachSpeciality in context.CoachSpecialties
                                       join specialty in context.Specialties
                                        on coachSpeciality.SpecialtyId equals specialty.SpecialtyId
                                       where coachSpeciality.CoachUserId == coach.CoachUserId
                                       select specialty.Name)
                                       .ToList(),
                        Badges = (from coachBadges in context.UserPlayBadges
                                  join playBadges in context.Sports
                                    on coachBadges.SportId equals playBadges.SportId
                                  where coachBadges.UserId == coach.CoachUserId
                                  select playBadges.Name)
                                  .ToList(),
                        Gyms = (from coachGym in context.CoachGyms
                                join gym in context.Gyms
                                    on coachGym.GymID equals gym.GymId
                                where coachGym.CoachUserId == coach.CoachUserId
                                select new GymViewModel
                                {
                                    GymId = gym.GymId,
                                    Gym = gym.GymName,
                                    Image = gym.Icon
                                })
                                .ToList(),
                        Languages = (from coachLanguage in context.CoachLanguages
                                     join language in context.Languages
                                        on coachLanguage.LanguageId equals language.LanguageId
                                     where coachLanguage.CoachUserId == coach.CoachUserId
                                     select new LanguageViewModel
                                     {
                                         LanguageId = language.LanguageId,
                                         Language = language._Language,
                                         Image = language.Icon
                                     })
                                     .ToList(),
                        CoachCustomSchedule = (from coachcustomschdule in context.CoachCustomSchedules
                                               where coachcustomschdule.CoachId == coach.CoachUserId
                                               select new CoachCustomScheduleViewModel
                                               {
                                                   Day = coachcustomschdule.Day,
                                                   EndTime = coachcustomschdule.EndTime,
                                                   StartTime = coachcustomschdule.StartTime
                                               }).ToList(),
                        CoachEverydayScheduleViewModel = context.CoachEverydaySchedules.FirstOrDefault(a => a.CoachId == coach.CoachUserId),

                        CoachNotAvailableSchedule = (from coachabsence in context.CoachAbsences
                                                     where coachabsence.CoachUserId == coach.CoachUserId
                                                     select new CoachNotAvailableScheduleViewModel
                                                     {
                                                         Date = coachabsence.StartDate,
                                                     }).ToList()
                    });
        }

        public async Task<IEnumerable<IndividualClassByFilterViewModel>> GetIndividualClassesByFilter(IEnumerable<FilterViewModel> filters)
        {
            var query = GetQuery_IndividualClass();

            foreach (var filter in filters)
            {
                // filter here!!!
                if (filter.FilterType == FilterType.Date)
                {
                    var date = Convert.ToDateTime(filter.FilterValue);
                    var weekDay = Helper.GetDayFromDayName(date.DayOfWeek.ToString());
                    query = query.Where(x => ((x.CoachEverydayScheduleViewModel != null) || x.CoachCustomSchedule.Select(x => x.Day).Contains((CoachingDay)weekDay)));
                    query = query.Where(x => !(x.CoachNotAvailableSchedule.Select(y => y.Date).Contains(date)));
                }

                if (filter.FilterType == FilterType.Area)
                {
                    query = query.Where(x => x.Location == filter.FilterValue);
                }

                if (filter.FilterType == FilterType.Price)
                {
                    var splPrice = filter.FilterValue.Split("-");
                    var firstValue = Convert.ToInt32(splPrice[0]);
                    var secondValue = Convert.ToInt32(splPrice[1]);

                    query = query.Where(x => x.Price >= firstValue && x.Price <= secondValue);
                }

                if (filter.FilterType == FilterType.Gender)
                {
                    Enum.TryParse(filter.FilterValue, out Genders gender);
                    query = query.Where(x => x.CoachGender == gender);
                }

                if (filter.FilterType == FilterType.Activity)
                {
                    // way to search from an array!!!
                }

                if (filter.FilterType == FilterType.GymCenter)
                {
                    // way to search from an array!!!
                    //need to adjust
                    query = query.Where(x => x.Gyms.Select(y => y.GymId).Contains(Guid.Parse(filter.FilterValue)));
                }

                if (filter.FilterType == FilterType.Language)
                {
                    query = query.Where(x => x.Languages.Select(x => x.LanguageId).Contains(Guid.Parse(filter.FilterValue)));
                }
            }
            return await query.ToListAsync();
        }

        public async Task<IndividualClassByFilterViewModel> GetIndividualClass_UserView(Guid classId)
        {
            var query = GetQuery_IndividualClass()
                .Where(x => x.ClassId == classId);

            return await query.FirstOrDefaultAsync();
        }


        public async Task<Guid> InsertIndividualClass(Guid userId, IndividualClass individualClass)
        {
            if (individualClass != null)
            {
                individualClass.ClassId = Guid.NewGuid();
                individualClass.CoachId = userId;
                individualClass.IsEnabled = true;
                individualClass.DateEnabled = Helper.GetDateTime();
                individualClass.CreatedBy = userId;
                individualClass.CreatedDate = Helper.GetDateTime();
                context.Add(individualClass);
                await context.SaveChangesAsync();
                return individualClass.ClassId;
            }
            return Guid.NewGuid();
        }

        public async Task DeleteClass(ChangeStatus classId)
        {
            var existingGroupClass = await context.GroupClasses.Where(g => g.GroupClassId == classId.GuID).FirstOrDefaultAsync();
            if (existingGroupClass != null)
            {
                context.GroupClasses.Remove(existingGroupClass);
            }

            var existingIndividualClass = await context.IndividualClasses.Where(i => i.ClassId == classId.GuID).FirstOrDefaultAsync();
            if (existingIndividualClass != null)
            {
                context.IndividualClasses.Remove(existingIndividualClass);

                var existingClassDetail = await context.IndividualClassDetails.Where(i => i.IndividualClassId == classId.GuID).FirstOrDefaultAsync();
                if (existingClassDetail != null)
                {
                    context.IndividualClassDetails.Remove(existingClassDetail);
                }
            }

            await context.SaveChangesAsync();

        }

        public async Task<ClassRenderViewModel> GetCoachingClass(Guid classId)
        {
            var viewModel = new ClassRenderViewModel();
            var players = new List<FacilityPlayer>();
            var users = await context.FacilityPlayers.ToListAsync();
            var groupClass = await context.GroupClasses.Where(g => g.GroupClassId == classId).FirstOrDefaultAsync();
            if (groupClass != null)
            {
                viewModel = new ClassRenderViewModel
                {
                    GroupClassId = groupClass.GroupClassId,
                    CoachId = groupClass.CoachId,
                    TrainingType = EBookingType.Group.ToString(),
                    ScheduleFrom = groupClass.Start.Value,
                    ScheduleTo = groupClass.Start.Value.AddHours(groupClass.Duration.Value),
                    Price = groupClass.Price,
                    IsEnabled = groupClass.IsEnabled.Value,
                    IsRepeat = groupClass.RepeatEveryWeek,
                    //AreaId = groupClass.AreaId,
                    GymId = groupClass.GymId.Value,
                    LevelId = groupClass.LevelId,
                    IsLocation = groupClass.IsLocation,
                    Participants = groupClass.Participants,
                    Description = groupClass.Notes,
                    Title = groupClass.Title,
                    Date = groupClass.Start.Value,
                };

                var getUser = await _userRepository.GetUser(viewModel.CoachId);
                if (getUser != null)
                {
                    viewModel.CoachName = $"{ getUser.FirstName} {getUser.LastName}";
                    viewModel.ImageUrl = getUser.ImageUrl;
                }

                var groupParticipants = await context.GroupBookings.Where(g => g.GroupClassId == viewModel.GroupClassId).ToListAsync();
                if (groupParticipants.Any())
                {
                    foreach (var item in groupParticipants)
                    {
                        var user = users.Where(u => u.UserId == item.ParticipantId).FirstOrDefault();
                        if (user != null)
                        {
                            players.Add(user);
                        }
                    }
                }

                viewModel.Players = players;

                return viewModel;
            }

            var individualClass = await context.IndividualClassDetails.Where(g => g.IndividualClassId == classId).FirstOrDefaultAsync();
            if (individualClass != null)
            {
                viewModel = new ClassRenderViewModel
                {
                    GroupClassId = individualClass.IndividualClassId,
                    TrainingType = EBookingType.Individual.ToString(),
                    Date = individualClass.Date,
                    ScheduleFrom = individualClass.Start,
                    ScheduleTo = individualClass.End,
                    Price = individualClass.Price,
                    IsEnabled = individualClass.IsEnabled.Value,
                    GymId = individualClass.GymId,
                    IsRepeat = individualClass.IsRepeatEveryWeek,
                    //AreaId = individualClass.AreaId,
                    LevelId = individualClass.LevelId,
                    IsLocation = !individualClass.IsOnline,
                    Participants = individualClass.Participants,
                    Description = individualClass.Description,
                    Title = individualClass.Title,
                };

                var coach = await context.IndividualClasses.Where(i => i.ClassId == viewModel.GroupClassId).FirstOrDefaultAsync();
                if (coach != null)
                {
                    viewModel.CoachId = coach.CoachId;
                    var getUser = await _userRepository.GetUser(viewModel.CoachId);
                    if (getUser != null)
                    {
                        viewModel.CoachName = $"{ getUser.FirstName} {getUser.LastName}";
                        viewModel.ImageUrl = getUser.ImageUrl;
                    }
                }

                var individualParticipants = await context.IndividualBookings.Where(g => g.ClassId == viewModel.GroupClassId).ToListAsync();
                if (individualParticipants.Any())
                {
                    foreach (var item in individualParticipants)
                    {
                        var user = users.Where(u => u.UserId == item.TraineeId).FirstOrDefault();
                        if (user != null)
                        {
                            players.Add(user);
                        }
                    }
                }

                viewModel.Players = players;

                return viewModel;
            }

            return viewModel;
        }

        public async Task CreateUpdateIndividualClass(Guid adminId, ClassRenderViewModel individualClass)
        {
            if (individualClass != null)
            {
                var existingClass = await context.IndividualClassDetails.Where(i => i.IndividualClassId == individualClass.GroupClassId).FirstOrDefaultAsync();
                if (existingClass == null)
                {
                    var newClass = new IndividualClass
                    {
                        ClassId = Guid.NewGuid(),
                        CoachId = individualClass.CoachId,
                        Price = individualClass.Price,

                        LastEditedBy = adminId,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = adminId,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsEnabledBy = adminId,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now
                    };

                    context.IndividualClasses.Add(newClass);

                    var classDetails = new IndividualClassDetails
                    {
                        IndividualClassId = newClass.ClassId,
                        LevelId = individualClass.LevelId,
                        Title = individualClass.Title,
                        Date = individualClass.Date,
                        IsRepeatEveryWeek = individualClass.IsRepeat,
                        Start = individualClass.ScheduleFrom,
                        End = individualClass.ScheduleTo,
                        IsOnline = !individualClass.IsLocation,
                        //AreaId = individualClass.AreaId,
                        GymId = individualClass.GymId,
                        Participants = individualClass.Participants,
                        Price = newClass.Price,
                        Description = individualClass.Description,

                        LastEditedBy = adminId,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = adminId,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsEnabledBy = adminId,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                    };

                    context.IndividualClassDetails.Add(classDetails);
                }
                else
                {
                    existingClass.Title = individualClass.Title;
                    existingClass.IsOnline = !individualClass.IsLocation;
                    existingClass.GymId = individualClass.GymId;
                    //existingClass.AreaId = individualClass.AreaId;
                    existingClass.Date = individualClass.Date;
                    existingClass.Start = individualClass.ScheduleFrom;
                    existingClass.End = individualClass.ScheduleTo;
                    existingClass.IsRepeatEveryWeek = individualClass.IsRepeat;
                    existingClass.Price = individualClass.Price;

                    existingClass.LastEditedBy = adminId;
                    existingClass.LastEditedDate = DateTime.Now;

                    context.IndividualClassDetails.Update(existingClass);

                    var existingMainClass = await context.IndividualClasses.Where(i => i.ClassId == existingClass.IndividualClassId).FirstOrDefaultAsync();
                    if (existingMainClass != null)
                    {
                        existingMainClass.Price = existingClass.Price;

                        existingMainClass.LastEditedBy = adminId;
                        existingMainClass.LastEditedDate = DateTime.Now;

                        context.IndividualClasses.Update(existingMainClass);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<IndividualClassDetails>> GetIndividualClassDetails(Guid individualClass)
        {
            return await context.IndividualClassDetails
                .Where(x => x.IndividualClassId == individualClass)
                .ToListAsync();
        }

        public async Task<IEnumerable<IndividualClassDetails>> GetLatestIndividualClassDetailsByCoach(Guid userId)
        {
            return await (from x in context.IndividualClassDetails
                          join y in context.IndividualClasses
                            on x.IndividualClassId equals y.ClassId
                          where y.CoachId == userId
                          && x.IsEnabled == true && y.IsEnabled == true
                          select x)
                          .ToListAsync();
        }

        public async Task InsertIndividualClassDetails(IEnumerable<IndividualClassDetails> details)
        {
            if (details.Any())
            {
                await context.IndividualClassDetails.AddRangeAsync(details);
                await context.SaveChangesAsync();
            }
        }

        public async Task DisableLatestIndividualClass(Guid userId,
            IndividualClass individualClass)
        {
            if (individualClass != null)
            {
                individualClass.IsEnabled = false;
                individualClass.LastEditedBy = userId;
                individualClass.LastEditedDate = Helper.GetDateTime();
                context.IndividualClasses.Update(individualClass);
                await context.SaveChangesAsync();
            }
        }

        public async Task DisableLatestIndividualClassDetails(Guid userId,
            IEnumerable<IndividualClassDetails> individualClassDetails)
        {
            if (individualClassDetails.Any())
            {
                var date = Helper.GetDateTime();
                individualClassDetails.ToList().ForEach(x =>
                {
                    x.IsEnabled = false;
                    x.DateEnabled = date;
                    x.LastEditedBy = userId;
                    x.LastEditedDate = date;
                });
                context.IndividualClassDetails.UpdateRange(individualClassDetails);
                await context.SaveChangesAsync();
            }
        }

        public async Task<GroupClass> GetGroupClass(Guid classId)
        {
            return await context.GroupClasses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.GroupClassId == classId);
        }

        public async Task<IEnumerable<GroupClass>> GetGroupClassesPerCoach(Guid userId)
        {
            return await context.GroupClasses
                .Where(x => x.CoachId == userId
                         && x.IsEnabled == true)
                .ToListAsync();
        }


        private IQueryable<GroupClassByFilterViewModel> GetQuery_GroupClass(Guid userId)
        {
            var userGoals = new List<UserGoalViewModel>();
            userGoals.Add(new UserGoalViewModel { GoalName = "Goal1" });
            userGoals.Add(new UserGoalViewModel { GoalName = "Goal2" });

            var myData = (from groupClass in context.GroupClasses
                          join user in context.Users
                       on groupClass.CoachId equals user.UserId
                          where groupClass.IsEnabled == true && groupClass.End >= Helper.GetDateTime() && groupClass.GymId != null
                          select new GroupClassByFilterViewModel()
                          {
                              GroupClassId = groupClass.GroupClassId,
                              dayOfWeek = groupClass.Start != null ? (int)Convert.ToDateTime(groupClass.Start).DayOfWeek : -1,
                              CoachId = groupClass.CoachId,
                              ChatReceiverId = user.Id,
                              CoachFirstName = user.FirstName,
                              CoachLastName = user.LastName,
                              CoachImage = user.ImageUrl,
                              CoachDescription = user.Description,
                              CoachGender = user.Gender,
                              Title = groupClass.Title,
                              ByLevel = groupClass.ByLevel,
                              LevelId = groupClass.LevelId,
                              Level = (context.Levels.Where(x => x.LevelId == groupClass.LevelId).Select(x => x.Name).FirstOrDefault()),
                              Start = groupClass.Start,
                              End = groupClass.Start.Value.AddHours(groupClass.Duration.GetValueOrDefault()),
                              StartTime = groupClass.StartTime,
                              EndTime = !string.IsNullOrWhiteSpace(groupClass.StartTime) ? TimeSpan.Parse(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TimeSpan.Parse(groupClass.StartTime).Hours, TimeSpan.Parse(groupClass.StartTime).Minutes, 0).AddHours((int)groupClass.Duration.GetValueOrDefault()).ToString("HH:mm")).ToString().Substring(0, 5) : "",
                              Duration = (int)(groupClass.Duration != null ? groupClass.Duration : null),
                              RepeatEveryWeek = groupClass.RepeatEveryWeek,
                              DuringNo = groupClass.DuringNo,
                              During = groupClass.During,
                              MaxParticipants = groupClass.Participants,
                              Price = groupClass.Price,
                              IsLocation = groupClass.IsLocation,
                              LocationId = groupClass.LocationId,
                              Location = (context.Locations.Where(x => x.LocationId == groupClass.LocationId).Select(x => x.Name).FirstOrDefault()),
                              IsOnline = groupClass.IsOnline,
                              Notes = groupClass.Notes,
                              UserGoals = userGoals,
                              GymLocation = (from gymlocation in context.GroupClasses
                                             join gyms in context.Gyms
                                             on gymlocation.GymId equals gyms.GymId
                                             where gymlocation.GymId == groupClass.GymId
                                             select new GymLocationViewModel()
                                             {
                                                 GymId = gyms.GymId,
                                                 GymAddress = gyms.GymAddress,
                                                 GymLat = gyms.GymLat,
                                                 GymLong = gyms.GymLong,
                                                 Image = gyms.Icon,
                                                 Gym = gyms.GymName
                                             }).ToList(),
                              Participants = (from participants in context.GroupBookings
                                              join participantProfile in context.Users
                                                 on participants.ParticipantId equals participantProfile.UserId
                                              where participants.GroupClassId == groupClass.GroupClassId && participants.IsPaid
                                              select new GroupClassParticipantsViewModel()
                                              {
                                                  ParticipantId = participantProfile.UserId,
                                                  FirstName = participantProfile.FirstName,
                                                  LastName = participantProfile.LastName,
                                                  Image = participantProfile.ImageUrl
                                              })
                                        .ToList(),
                              Specialties = (from coachSpeciality in context.CoachSpecialties
                                             join specialty in context.Specialties
                                              on coachSpeciality.SpecialtyId equals specialty.SpecialtyId
                                             where coachSpeciality.CoachUserId == user.UserId
                                             select specialty.Name)
                                       .ToList(),
                              Badges = (from coachBadges in context.UserPlayBadges
                                        join playBadges in context.Sports
                                          on coachBadges.SportId equals playBadges.SportId
                                        where coachBadges.UserId == user.UserId
                                        select playBadges.Name)
                                  .ToList(),
                              Gyms = (from coachGym in context.CoachGyms
                                      join gym in context.Gyms
                                          on coachGym.GymID equals gym.GymId
                                      where coachGym.CoachUserId == user.UserId
                                      select new GymViewModel
                                      {
                                          GymId = gym.GymId,
                                          Gym = gym.GymName,
                                          Image = gym.Icon
                                      })
                                .ToList(),
                              Languages = (from coachLanguage in context.CoachLanguages
                                           join language in context.Languages
                                              on coachLanguage.LanguageId equals language.LanguageId
                                           where coachLanguage.CoachUserId == user.UserId
                                           select new LanguageViewModel
                                           {
                                               LanguageId = language.LanguageId,
                                               Language = language._Language,
                                               Image = language.Icon
                                           })
                                     .ToList()
                          }); ;

            return myData;

        }

        public async Task<IEnumerable<GroupClassByFilterViewModel>> GetGroupClassByFilter(IEnumerable<FilterViewModel> filters, Guid userId)
        {
            var query = GetQuery_GroupClass(userId);
            bool isFilterByDate = false;
            // filter here!!!
            foreach (var filter in filters)
            {
                if (filter.FilterType == FilterType.Date)
                {
                    isFilterByDate = true;
                    var date = Convert.ToDateTime(filter.FilterValue);
                    query = query.ToList().Where(x => x.dayOfWeek == (int)(date).DayOfWeek && date <= x.End).AsQueryable();
                }

                if (filter.FilterType == FilterType.Area)
                {
                    query = query.Where(x => x.LocationId == Guid.Parse(filter.FilterValue));
                }

                if (filter.FilterType == FilterType.Price)
                {
                    var splPrice = filter.FilterValue.Split("-");
                    var firstValue = Convert.ToDecimal(splPrice[0]);
                    var secondValue = Convert.ToDecimal(splPrice[1]);

                    query = query.Where(x => x.Price >= firstValue && x.Price <= secondValue);
                }

                if (filter.FilterType == FilterType.Gender)
                {
                    Enum.TryParse(filter.FilterValue, out Genders gender);
                    query = query.Where(x => x.CoachGender == gender);
                }

                if (filter.FilterType == FilterType.Activity)
                {
                    // way to search from an array!!!
                }

                if (filter.FilterType == FilterType.GymCenter)
                {
                    //need to adjust
                    query = query.Where(x => x.Gyms.Select(y => y.GymId).Contains(Guid.Parse(filter.FilterValue)));
                }

                if (filter.FilterType == FilterType.Language)
                {
                    query = query.Where(a => a.Languages.Select(b => b.LanguageId).Contains(Guid.Parse(filter.FilterValue)));
                }
            }

            if (isFilterByDate)
                return query;
            else if (query != null)
                return query.ToList().Where(x => x.dayOfWeek == (int)(Helper.GetDateTime()).DayOfWeek);
            else
                return new List<GroupClassByFilterViewModel>();
        }

        public async Task<GroupClassByFilterViewModel> GetGroupClass_UserView(Guid classId, Guid userId)
        {
            var query = GetQuery_GroupClass(userId)
                .Where(x => x.GroupClassId == classId);
            return await query.FirstOrDefaultAsync();
        }
        public async Task InsertUpdateGroupClass(Guid userId, GroupClass groupClass)
        {
            var getGroupClass = await GetGroupClass(groupClass.GroupClassId);
            if (getGroupClass != null)
            {
                getGroupClass.CoachId = groupClass.CoachId;
                getGroupClass.Title = groupClass.Title;
                getGroupClass.ByLevel = groupClass.ByLevel;
                getGroupClass.LevelId = groupClass.LevelId;
                getGroupClass.Start = groupClass.Start;
                getGroupClass.End = groupClass.End;
                getGroupClass.RepeatEveryWeek = groupClass.RepeatEveryWeek;
                getGroupClass.DuringNo = groupClass.DuringNo;
                getGroupClass.During = groupClass.During;
                getGroupClass.Participants = groupClass.Participants;
                getGroupClass.Price = groupClass.Price;
                getGroupClass.IsLocation = groupClass.IsLocation;
                getGroupClass.LocationId = groupClass.LocationId;
                getGroupClass.IsOnline = groupClass.IsOnline;
                getGroupClass.Notes = groupClass.Notes;
                getGroupClass.GymId = groupClass.GymId;
                getGroupClass.Duration = groupClass.Duration;

                getGroupClass.LastEditedBy = userId;
                getGroupClass.LastEditedDate = Helper.GetDateTime();
                context.GroupClasses.Update(getGroupClass);
            }
            else
            {
                groupClass.GroupClassId = Guid.NewGuid();
                groupClass.CoachId = groupClass.CoachId;
                groupClass.CreatedBy = userId;
                groupClass.CreatedDate = Helper.GetDateTime();
                groupClass.IsEnabled = true;
                groupClass.DateEnabled = Helper.GetDateTime();
                context.GroupClasses.Add(groupClass);
            }
            await context.SaveChangesAsync();
        }

        public async Task<Filters> GetFilters()
        {
            IEnumerable<Area> areas = context.Areas
            .Where(x => x.IsEnabled == true)
            .ToList();  //Todo Alvin fix area issue

            IEnumerable<Location> locations = context.Locations
            .Where(x => x.IsEnabled == true)
            .ToList();

            var sports = await context.Sports
                .Where(x => x.IsEnabled == true)
                .ToListAsync();

            var gyms = await context.Gyms
                .Where(x => x.IsEnabled == true)
                .ToListAsync();

            var languages = await context.Languages
                .Where(x => x.IsEnabled == true)
                .ToListAsync();

            var levels = await context.Levels
                .Where(x => x.IsEnabled == true)
                .ToListAsync();

            IEnumerable<string> prices = new List<string>() { "50-100", "100-200", "200-300" };

            return new Filters
            {
                AreaFilters = areas,
                LocationFilters = locations,
                SportFilters = sports,
                GymFilters = gyms,
                LanguageFilters = languages,
                Prices = prices,
                LevelFilters = levels
            };
        }

        public async Task<IEnumerable<ClassRenderViewModel>> GetAllGroupClasses()
        {
            var groups = await context.GroupClasses.ToListAsync();
            var groupClasses = new List<ClassRenderViewModel>();
            foreach (var g in groups)
            {
                var newTest = new ClassRenderViewModel();
                newTest.GroupClassId = g.GroupClassId;
                newTest.CoachId = g.CoachId;
                newTest.TrainingType = "group";
                newTest.ScheduleFrom = g.Start.GetValueOrDefault();
                newTest.ScheduleTo = g.Start.GetValueOrDefault().AddHours(g.Duration.GetValueOrDefault());
                newTest.Price = g.Price;
                newTest.DateUpdated = g.LastEditedDate.Value;
                newTest.IsEnabled = g.IsEnabled.Value;
                newTest.CreatedDate = g.CreatedDate.GetValueOrDefault();

                groupClasses.Add(newTest);
            }

            var individualClasses = await context.IndividualClassDetails.Select(i => new ClassRenderViewModel
            {
                GroupClassId = i.IndividualClassId,
                TrainingType = "Individual",
                ScheduleFrom = i.Start,
                ScheduleTo = i.End,
                Price = i.Price,
                DateUpdated = i.LastEditedDate.Value,
                IsEnabled = i.IsEnabled.Value,
                CreatedDate = i.CreatedDate.GetValueOrDefault()
            }).ToListAsync();

            var commissions = await _commissionRepository.ComissionTrains();
            if (groupClasses.Any())
            {
                foreach (var groupClass in groupClasses)
                {
                    var getUser = await _userRepository.GetUser(groupClass.CoachId);
                    groupClass.CoachName = getUser != null ? $"{ getUser.FirstName} {getUser.LastName}" : string.Empty;
                    groupClass.CoachUserEmail = getUser != null ? getUser.Email : string.Empty;
                    var price = groupClass.Price;
                    if (price == default)
                    {
                        groupClass.Commission = default;
                    }
                    else
                    {
                        groupClass.Commission = commissions.Payload != null ? (commissions.Payload.CoachingGroupComission / groupClass.Price) * 100 : 0;
                    }

                }
            }

            var individualClassList = await context.IndividualClasses.ToListAsync();

            if (individualClasses.Any())
            {
                foreach (var individualClass in individualClasses)
                {
                    var classObject = individualClassList.Where(i => i.ClassId == individualClass.GroupClassId).FirstOrDefault();
                    if (classObject != null)
                    {
                        individualClass.CoachId = classObject.CoachId;
                        var getUser = await _userRepository.GetUser(individualClass.CoachId);
                        individualClass.CoachName = getUser != null ? $"{ getUser.FirstName} {getUser.LastName}" : string.Empty;
                        individualClass.CoachUserEmail = getUser != null ? getUser.Email : string.Empty;
                        var price = individualClass.Price;
                        if (price == default)
                        {
                            individualClass.Commission = default;
                        }
                        else
                        {
                            individualClass.Commission = commissions.Payload != null ? (commissions.Payload.CoachingIndividualComission / individualClass.Price) * 100 : 0;
                        }

                    }
                }

                groupClasses.AddRange(individualClasses);
            }

            return groupClasses.Any() ? groupClasses.OrderByDescending(g => g.DateUpdated).ToList() : groupClasses;
        }



        public async Task<IEnumerable<IndividualClassDetails>> GetIndividualClassDetailsByCoachForBetweenDate(Guid userId, DateTime startDate, DateTime EndDate, Guid ClassId)
        {
            var query = (from x in context.IndividualClassDetails.AsNoTracking()
                         join y in context.IndividualClasses.AsNoTracking()
                           on x.IndividualClassId equals y.ClassId
                         where y.CoachId == userId && y.ClassId != ClassId && (x.Start >= startDate || x.Start <= EndDate) && (x.End >= startDate || x.End <= EndDate)
                         && x.IsEnabled == true && y.IsEnabled == true
                         select x);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<GroupClass>> GetGroupClassDetailsByCoachForBetweenDate(Guid userId, DateTime startDate, DateTime EndDate, Guid GroupClassId)
        {
            var query = (from x in context.GroupClasses.AsNoTracking()
                         where x.CoachId == userId && x.GroupClassId != GroupClassId && (x.Start >= startDate && x.Start <= EndDate)
                         && x.IsEnabled == true
                         select x);

            return await query.ToListAsync();
        }
    }
}
