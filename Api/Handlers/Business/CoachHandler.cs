using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class CoachHandler : ICoachHandler
    {
        private readonly ICoachRepository coachRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILoggerManager loggerManager;
        private readonly IUserHelper userHelper;
        private readonly IMapper mapper;
        private readonly IClassRepository classRepository;

        public CoachHandler(ICoachRepository coachRepository,
            IUnitOfWork unitOfWork,
            ILoggerManager loggerManager,
            IUserHelper userHelper,
            IMapper mapper, 
            IClassRepository classRepository)
        {
            this.coachRepository = coachRepository;
            this.unitOfWork = unitOfWork;
            this.loggerManager = loggerManager;
            this.userHelper = userHelper;
            this.mapper = mapper;
            this.classRepository = classRepository;
        }

        public async Task<APIResponse<IEnumerable<CoachViewModel>>> GetCoaches()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoaches = await coachRepository.GetCoaches();

                return new APIResponse<IEnumerable<CoachViewModel>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoaches
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<CoachViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<IEnumerable<CoachViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> BecomeACoach(CoachUpdateProfile updateProf)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                unitOfWork.BeginTransaction();
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                if (updateProf.UserID == Guid.Empty)
                {                   
                    var getCoach = await coachRepository.GetCoach(currentLogin);
                    if (getCoach != null)
                    {
                        return new APIResponse
                        {
                            Message = Messages.AlreadyACoach,
                            Status = Status.Failed,
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                        };
                    }
                }
               
                currentLogin = updateProf.UserID != Guid.Empty ? updateProf.UserID : currentLogin;

                await coachRepository.UpdateUserFromCoachProfile(currentLogin, updateProf.DateOfBirth, updateProf.Description, updateProf.Gender);
                await coachRepository.InsertUpdateCoach(currentLogin, updateProf);

                var getSpecialties = await coachRepository.GetSpecialties(currentLogin);
                await coachRepository.DeleteSpecialties(getSpecialties);
                await coachRepository.InsertSpecialties(currentLogin, updateProf.Specialties);

                var getLanguages = await coachRepository.GetLanguages(currentLogin);
                await coachRepository.DeleteLanguages(getLanguages);
                await coachRepository.InsertLanguages(currentLogin, updateProf.Languages);

                var getGymsAccess = await coachRepository.GetGymsAccess(currentLogin);
                await coachRepository.DeleteGymsAccess(getGymsAccess);
                await coachRepository.InsertGymsAccess(currentLogin, updateProf.GymsAccess);

                var mappedSched = mapper.Map<CoachScheduleViewModel>(updateProf);
                var apiResponse = await InsertUpdateSchedule(currentLogin, mappedSched);
                if (apiResponse != null)
                {
                    unitOfWork.RollbackTransaction();
                    return apiResponse;
                }
                unitOfWork.CommitTransaction();

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InsertSuccess);
            }
            catch (Exception ex)
            {
                unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<CoachScheduleViewModel>> GetCoachSchedule(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoach(userId);
                if (getCoach == null)
                {
                    return APIResponseHelper<CoachScheduleViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var schedule = await coachRepository.GetCoachSchedule(userId);

                return new APIResponse<CoachScheduleViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = schedule
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<CoachScheduleViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<CoachScheduleViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> UpdateCoachSchedule(CoachScheduleViewModel sched)
        {
            try
            {
                var apiResponse = new APIResponse();
                unitOfWork.BeginTransaction();
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                apiResponse = await InsertUpdateSchedule(currentLogin, sched);

                if (apiResponse != null)
                {
                    unitOfWork.RollbackTransaction();
                    return apiResponse;
                }
                unitOfWork.CommitTransaction();

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.UpdateSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetTrainingBookings()
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
            try
            {               
                var apiResponse = new APIResponse();
                var trainingBookings = await coachRepository.GetTrainingBookings();

                return new APIResponse
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = trainingBookings
                };
            }
            catch(Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        private async Task<APIResponse> InsertUpdateSchedule(Guid userId,
            CoachScheduleViewModel sched)
        {
            if (!sched.IsWeekPersonalized)
            {
                if (sched.CustomSchedule.Any())
                {
                    return new APIResponse
                    {
                        Message = Messages.CustomScheduleShouldBeEmpty,
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    };
                }
            }


            if (sched.CustomSchedule.Any() || sched.EverydaySchedule != null)
            {
                var getEverydaySchedule = await coachRepository.GetEverydaySchedule(userId);
                await coachRepository.DeleteEverydaySchedule(getEverydaySchedule);
                await coachRepository.InsertEverydaySchedule(userId, sched.EverydaySchedule);

                var getCustomSchedule = await coachRepository.GetCustomSchedule(userId);
                await coachRepository.DeleteCustomSchedule(getCustomSchedule);
                await coachRepository.InsertCustomSchedule(userId, sched.CustomSchedule);


                var getIndividualClassDetails = await classRepository.GetLatestIndividualClassDetailsByCoach(userId);
                if (getIndividualClassDetails != null)
                {
                    if (!sched.IsWeekPersonalized)
                    {
                        await classRepository.DisableLatestIndividualClassDetails(userId, getIndividualClassDetails);
                    }
                    else
                    {
                        var notExistingCustomSched = getIndividualClassDetails
                            .Where(x => !sched.CustomSchedule.Select(y => y.Day).Contains(x.CoachingDay.Value))
                            .ToList();
                        await classRepository.DisableLatestIndividualClassDetails(userId, notExistingCustomSched);
                    }
                }
            }
            return null;
        }
        public async Task<APIResponse<CoachInfoViewModel>> GetCoache(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoache(userId);
                if (getCoach == null)
                {
                    return APIResponseHelper<CoachInfoViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<CoachInfoViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<CoachInfoViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<CoachInfoViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public APIResponse EditCoachProfile(CoachEditProfileFormModel userProfile)
        {
           // unitOfWork.BeginTransaction();
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(userProfile);
                return coachRepository.EditCoachProfile(userProfile);
            }
            catch (Exception ex)
            {
             //   unitOfWork.RollbackTransaction();

                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<CoachProfileViewModel>> GetCoacheProfile(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoacheProfile(userId);
                if (getCoach == null)
                {
                    return APIResponseHelper<CoachProfileViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<CoachProfileViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<CoachProfileViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<CoachProfileViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<MyCoachingGroupListViewModel>> GetMyCoachingGroupList(Guid userId,string title)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetMyCoachingGroupList(userId, title);
                if (getCoach == null)
                {
                    return APIResponseHelper<MyCoachingGroupListViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<MyCoachingGroupListViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<MyCoachingGroupListViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<MyCoachingGroupListViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<MyIndividualGroupListViewModel>> GetMyIndividualGroupList(Guid userId,string name)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetMyIndividualGroupList(userId, name);
                if (getCoach == null)
                {
                    return APIResponseHelper<MyIndividualGroupListViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<MyIndividualGroupListViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<MyIndividualGroupListViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<MyIndividualGroupListViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<List<CoachAbsentSlotViewModel>>> GetCoachAbsentSlot(Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoachAbsentSlot(userId);
                if (getCoach == null)
                {
                    return APIResponseHelper<List<CoachAbsentSlotViewModel>>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<List<CoachAbsentSlotViewModel>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<List<CoachAbsentSlotViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<List<CoachAbsentSlotViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<GetCoachAbsentSlotDetailsViewModel>> GetCoachAbsentSlotDetails(int id)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoachAbsentSlotDetails(id);
                if (getCoach == null)
                {
                    return APIResponseHelper<GetCoachAbsentSlotDetailsViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<GetCoachAbsentSlotDetailsViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<GetCoachAbsentSlotDetailsViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<GetCoachAbsentSlotDetailsViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> UpdateCoachAbsentSlot(CoachCoachAbsentSlotViewModel absentSlot,int id)
        {
            try
            {
                var apiResponse = new APIResponse();
                unitOfWork.BeginTransaction();
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                await coachRepository.UpdateCoachAbsentSlot(currentLogin, absentSlot,id);

                unitOfWork.CommitTransaction();

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.UpdateSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> DeleteCoachAbsentSlot(int id)
        {
            try
            {
                var apiResponse = new APIResponse();
                unitOfWork.BeginTransaction();
                await coachRepository.DeleteCoachAbsentSlot(id);

                unitOfWork.CommitTransaction();

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.DeleteSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<GetInsightActivitiesViewModel>> GetCoachInsightActivities(int searchType, Guid userId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoachInsightActivities(searchType, userId);
                if (getCoach == null)
                {
                    return APIResponseHelper<GetInsightActivitiesViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<GetInsightActivitiesViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<GetInsightActivitiesViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<GetInsightActivitiesViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<List<CoachCustomerListViewModel>>> GetCoachCustomer(Guid userId, string name)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCoach = await coachRepository.GetCoachCustomer(userId, name);
                if (getCoach == null)
                {
                    return APIResponseHelper<List<CoachCustomerListViewModel>>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                return new APIResponse<List<CoachCustomerListViewModel>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getCoach
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<List<CoachCustomerListViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<List<CoachCustomerListViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
    }
}
