using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Model;
using Sidekick.Model.UserNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Sidekick.Api.Handlers.Business
{
    public class UserNotificationHandler : IUserNotificationHandler
    {
        private readonly IUserNotificationRepository usernotificationRepository;
        private readonly IMapper mapper;
        private readonly ILoggerManager loggerManager;
        private readonly APIConfigurationManager aPIConfigurationManager;

        public UserNotificationHandler(IUserNotificationRepository usernotificationRepository,
            IMapper mapper,
            ILoggerManager loggerManager, APIConfigurationManager aPIConfigurationManager)
        {
            this.usernotificationRepository = usernotificationRepository;
            this.mapper = mapper;
            this.loggerManager = loggerManager;
            this.aPIConfigurationManager = aPIConfigurationManager;
        }

        public async Task<APIResponse<UserNotificationViewModel>> GetNotifcation(Guid usernofication)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await usernotificationRepository.GetNotification(usernofication);
                if (response == null)
                    response = new UserNotification();

                var mappedResponse = mapper.Map<UserNotificationViewModel>(response);
                return new APIResponse<UserNotificationViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<UserNotificationViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper<UserNotificationViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<List<UserNotificationViewModel>>> GetUserNotifcation()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await usernotificationRepository.GetUserNotification();
                if (response == null)
                    response = null;

                var mappedResponse = mapper.Map<List<UserNotificationViewModel>>(response);
                return new APIResponse<List<UserNotificationViewModel>>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<List<UserNotificationViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper<List<UserNotificationViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> GetNotifications(Guid facilityId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var notifications = await usernotificationRepository.GetNotifications(facilityId);
                if (facilityId == Guid.Empty)
                {
                    return new APIResponse
                    {
                        Message = "FacilityId is not existing",

                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

                return new APIResponse
                {
                    Payload = notifications,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

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

        public async Task<APIResponse> InsertUpdateNotification(UserNotificationViewModel notification)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogDebugObject(notification);
                var mappedResponse = mapper.Map<UserNotification>(notification);
                await usernotificationRepository.InsertUpdateNotification(mappedResponse);
                return new APIResponse
                {
                    Status = Status.Success,
                    Message = Messages.NoticationUpdateSuccess,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        #region INApp notification common templates

        public async Task PitchBookingCancellationFromCaptainMorethan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromCaptainMorethan24HoursToCaptain,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainMorethan24HoursToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers,
                commonTemplate.CaptainName,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromPlayerMorethan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromPlayerMorethan24HoursToPlayer,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromPlayerMorethan24HoursToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromPlayerMorethan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromPlayerMorethan24HoursToCaptain,
                commonTemplate.UserName,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromPlayerMorethan24HoursToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromCaptainLessthan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromCaptainLessthan24HoursToCaptain,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainLessthan24HoursToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromCaptainLessthan24HoursToAllPlayers,
                commonTemplate.CaptainName,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromPlayerLessthan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromPlayerLessthan24HoursToPlayer,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromPlayerMorethan24HoursToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromPlayerLessthan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromPlayerLessthan24HoursToCaptain,
                commonTemplate.UserName,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromPlayerMorethan24HoursToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromFacilityMorethan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromFacilityMorethan24HoursToCaptain,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime,
                commonTemplate.FacilityName
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromFacilityMorethan24HoursToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime,
                commonTemplate.FacilityName
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromFacilityLessthan24HoursToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromFacilityLessthan24HoursToCaptain,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime,
                commonTemplate.FacilityName
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromFacilityMorethan24HoursToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime,
                commonTemplate.FacilityName
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainMorethan24HoursToAllPlayers,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task IndividualCoachingBookingConfirmationToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.IndividualCoachingBookingConfirmationToPlayer,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.IndividualCoachingBookingConfirmationToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task GroupCoachingBookingConfirmationToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.GroupCoachingBookingConfirmationToPlayer,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.GroupCoachingBookingConfirmationToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task CancellationCoachingFromUserMorethan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.CancellationCoachingFromUserMorethan24HoursToPlayer,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.CancellationCoachingFromUserMorethan24HoursToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task CancellationCoachingFromUserLessthan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.CancellationCoachingFromUserLessthan24HoursToPlayer,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.CancellationCoachingFromUserLessthan24HoursToPlayer,
                NotificationType = commonTemplate.NotificationType
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task CancellationCoachingFromCoachMorethan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.CancellationCoachingFromCoachMorethan24HoursToPlayer,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.CancellationCoachingFromCoachMorethan24HoursToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task CancellationCoachingFromCoachLessthan24HoursToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.CancellationCoachingFromCoachLessthan24HoursToPlayer,
                commonTemplate.Activity,
                commonTemplate.CoachName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.CancellationCoachingFromCoachLessthan24HoursToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingConfirmationToCaptain(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingConfirmationToCaptain,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingConfirmationToCaptain,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingConfirmationToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingConfirmationToPlayer,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingConfirmationToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task IndividualCoachingRequestToCoach(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.IndividualCoachingRequestToCoach,
                commonTemplate.PlayerName,
                commonTemplate.Activity,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.IndividualCoachingRequestToCoach,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task GroupCoachingBookingConfirmationToCoach(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.GroupCoachingBookingConfirmationToCoach,
                commonTemplate.PlayerName,
                commonTemplate.Activity,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.GroupCoachingBookingConfirmationToCoach,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task IndividualCoachingCancellationLessthan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.IndividualCoachingCancellationLessthan24HoursToCoach,
                commonTemplate.PlayerName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.IndividualCoachingCancellationLessthan24HoursToCoach,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task IndividualCoachingCancellationMorethan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.IndividualCoachingCancellationMorethan24HoursToCoach,
                commonTemplate.PlayerName,
                commonTemplate.Activity,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.IndividualCoachingCancellationMorethan24HoursToCoach,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task GroupCoachingCancellationLessthan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.GroupCoachingCancellationLessthan24HoursToCoach,
                commonTemplate.PlayerName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.GroupCoachingCancellationLessthan24HoursToCoach,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task GroupCoachingCancellationMorethan24HoursToCoach(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.GroupCoachingCancellationMorethan24HoursToCoach,
                commonTemplate.PlayerName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.GroupCoachingCancellationMorethan24HoursToCoach,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task ShareEventToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.ShareEventToPlayer,
                commonTemplate.CaptainName,
                commonTemplate.Sport,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.ShareEventToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task CaptainAcceptsTheRequestToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.CaptainAcceptsTheRequestToPlayer,
                commonTemplate.CaptainName,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.CaptainAcceptsTheRequestToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId

            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task OneSpotIsFreeFromWaitingListToPlayer(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.OneSpotIsFreeFromWaitingListToPlayer,
                commonTemplate.Sport,
                commonTemplate.FacilityName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                UserId = commonTemplate.UserId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.OneSpotIsFreeFromWaitingListToPlayer,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingConfirmationToFacility(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingConfirmationToFacility,
                commonTemplate.UserName,
                commonTemplate.PitchName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingConfirmationToFacility,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId,
                IsFacility = true,
                UserId = commonTemplate.UserId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromCaptainMorethan24HoursToFacility(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromCaptainMorethan24HoursToFacility,
                commonTemplate.CaptainName,
                commonTemplate.PitchName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromCaptainMorethan24HoursToFacility,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId,
                IsFacility = true,
                UserId = commonTemplate.UserId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }

        public async Task PitchBookingCancellationFromPlayerMorethan24HoursToFacility(BookingNotificationCommonTemplate commonTemplate)
        {
            loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
            string NotificationTitle = string.Format(aPIConfigurationManager.INAPPNotificationTemplateConfig.PitchBookingCancellationFromPlayerMorethan24HoursToFacility,
                commonTemplate.UserName,
                commonTemplate.PitchName,
                commonTemplate.BookingDate.ToString("dd-MM-yyyy"),
                commonTemplate.BookingTime
                );
            UserNotification userNotification = new UserNotification()
            {
                BookingConfirmed = commonTemplate.BookingConfirmed,
                BookingId = commonTemplate.BookingId,
                NotificationTitle = NotificationTitle,
                NotificationTemplateType = (int)INAPPNotificationTemplateType.PitchBookingCancellationFromPlayerMorethan24HoursToFacility,
                NotificationType = commonTemplate.NotificationType,
                FacilityId = commonTemplate.FacilityId,
                IsFacility = true,
                UserId = commonTemplate.UserId
            };
            await usernotificationRepository.InsertNotification(userNotification);
        }
        #endregion
    }
}
