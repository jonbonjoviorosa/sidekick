using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class ClassHandler : IClassHandler
    {
        private readonly ILoggerManager loggerManager;
        private readonly IClassRepository classRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IUserHelper userHelper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ICoachRepository coachRepository;
        private readonly IBookingRepository bookingRepository;
        private readonly APIDBContext _dbContext;

        public ClassHandler(ILoggerManager loggerManager,
            IClassRepository classRepository,
            IMapper mapper,
            IUserRepository userRepository,
            IUserHelper userHelper,
            IUnitOfWork unitOfWork,
            ICoachRepository coachRepository,
            IBookingRepository bookingRepository,
            APIDBContext dbContext)
        {
            this.loggerManager = loggerManager;
            this.classRepository = classRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.userHelper = userHelper;
            this.unitOfWork = unitOfWork;
            this.coachRepository = coachRepository;
            this.bookingRepository = bookingRepository;
            _dbContext = dbContext;
        }

        public async Task<APIResponse<IEnumerable<IndividualClassViewModel>>> GetIndividualClasses()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);

                var responses = await classRepository.GetIndividualClasses();
                var mappedResponses = mapper.Map<IEnumerable<IndividualClassViewModel>>(responses);
                foreach (var item in mappedResponses)
                {
                    var getUser = await userRepository.GetUser(item.CoachId.Value);
                    item.CoachFirstName = getUser.FirstName;
                    item.CoachLastName = getUser.LastName;

                    var getIndividualClassDetails = await classRepository.GetIndividualClassDetails(item.ClassId.Value);
                    var mappedResponseDetails = mapper.Map<IEnumerable<IndividualClassDetailsViewModel>>(getIndividualClassDetails);
                    item.CustomSchedPrices = mappedResponseDetails;
                }

                return new APIResponse<IEnumerable<IndividualClassViewModel>>
                {
                    Status = Status.Success,
                    Payload = mappedResponses,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<IndividualClassViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<IndividualClassViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IndividualClassViewModel>> GetIndividualClass(Guid classId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await classRepository.GetIndividualClass(classId);
                if (response == null)
                {
                    return APIResponseHelper<IndividualClassViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                var getUser = await userRepository.GetUser(response.CoachId);
                var mappedResponse = mapper.Map<IndividualClassViewModel>(response);
                mappedResponse.CoachFirstName = getUser.FirstName;
                mappedResponse.CoachLastName = getUser.LastName;

                var getIndividualClassDetails = (await classRepository.GetIndividualClassDetails(classId)).Where(x => x.IsEnabled == true);
                var mappedResponseDetails = mapper.Map<IEnumerable<IndividualClassDetailsViewModel>>(getIndividualClassDetails);
                mappedResponse.CustomSchedPrices = mappedResponseDetails;

                return new APIResponse<IndividualClassViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IndividualClassViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<IndividualClassViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<IndividualClassByFilterViewModel>>> GetIndividualClassesByFilter(IEnumerable<FilterViewModel> filter)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(filter);

                var response = await classRepository.GetIndividualClassesByFilter(filter);
                var individualClasses = new List<IndividualClassByFilterViewModel>();

                var dateFilter = filter.FirstOrDefault(x => x.FilterType == FilterType.Date);

                DateTime bookdate = new DateTime();
                bool isDateFilter = false; 
                if(dateFilter !=null && !string.IsNullOrWhiteSpace(dateFilter.FilterValue))
                {
                    bookdate = Convert.ToDateTime(dateFilter.FilterValue);
                    isDateFilter = true;
                }
                foreach (var itemindividualClass in response)
                {
                    var individualClass = new IndividualClassByFilterViewModel();
                    individualClass.Badges = itemindividualClass.Badges;
                    individualClass.ClassId = itemindividualClass.ClassId;
                    individualClass.CoachAge = itemindividualClass.CoachAge;
                    individualClass.CoachDescrption = itemindividualClass.CoachDescrption;
                    individualClass.CoachFirstName = itemindividualClass.CoachDescrption;
                    individualClass.CoachGender = itemindividualClass.CoachGender;
                    individualClass.CoachId = itemindividualClass.CoachId;
                    individualClass.Title = itemindividualClass.Title;
                    individualClass.CoachImage = itemindividualClass.CoachImage;
                    individualClass.CoachLastName = itemindividualClass.CoachLastName;
                    individualClass.CoachNotAvailableSchedule = itemindividualClass.CoachNotAvailableSchedule;
                    individualClass.CoachSchedule = itemindividualClass.CoachSchedule;
                    individualClass.CustomSchedPrices = itemindividualClass.CustomSchedPrices;

                    individualClass.endTime = itemindividualClass.endTime;
                    individualClass.Friends = await userRepository.GetFriends(userHelper.GetCurrentUserGuidLogin(), null);
                    individualClass.Gyms = itemindividualClass.Gyms;

                    individualClass.Languages = itemindividualClass.Languages;
                    individualClass.Location = itemindividualClass.Location;
                    individualClass.Notation = itemindividualClass.Notation;
                    individualClass.ParticipateToOffer = itemindividualClass.ParticipateToOffer;

                    // Check everyday and custom price..


                    if (isDateFilter)
                    {
                        if (itemindividualClass.CustomSchedPrices != null)
                        {
                            if (itemindividualClass.CustomSchedPrices != null && itemindividualClass.CustomSchedPrices.Any())
                            {
                                var weekDay = Helper.GetDayFromDayName(bookdate.DayOfWeek.ToString());
                                if (weekDay != -1)
                                {
                                    var selectedDate = itemindividualClass.CustomSchedPrices.FirstOrDefault(a => (int)a.CoachingDay == (int)weekDay);
                                    if (selectedDate != null)
                                    {
                                        individualClass.Price = selectedDate.Price;
                                    }
                                }
                            }
                        }
                        else
                        {
                            individualClass.Price = itemindividualClass.Price;
                        }
                    }
                    else
                    {
                        individualClass.Price = itemindividualClass.Price;
                    }
                    
                    individualClass.Ratings = itemindividualClass.Ratings;
                    individualClass.Specialties = itemindividualClass.Specialties;

                    individualClass.startTime = itemindividualClass.startTime;
                    individualClass.CoachCustomSchedule = itemindividualClass.CoachCustomSchedule;
                    individualClass.CoachEverydayScheduleViewModel = itemindividualClass.CoachEverydayScheduleViewModel;
                    individualClass.CoachNotAvailableSchedule = itemindividualClass.CoachNotAvailableSchedule;

                    individualClasses.Add(individualClass);
                }


                return new APIResponse<IEnumerable<IndividualClassByFilterViewModel>>
                {
                    Status = Status.Success,
                    Payload = individualClasses.AsEnumerable(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<IndividualClassByFilterViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<IndividualClassByFilterViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetCoachingClass(Guid classId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(classId);
                var classObject = await classRepository.GetCoachingClass(classId);
                return new APIResponse
                {
                    Message = "Retrieved Coaching Class",
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = classObject
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IndividualClassByFilterViewModel>> GetIndividualClass_UserView(Guid classId, DateTime bookDate)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(classId);

                var response = await classRepository.GetIndividualClass_UserView(classId);
                if (response != null)
                {
                    response.CoachSchedule = await coachRepository.GetCoachSchedule(response.CoachId.Value);
                    if (response.CoachSchedule != null)
                    {
                        if (response.CoachSchedule.CustomSchedule != null && (response.CoachSchedule.CustomSchedule.Any() || response.CoachSchedule?.EverydaySchedule != null))
                        {
                            var weekDay = Helper.GetDayFromDayName(bookDate.DayOfWeek.ToString());
                            if (weekDay != -1)
                            {
                                var selectedDate = response.CoachSchedule.CustomSchedule.FirstOrDefault(a => (int)a.Day == (int)weekDay);
                                if (selectedDate != null)
                                {
                                    response.startTime = selectedDate.StartTime;
                                    response.endTime = selectedDate.EndTime;
                                }
                                else
                                {
                                    response.startTime = response.CoachSchedule?.EverydaySchedule?.StartTime;
                                    response.endTime = response.CoachSchedule?.EverydaySchedule?.EndTime;
                                }
                            }
                        }
                    }
                    var getLatestIndividualClassDetails = await classRepository.GetLatestIndividualClassDetailsByCoach(response.CoachId.Value);
                    var mappedResponseDetails = mapper.Map<IEnumerable<IndividualClassDetailsViewModel>>(getLatestIndividualClassDetails);
                    response.CustomSchedPrices = mappedResponseDetails;
                    if (response.CustomSchedPrices != null)
                    {
                        if (response.CustomSchedPrices != null && response.CustomSchedPrices.Any())
                        {
                            var weekDay = Helper.GetDayFromDayName(bookDate.DayOfWeek.ToString());
                            if (weekDay != -1)
                            {
                                var selectedDate = response.CustomSchedPrices.FirstOrDefault(a => (int)a.CoachingDay == (int)weekDay);
                                if (selectedDate != null)
                                {
                                    response.Price = selectedDate.Price;
                                }
                            }
                        }
                    }
                    response.CoachNotAvailableSchedule = await bookingRepository.GetCoachNotAvailableSched(response.CoachId.Value);

                    response.Notation = await userRepository.GetEnumUserReviews(response.CoachId.Value);
                    response.Friends = await userRepository.GetFriends(userHelper.GetCurrentUserGuidLogin(), null);

                }
                return new APIResponse<IndividualClassByFilterViewModel>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IndividualClassByFilterViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IndividualClassByFilterViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<GroupClassByFilterViewModel>>> GetGroupClassByFilter(IEnumerable<FilterViewModel> filter)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(filter);

                var response = await classRepository.GetGroupClassByFilter(filter, userHelper.GetCurrentUserGuidLogin());
                return new APIResponse<IEnumerable<GroupClassByFilterViewModel>>
                {
                    Message = Messages.Success,
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                //unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<GroupClassByFilterViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<GroupClassByFilterViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<GroupClassByFilterViewModel>> GetGroupClass_UserView(Guid classId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(classId);

                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var response = await classRepository.GetGroupClass_UserView(classId, currentLogin);
                response.Notation = await userRepository.GetGroupUserReviews(response.CoachId.Value);

                return new APIResponse<GroupClassByFilterViewModel>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<GroupClassByFilterViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<GroupClassByFilterViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> DeleteClass(ChangeStatus classId)
        {
            try
            {
                unitOfWork.BeginTransaction();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogDebugObject(classId);
                await classRepository.DeleteClass(classId);
                unitOfWork.CommitTransaction();
                return new APIResponse
                {
                    Message = "Successfully Deleted Class",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.DELETE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> CreateUpdateIndividualClass(string auth, ClassRenderViewModel individualClass)
        {
            try
            {
                unitOfWork.BeginTransaction();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(individualClass);
                var isLoggedIn = _dbContext.AdminLoginTransactions.FirstOrDefault(alt => alt.Token == auth);
                if (isLoggedIn == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);
                }

                // check for coach..

                DateTime ScheduledStartTime = new DateTime();
                DateTime ScheduledEndTime = new DateTime();
                var CoachSchedule = await coachRepository.GetCoachSchedule(individualClass.CoachId);
                if (CoachSchedule != null)
                {
                    if (CoachSchedule.CustomSchedule != null && (CoachSchedule.CustomSchedule.Any() || CoachSchedule?.EverydaySchedule != null))
                    {
                        var weekDay = Helper.GetDayFromDayName(individualClass.Date.DayOfWeek.ToString());
                        if (weekDay != -1)
                        {
                            var selectedDate = CoachSchedule.CustomSchedule.FirstOrDefault(a => (int)a.Day == (int)weekDay);
                            if (selectedDate != null)
                            {
                                DateTime.TryParseExact(selectedDate.StartTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledStartTime);
                                DateTime.TryParseExact(selectedDate.EndTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledEndTime);
                            }
                            else
                            {

                                DateTime.TryParseExact(CoachSchedule?.EverydaySchedule?.StartTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledStartTime);
                                DateTime.TryParseExact(CoachSchedule?.EverydaySchedule?.EndTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledEndTime);
                            }
                        }
                    }
                }
                ScheduledStartTime = new DateTime(individualClass.ScheduleFrom.Year, individualClass.ScheduleFrom.Month, individualClass.ScheduleFrom.Day, ScheduledStartTime.Hour, ScheduledStartTime.Minute, ScheduledStartTime.Second);
                ScheduledEndTime = new DateTime(individualClass.ScheduleTo.Year, individualClass.ScheduleTo.Month, individualClass.ScheduleTo.Day, ScheduledEndTime.Hour, ScheduledEndTime.Minute, ScheduledEndTime.Second);
                // check start time and end time is found for coach or not.
                if (!(individualClass.ScheduleFrom >= ScheduledStartTime && individualClass.ScheduleTo <= ScheduledEndTime))
                {
                    return new APIResponse
                    {
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Coach schedule is not available for given time."
                    };
                }

                // check for coach over lapping booking..
                // check individual class and group class time..
                var classList = await classRepository.GetIndividualClassDetailsByCoachForBetweenDate(individualClass.CoachId, individualClass.ScheduleFrom, individualClass.ScheduleTo, individualClass.GroupClassId);

                if (classList != null && classList.Any())
                {
                    return new APIResponse
                    {
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Coach session is already available for specified time."
                    };
                }

                // check here for group class..
                DateTime groupStartDate = individualClass.ScheduleFrom.Date;
                DateTime groupEndDate = new DateTime(individualClass.ScheduleFrom.Year, individualClass.ScheduleFrom.Month, individualClass.ScheduleFrom.Day, 23, 59, 59);

                var groupClassList = await classRepository.GetGroupClassDetailsByCoachForBetweenDate(individualClass.CoachId, groupStartDate, groupEndDate,individualClass.GroupClassId);
                if (groupClassList != null && groupClassList.Any())
                {
                    foreach (var item in groupClassList)
                    {
                        DateTime StartTime = new DateTime();
                        DateTime.TryParseExact(item.StartTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out StartTime);
                        DateTime startdateTime = new DateTime(item.Start.Value.Year, item.Start.Value.Month, item.Start.Value.Day, StartTime.Hour, StartTime.Minute, 0);

                        item.Start = startdateTime;
                        if (item.Duration != 0)
                        {
                            item.End = startdateTime.AddHours(item.Duration.Value);
                        }
                    }

                    var groupClassExistsList = groupClassList.Where(x => (x.Start.Value >= individualClass.ScheduleFrom && x.Start.Value <= individualClass.ScheduleTo) && (x.End.Value >= individualClass.ScheduleFrom && x.End.Value <= individualClass.ScheduleTo));
                    if (groupClassExistsList != null && groupClassExistsList.Any())
                    {
                        return new APIResponse
                        {
                            Status = Status.Failed,
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            Message = "Coach group session is already available for specified time."
                        };
                    }
                }
                await classRepository.CreateUpdateIndividualClass(isLoggedIn.AdminId, individualClass);
                unitOfWork.CommitTransaction();
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> InsertIndividualClass(IndividualClassViewModel individualClass)
        {
            try
            {
                unitOfWork.BeginTransaction();
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(individualClass);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getLatestIndividualClass = await classRepository.GetLatestIndividualClassByCoach(currentLogin);
                await classRepository.DisableLatestIndividualClass(currentLogin, getLatestIndividualClass);
                var mappedIndividualClass = mapper.Map<IndividualClass>(individualClass);
                var individualClassId = await classRepository.InsertIndividualClass(currentLogin, mappedIndividualClass);

                var getLatestIndividualClassDetails = await classRepository.GetLatestIndividualClassDetailsByCoach(currentLogin);
                await classRepository.DisableLatestIndividualClassDetails(currentLogin, getLatestIndividualClassDetails);
                var mappedIndividualClassDetails = mapper.Map<IEnumerable<IndividualClassDetails>>(individualClass.CustomSchedPrices);
                if (mappedIndividualClassDetails.Any())
                {
                    mappedIndividualClassDetails.ToList()
                        .ForEach(x =>
                        {
                            x.CreatedBy = currentLogin;
                            x.CreatedDate = Helper.GetDateTime();
                            x.IsEnabled = true;
                            x.IndividualClassId = individualClassId;
                        });
                    await classRepository.InsertIndividualClassDetails(mappedIndividualClassDetails);

                }

                unitOfWork.CommitTransaction();
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<IndividualClassViewModel>> GetCoachIndividualClass(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await classRepository.GetLatestIndividualClassByCoach(userId);
                var getUser = await userRepository.GetUser(userId);
                var mappedResponse = mapper.Map<IndividualClassViewModel>(response);
                if (mappedResponse == null)
                {
                    mappedResponse = new IndividualClassViewModel();
                }

                mappedResponse.CoachFirstName = getUser.FirstName;
                mappedResponse.CoachLastName = getUser.LastName;

                var getLatestIndividualClassDetails = await classRepository.GetLatestIndividualClassDetailsByCoach(userId);
                var mappedResponseDetails = mapper.Map<IEnumerable<IndividualClassDetailsViewModel>>(getLatestIndividualClassDetails);
                mappedResponse.CustomSchedPrices = mappedResponseDetails;

                var schedule = new CoachScheduleViewModel();

                var customSched = await coachRepository.GetCustomSchedule(userId);
                var everydaySched = await coachRepository.GetEverydaySchedule(userId);
                var mappedCustomSched = mapper.Map<IEnumerable<CoachCustomScheduleViewModel>>(customSched);
                var mappedEverydaySched = mapper.Map<CoachEverydayScheduleViewModel>(everydaySched);
                schedule = new CoachScheduleViewModel()
                {
                    CustomSchedule = mappedCustomSched,
                    EverydaySchedule = mappedEverydaySched,
                    IsWeekPersonalized = mappedCustomSched.Any() ? true : false
                };

                mappedResponse.CoachSchedule = schedule;

                return new APIResponse<IndividualClassViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IndividualClassViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<IndividualClassViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<GroupClassViewModel>> GetGroupClass(Guid classId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await classRepository.GetGroupClass(classId);
                if (response == null)
                {
                    return APIResponseHelper<GroupClassViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                var getUser = await userRepository.GetUser(response.CoachId);
                var mappedResponse = mapper.Map<GroupClassViewModel>(response);
                mappedResponse.CoachFirstName = getUser.FirstName;
                mappedResponse.CoachLastName = getUser.LastName;

                return new APIResponse<GroupClassViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<GroupClassViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<GroupClassViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetAllGroupClasses()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await classRepository.GetAllGroupClasses();
                if (response == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }


                return new APIResponse
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<GroupClassViewModel>>> GetGroupClassesPerCoach(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var responses = await classRepository.GetGroupClassesPerCoach(userId);
                var mappedResponses = mapper.Map<IEnumerable<GroupClassViewModel>>(responses);
                foreach (var item in mappedResponses)
                {
                    var getUser = await userRepository.GetUser(item.CoachId.Value);
                    item.CoachFirstName = getUser.FirstName;
                    item.CoachLastName = getUser.LastName;
                }

                return new APIResponse<IEnumerable<GroupClassViewModel>>
                {
                    Status = Status.Success,
                    Payload = mappedResponses,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<GroupClassViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<GroupClassViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> InsertUpdateGroupClass(GroupClassViewModel groupClass)
        {
            try
            {
                unitOfWork.BeginTransaction();
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                if (groupClass.Start.HasValue && string.IsNullOrWhiteSpace(groupClass.StartTime))
                {
                    groupClass.StartTime = groupClass.Start.Value.ToString("HH:mm");
                }
                else
                {
                    DateTime dateTime = new DateTime();
                    DateTime.TryParseExact(groupClass.StartTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dateTime);
                    groupClass.Start = new DateTime(groupClass.Start.Value.Year, groupClass.Start.Value.Month, groupClass.Start.Value.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                }
                if (groupClass.RepeatEveryWeek)
                {
                    if (groupClass.During == EDuring.Month)
                    {
                        groupClass.End = groupClass.Start.GetValueOrDefault().AddMonths((int)groupClass.DuringNo);
                    }
                    else if (groupClass.During == EDuring.Week)
                    {
                        groupClass.End = groupClass.Start.GetValueOrDefault().AddDays(7 * (int)groupClass.DuringNo);
                    }
                    else if (groupClass.During == EDuring.Day)
                    {
                        groupClass.End = groupClass.Start.GetValueOrDefault().AddDays((int)groupClass.DuringNo);
                    }
                    else if (groupClass.During == EDuring.Year)
                    {
                        groupClass.End = groupClass.Start.GetValueOrDefault().AddYears((int)groupClass.DuringNo);
                    }

                }
                else
                {
                    groupClass.End = groupClass.Start;
                }

                groupClass.End=new DateTime(groupClass.End.GetValueOrDefault().Year, groupClass.End.GetValueOrDefault().Month, groupClass.End.GetValueOrDefault().Day, 23, 59, 59);
                

                //TODO:Bhavik for check group time duplicate slot
                // check here given time is available for coach..

                if (groupClass.CoachId.HasValue && groupClass.Start.HasValue)
                {
                    // changes for start and end date..
                    DateTime RequestedStartTime = new DateTime(groupClass.Start.GetValueOrDefault().Year, groupClass.Start.GetValueOrDefault().Month, groupClass.Start.GetValueOrDefault().Day, groupClass.Start.GetValueOrDefault().Hour, groupClass.Start.GetValueOrDefault().Minute, groupClass.Start.GetValueOrDefault().Second);
                    DateTime RequestedEndTime = new DateTime(groupClass.Start.GetValueOrDefault().Year, groupClass.Start.GetValueOrDefault().Month, groupClass.Start.GetValueOrDefault().Day, groupClass.Start.GetValueOrDefault().Hour + groupClass.Duration, groupClass.Start.GetValueOrDefault().Minute, groupClass.Start.GetValueOrDefault().Second);

                    DateTime ScheduledStartTime = new DateTime();
                    DateTime ScheduledEndTime = new DateTime();
                    var CoachSchedule = await coachRepository.GetCoachSchedule(groupClass.CoachId.Value);
                    if (CoachSchedule != null)
                    {
                        if (CoachSchedule.CustomSchedule != null && (CoachSchedule.CustomSchedule.Any() || CoachSchedule?.EverydaySchedule != null))
                        {
                            var weekDay = Helper.GetDayFromDayName(groupClass.Start.Value.Date.DayOfWeek.ToString());
                            if (weekDay != -1)
                            {
                                var selectedDate = CoachSchedule.CustomSchedule.FirstOrDefault(a => (int)a.Day == (int)weekDay);
                                if (selectedDate != null)
                                {
                                    DateTime.TryParseExact(selectedDate.StartTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledStartTime);
                                    DateTime.TryParseExact(selectedDate.EndTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledEndTime);
                                }
                                else
                                {

                                    DateTime.TryParseExact(CoachSchedule?.EverydaySchedule?.StartTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledStartTime);
                                    DateTime.TryParseExact(CoachSchedule?.EverydaySchedule?.EndTime, "hh:mm tt", null, System.Globalization.DateTimeStyles.None, out ScheduledEndTime);
                                }
                            }
                        }
                    }
                    ScheduledStartTime = new DateTime(groupClass.Start.Value.Year, groupClass.Start.Value.Month, groupClass.Start.Value.Day, ScheduledStartTime.Hour, ScheduledStartTime.Minute, ScheduledStartTime.Second);
                    ScheduledEndTime = new DateTime(groupClass.Start.Value.Year, groupClass.Start.Value.Month, groupClass.Start.Value.Day, ScheduledEndTime.Hour, ScheduledEndTime.Minute, ScheduledEndTime.Second);
                    // check start time and end time is found for coach or not.
                    if (!(RequestedStartTime >= ScheduledStartTime && RequestedEndTime <= ScheduledEndTime))
                    {
                        return new APIResponse
                        {
                            Status = Status.Failed,
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            Message = "Coach schedule is not available for given time."
                        };
                    }

                    // check for coach over lapping booking..
                    // check individual class and group class time..
                    // removed this code on 23022022 as individual class is only add while update coach price and there is no any start and end time
                    //var classList = await classRepository.GetIndividualClassDetailsByCoachForBetweenDate(groupClass.CoachId.Value, RequestedStartTime, RequestedEndTime, groupClass.GroupClassId.GetValueOrDefault());

                    //if (classList != null && classList.Any())
                    //{
                    //    return new APIResponse
                    //    {
                    //        Status = Status.Failed,
                    //        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    //        Message = "Coach session is already available for specified time."
                    //    };
                    //}

                    // check here for group class..
                    DateTime groupStartDate = groupClass.Start.Value.Date;
                    DateTime groupEndDate = new DateTime(groupClass.Start.Value.Year, groupClass.Start.Value.Month, groupClass.Start.Value.Day, 23, 59, 59);

                    var groupClassList = await classRepository.GetGroupClassDetailsByCoachForBetweenDate(groupClass.CoachId.Value, groupStartDate, groupEndDate, groupClass.GroupClassId.GetValueOrDefault());
                    if (groupClassList != null && groupClassList.Any())
                    {
                        foreach (var item in groupClassList)
                        {
                            DateTime StartTime = new DateTime();
                            DateTime.TryParseExact(item.StartTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out StartTime);
                            DateTime startdateTime = new DateTime(item.Start.Value.Year, item.Start.Value.Month, item.Start.Value.Day, StartTime.Hour, StartTime.Minute, 0);

                            item.Start = startdateTime;
                            if (item.Duration != 0)
                            {
                                item.End = startdateTime.AddHours(item.Duration.Value);
                            }

                        }

                        var groupClassExistsList = groupClassList.Where(x => (x.Start.Value >= RequestedStartTime && x.Start.Value <= RequestedEndTime) && (x.End.Value >= RequestedStartTime && x.End.Value <= RequestedEndTime));
                        if (groupClassExistsList != null && groupClassExistsList.Any())
                        {
                            return new APIResponse
                            {
                                Status = Status.Failed,
                                StatusCode = System.Net.HttpStatusCode.BadRequest,
                                Message = "Coach group session is already available for specified time."
                            };
                        }
                    }
                }


                var mappedGroupClass = mapper.Map<GroupClass>(groupClass);
                await classRepository.InsertUpdateGroupClass(currentLogin, mappedGroupClass);
                unitOfWork.CommitTransaction();
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<Filters>> GetAllFilters()
        {
            try
            {
                var getAllFilters = await classRepository.GetFilters();

                return new APIResponse<Filters>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getAllFilters
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<Filters>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<Filters>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

    }
}
