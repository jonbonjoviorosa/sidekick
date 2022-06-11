using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.FireBase;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.Service.IService;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Booking;
using Sidekick.Model.Gym;
using Sidekick.Model.Notification;
using Sidekick.Model.Payment;
using Sidekick.Model.UserNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class BookingHandler : IBookingHandler
    {
        private readonly ILoggerManager loggerManager;
        private readonly IBookingRepository bookingRepository;
        private readonly IClassRepository classRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IUserHelper userHelper;
        private readonly IUserNotificationHandler notificationHandler;
        private readonly IUnitOfWork unitOfWork;
        private readonly ITelRService telRService;
        private readonly ICoachRepository coachRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly ICommissionRepository commissionRepository;
        private readonly INotificationRepository notificationRepository;
        private readonly IUserNotificationRepository userNotificationRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IUserNotificationHandler userNotificationHandler;
        private readonly IPushNotificationTemplateRepository pushNotificationTemplateRepository;
        private readonly IGymRepository gymRepository;
        private readonly IUserDevicesRepository userDevicesRepository;
        private APIConfigurationManager APIConfig { get; set; }

        public BookingHandler(ILoggerManager loggerManager,
            IBookingRepository bookingRepository,
            IClassRepository classRepository,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            IUserRepository userRepository,
            IUserHelper userHelper,
            IUnitOfWork unitOfWork,
            ITelRService telRService,
            INotificationRepository notificationRepository,
            IUserNotificationRepository userNotificationRepository,
            ICommissionRepository commissionRepository,
            IUserNotificationHandler notificationHandler,
            ICoachRepository coachRepository,
            ILocationRepository locationRepository,
            APIConfigurationManager _apiCon,
            IUserNotificationHandler userNotificationHandler,
            IGymRepository gymRepository,
            IPushNotificationTemplateRepository pushNotificationTemplateRepository,
            IUserDevicesRepository userDevicesRepository
            )
        {
            this.loggerManager = loggerManager;
            this.bookingRepository = bookingRepository;
            this.classRepository = classRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.userHelper = userHelper;
            this.unitOfWork = unitOfWork;
            this.telRService = telRService;
            this.coachRepository = coachRepository;
            this.paymentRepository = paymentRepository;
            this.commissionRepository = commissionRepository;
            this.notificationRepository = notificationRepository;
            this.userNotificationRepository = userNotificationRepository;
            this.notificationHandler = notificationHandler;
            this.locationRepository = locationRepository;
            this.APIConfig = _apiCon;
            this.userNotificationHandler = userNotificationHandler;
            this.gymRepository = gymRepository;
            this.pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            this.userDevicesRepository = userDevicesRepository;
        }

        public async Task<APIResponse<MyBookingViewModel>> GetIndivdualBookingsPerUser(bool getLatest)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(getLatest);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetIndividualBookingsPerParticipant(currentLogin, getLatest);
                var myBookingViewModel = new MyBookingViewModel();
                myBookingViewModel.UpComingBooking = response.Where(x => x.Date.Date >= Helper.GetDateTime().Date).ToList();
                myBookingViewModel.BookingHistory = response.Where(x => x.Date.Date < Helper.GetDateTime().Date).ToList();
                return new APIResponse<MyBookingViewModel>
                {
                    Status = Status.Success,
                    Payload = myBookingViewModel,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<MyBookingViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<MyBookingViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<IEnumerable<IndividualBookingViewModel>>> GetAllBookingsBookingsPerUser()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetAllBookingsPerParticipant(currentLogin);
                return new APIResponse<IEnumerable<IndividualBookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<IndividualBookingViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<IndividualBookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<IndividualBookingViewModel>>> GetAllBookings()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);

                var response = await bookingRepository.GetAllBookings();
                return new APIResponse<IEnumerable<IndividualBookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<IndividualBookingViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<IndividualBookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<IEnumerable<IndividualBookingViewModel>>> GetIndivdualBookingsPerCoach(bool getLatest)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(getLatest);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetIndividualBookingsPerCoach(currentLogin, getLatest);

                return new APIResponse<IEnumerable<IndividualBookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<IndividualBookingViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<IndividualBookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IndividualBooking>> InsertUpdateIndividualBooking(IndividualBooking_SaveViewModel booking)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(booking);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getIndividualClass = await classRepository.GetIndividualClass(booking.IndivdualClassId);
                if (getIndividualClass == null)
                {
                    return APIResponseHelper<IndividualBooking>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                var getIndividualClassDetail = await classRepository.GetIndividualClassDetails(booking.IndivdualClassId);
                if (getIndividualClassDetail.Any())
                {
                    booking.Coaching = getIndividualClassDetail.FirstOrDefault().Title != null ? getIndividualClassDetail.FirstOrDefault().Title : "";
                }
                else
                    booking.Coaching = "";
                var date = Helper.GetDateTime();
                var countBookingsToday = await bookingRepository.GetCount_IndividualBookingByDate(date);

                var totalHour = booking.Duration;
                var startTime = TimeSpan.Parse(booking.StartTime);
                booking.Date = booking.Date.Add(startTime);

                booking.EndTime = TimeSpan.Parse(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, startTime.Hours, startTime.Minutes, 0).AddHours(booking.Duration).ToString("HH:mm")).ToString().Substring(0, 5);

                var mappedBooking = mapper.Map<IndividualBooking>(booking);
                mappedBooking.TraineeId = currentLogin;
                mappedBooking.ClassId = booking.IndivdualClassId;
                mappedBooking.TransactionNo = Helper.TransactionNoGenerator(countBookingsToday);
                mappedBooking.AmountPerHour = booking.PricePerHour;
                mappedBooking.ServiceFees = (booking.PricePerHour * Convert.ToDecimal(totalHour)) * 5 / 100;
                var commissionTrains = await this.commissionRepository.ComissionTrains();
                if (commissionTrains != null && commissionTrains.Payload != null && commissionTrains.Payload.CoachingIndividualComission != 0)
                    mappedBooking.SideKickCommission = Convert.ToDecimal(commissionTrains.Payload.CoachingIndividualComission);
                else
                    mappedBooking.SideKickCommission = 0;
                mappedBooking.TotalAmount = (Convert.ToDecimal(totalHour) * booking.PricePerHour) + mappedBooking.ServiceFees + mappedBooking.SideKickCommission;

                var bookingId = await bookingRepository.InsertUpdateIndividualBooking(currentLogin, mappedBooking);

                return new APIResponse<IndividualBooking>
                {
                    Status = Status.Success,
                    Payload = mappedBooking,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IndividualBooking>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IndividualBooking>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IndividualConfirmBookingViewModel>> ConfirmIndividualBooking(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getIndividualBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getIndividualBooking == null)
                {
                    return APIResponseHelper<IndividualConfirmBookingViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                var classDetails = await classRepository.GetIndividualClass(getIndividualBooking.ClassId);
                var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                var userDetails = await userRepository.GetUser(classDetails.CoachId);
                var userDetailsBooked = await userRepository.GetUser(currentLogin);

                var mappedResponse = mapper.Map<IndividualConfirmBookingViewModel>(getIndividualBooking);
                mappedResponse.CoachFirstName = userDetails.FirstName;
                mappedResponse.CoachLasttName = userDetails.LastName;
                mappedResponse.CoachLocation = coachDetails.Location;

                mappedResponse.PriceIncludingVat = getIndividualBooking.AmountPerHour + (getIndividualBooking.AmountPerHour) * 5 / 100;
                mappedResponse.UserFirstName = userDetailsBooked.FirstName;
                mappedResponse.UserLastName = userDetailsBooked.LastName;
                mappedResponse.coachImage = userDetails.ImageUrl;
                mappedResponse.TransactionNo = getIndividualBooking.TransactionNo;
                return new APIResponse<IndividualConfirmBookingViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedResponse,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IndividualConfirmBookingViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IndividualConfirmBookingViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IndividualBooking>> GetIndivdualBookingsByTelRRefNo(string TelRRefNo)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(TelRRefNo);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetIndividualBookingByTelRRefNo(TelRRefNo);
                return new APIResponse<IndividualBooking>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IndividualBooking>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IndividualBooking>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse> UpdateIndividualBookingPaymentValidation(Guid bookingId, bool isPaymentValidated)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(bookingId);
                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var date = Helper.GetDateTime();
                getBooking.DatePaymentValidated = date;
                getBooking.IsPaymentValidated = isPaymentValidated;

                await bookingRepository.UpdateIndividualBooking(Guid.NewGuid(), getBooking);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> UpdateIndividualBookingPaymentComplete(Guid bookingId, bool isPaymentValidated)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(bookingId);
                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                getBooking.Status = EBookingStatus.Complete;
                await bookingRepository.UpdateIndividualBooking(Guid.NewGuid(), getBooking);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse> UpdateIndividualBookingPaymentPaid(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(bookingId);
                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var date = Helper.GetDateTime();
                getBooking.DatePaid = date;
                getBooking.IsPaid = true;
                getBooking.Status = EBookingStatus.Pending;
                await bookingRepository.UpdateIndividualBooking(Guid.NewGuid(), getBooking);

                //update payment
                var getPayment = await paymentRepository.GetPaymentByBookingID(bookingId);
                getPayment.Status = PaymentStatus.Paid;
                getPayment.DatePaid = Helper.GetDateTime();
                await paymentRepository.InsertUpdatePayment(getPayment);

                //Add Notification to send coach for join new player
                var userDetailsBooked = await userRepository.GetUser(getBooking.TraineeId);
                var classDetails = await classRepository.GetIndividualClass(getBooking.ClassId);
                var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);
                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    UserName = userDetailsBooked.FirstName,
                    CoachName = coachuserDetails.FirstName,
                    Activity = "Individual",
                    BookingDate = getBooking.Date,
                    BookingTime = getBooking.StartTime + "-" + getBooking.EndTime,
                    TotalAmount = getBooking.TotalAmount,
                    PriceCoaching = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = coachuserDetails.UserId,
                    BookingId = getBooking.BookingId,
                    PlayerName = userDetailsBooked.FirstName + " " + userDetailsBooked.LastName,
                    NotificationType = (int)ENotificationType.Individualbooking
                };
                await userNotificationHandler.IndividualCoachingRequestToCoach(commonTemplate);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> ChangeStatusIndivdualBooking(Guid bookingId, EBookingStatus status)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(new { bookingId, status });
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                getBooking.BookingId = bookingId;
                getBooking.Status = status;
                await bookingRepository.UpdateIndividualBooking(currentLogin, getBooking);

                var usernotification = await userNotificationRepository.GetNotificationByBookingId(getBooking.BookingId);
                if (usernotification != null)
                {
                    usernotification.BookingConfirmed = true;
                    await this.userNotificationRepository.InsertUpdateNotification(usernotification);
                }
                // send email to player...
                var classDetails = await classRepository.GetIndividualClass(getBooking.ClassId);
                var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);
                var playerDetails = await userRepository.GetUser(getBooking.TraineeId);
                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    EmailTo = new List<string>() { playerDetails.Email },
                    UserName = playerDetails.FirstName,
                    CoachName = coachuserDetails.FirstName,
                    Activity = "Individual",
                    BookingDate = getBooking.Date,
                    BookingTime = getBooking.StartTime + "-" + getBooking.EndTime,
                    Location = getBooking.Location,
                    TotalAmount = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = playerDetails.UserId,
                    BookingId = bookingId,
                    NotificationType = (int)ENotificationType.Individualbooking
                };

                EmailTemplatesHelper.IndividualCoachingBookingConfirmation(APIConfig, loggerManager, commonTemplate);
                userNotificationHandler.IndividualCoachingBookingConfirmationToPlayer(commonTemplate).GetAwaiter().GetResult();
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.UpdateSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<TelRResponseViewModel>> IndividualBookingPayment(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var paymentRequestModel = new Payment()
                {
                    BookingId = bookingId,
                    Amount = getBooking.TotalAmount,
                    BookingType = (int)EBookingType.Individual,
                    DatePaid = Helper.GetDateTime(),
                    SideKickCommission = getBooking.SideKickCommission,
                    Status = PaymentStatus.Pending,
                    TelRRefNo = getBooking.TelRRefNo,
                    TransactionNo = getBooking.TransactionNo
                };
                await paymentRepository.InsertUpdatePayment(paymentRequestModel);

                var user = await userRepository.GetUser(currentLogin);
                var userAddress = await userRepository.GetUserAddress(currentLogin);
                if (user.TelRRefNo != null)
                {
                    var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                    if (checkPayment.Payload.Order != null && checkPayment.Payload.Order.Transaction != null)
                    {
                        var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                        var paymentStatus = telRService.AuthPayment(getTelRRefNo, user.TransactionNo, 1);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            await this.UpdateIndividualBookingPaymentPaid(getBooking.BookingId);
                            var response = new TelRResponseViewModel();
                            var bookingDetail = await ConfirmIndividualBooking(bookingId);

                            getBooking.DepositeAuthCode = paymentStatus.Payload.TelRefNo;
                            await bookingRepository.UpdateIndividualBooking(currentLogin, getBooking);

                            response.bookingDetails = bookingDetail.Payload;

                            response.PaymentDone = true;
                            return new APIResponse<TelRResponseViewModel>
                            {
                                Message = Messages.SuccessInitPayment,
                                Status = Status.Success,
                                StatusCode = System.Net.HttpStatusCode.OK,
                                Payload = response
                            };


                        }
                    }
                }

                var payment = new TelRRequestViewModel()
                {
                    TransactionNo = getBooking.TransactionNo,
                    Amount = 1,//getBooking.TotalAmount,
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = userAddress?.CompleteAddress != null ? userAddress?.CompleteAddress : "",
                    City = userAddress?.City != null ? userAddress?.City : "",
                    CountryCode = userAddress?.CountryAlpha3Code != null ? userAddress?.CountryAlpha3Code : "",
                    Email = user.Email
                };
                var telRResponse = telRService.Payment(payment);

                if (telRResponse.Payload.Order != null)
                {
                    getBooking.TelRRefNo = telRResponse.Payload.Order.Ref;
                    await bookingRepository.UpdateIndividualBooking(currentLogin, getBooking);
                }

                return telRResponse;



            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<TelRPaymentReponseViewModel>> IndividualBookingPaymentProcess(Guid bookingId, APIConfigurationManager _conf = null)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);

                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                if (getBooking != null)
                {
                    var users = await userRepository.GetUser(getBooking.TraineeId);
                    var checkPayment = telRService.CheckPayment(users.TelRRefNo);

                    if (checkPayment.Payload.Order != null)
                    {
                        telRService.CancelPayment(getBooking.DepositeAuthCode, getBooking.TransactionNo, 1);
                        var paymentStatus = telRService.CapturePayment(getBooking.AuthCode, getBooking.TransactionNo, getBooking.TotalAmount);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.Status = EBookingStatus.Complete;
                            await bookingRepository.UpdateIndividualBooking(Guid.Empty, getBooking);
                            return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                        else
                        {
                            //To-Do //send payment failed notification
                            await SendIndividualPaymentFailNotification(bookingId, getBooking, users);
                        }
                    }
                    else
                    {
                        //To-Do //send payment failed notification
                        await SendIndividualPaymentFailNotification(bookingId, getBooking, users);
                        return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
                    }
                }
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NoTelRTranAttached);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        private async Task SendIndividualPaymentFailNotification(Guid bookingId, IndividualBooking getBooking, User users)
        {
            try
            {
                var classDetails = await classRepository.GetIndividualClass(getBooking.ClassId);
                var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);

                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    Type = "Individual",
                    EmailTo = new List<string>() { users.Email },
                    UserName = users.FirstName,
                    CoachName = coachuserDetails.FirstName,
                    Activity = "Individual",
                    BookingDate = getBooking.Date,
                    BookingTime = getBooking.StartTime + "-" + getBooking.EndTime,
                    Location = getBooking.Location,
                    TotalAmount = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = users.UserId,
                    BookingId = bookingId,
                    NotificationType = (int)ENotificationType.Individualbooking
                };
                var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(users.UserId);
                if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                {
                    // send notification
                    List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                    await pushNotificationTemplateRepository.PaymentFailTrain(APIConfig, loggerManager, DeviceFCMTokens, commonTemplate);
                }
                EmailTemplatesHelper.PaymentFailedForTrain(APIConfig, loggerManager, commonTemplate);
            }
            catch (Exception ex)
            {

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SENDEMAIL);
                loggerManager.LogException(ex);
            }
        }

        public async Task<APIResponse<TelRPaymentReponseViewModel>> IndividualBookingPaymentAuthProcess(Guid bookingId, APIConfigurationManager _conf = null)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                //var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                if (getBooking != null)

                {
                    var users = await userRepository.GetUser(getBooking.TraineeId);
                    var checkPayment = telRService.CheckPayment(users.TelRRefNo);
                    if (checkPayment.Payload.Order != null)
                    {
                        var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                        var paymentStatus = telRService.AuthPayment(getTelRRefNo, getBooking.TransactionNo, getBooking.TotalAmount);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.DatePaymentValidated = Helper.GetDateTime();
                            getBooking.IsPaymentValidated = true;
                            getBooking.AuthCode = paymentStatus.Payload.TelRefNo;
                            await bookingRepository.UpdateIndividualBooking(Guid.Empty, getBooking);
                            return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                    }
                    else
                    {
                        //ToDo, Needs to send message for failed payment
                        return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
                    }
                }
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NoTelRTranAttached);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<TelRPaymentReponseViewModel>> CancelSlotBooking(Guid facilityPitchTimingId)
        {
            //Cancel done by Facility Admin
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(facilityPitchTimingId);

                var getUserBookings = await bookingRepository.GetBookingReferencesByBookingId(facilityPitchTimingId);
                if (!getUserBookings.Any())
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                foreach (var booking in getUserBookings)
                {
                    BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                    {
                        Type = "PitchBooking",
                        EmailTo = new List<string>() { booking.Player.Email },
                        UserName = booking.Player.FirstName,
                        Activity = "PitchBooking",
                        BookingDate = booking.BookingDate,
                        BookingTime = booking.Start.ToShortTimeString() + "-" + booking.End.ToShortTimeString(),
                        Location = booking.Location,
                        Sport = booking.Sport,
                        FacilityName = booking.Facility,
                        PricePerPlayer = booking.PricePerPlayer,
                        TotalAmount = booking.TotalAmount,
                        //ServiceFees = getBooking.ServiceFees,
                        UserId = booking.Player.UserId,
                        BookingId = booking.BookingId,
                        NotificationType = (int)ENotificationType.PitchBooking,
                    };


                    //if (!booking.IsFree)
                    //{
                    //    if (booking.TelRRefNo != null)
                    //    {
                    //        var checkPayment = telRService.CheckPayment(booking.TelRRefNo);
                    //        if (checkPayment.Payload.Order != null)
                    //        {
                    //            var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                    //            var paymentStatus = telRService.CapturePayment(getTelRRefNo, booking.TransactionNo, booking.TotalAmount + 100);
                    //            if (paymentStatus.Payload.IsSuccess)
                    //            {
                    //                // send email and notification here.

                    //                //var classDetails = await classRepository.GetIndividualClass(getBooking.ClassId);
                    //                //var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                    //                //var coachUserDetails = await userRepository.GetUser(classDetails.CoachId);
                    //                //var userDetailsBooked = await userRepository.GetUser(currentLogin);

                    //                //var EmailParam = _conf.MailConfig;
                    //                //string serviceFees = "0";

                    //                //EmailParam.To = new List<string>() { userDetailsBooked.Email };
                    //                //EmailParam.Subject = APIConfig.BookingNotificationConfig.IndividualCoachingBookingConfirmationEmailSubject;
                    //                //EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.IndividualCoachingBookingConfirmation,
                    //                //    userDetailsBooked.FirstName, "Individual Class",
                    //                //    coachUserDetails.FirstName,getBooking.Date.ToString("dd-MM-yyyy"),getBooking.StartTime
                    //                //    ,getBooking.Location,
                    //                //    getBooking.TotalAmount, serviceFees,
                    //                //    getBooking.TotalAmount);

                    //                //var sendStatus =EmailHelper.SendEmailByEmailAddress(new List<string>() { userDetailsBooked.Email }, EmailParam, loggerManager);
                    //                SendEmailNotification(booking, commonTemplate);

                    //                await bookingRepository.DeleteBookingId(booking);
                    //            }
                    //        }
                    //        //else
                    //        //{
                    //        //    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
                    //        //}
                    //    }
                    //}

                    SendEmailNotification(booking, commonTemplate);

                    await bookingRepository.DeleteBookingId(booking);
                }

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<TelRPaymentReponseViewModel>> CancelBooking(Guid bookingID)
        {
            //Cancel done by Facility Admin
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingID);

                var getUserBookings = await bookingRepository.GetBookingReferencesByBookingId(bookingID);
                if (!getUserBookings.Any())
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                foreach (var booking in getUserBookings)
                {
                    BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                    {
                        Type = "PitchBooking",
                        EmailTo = new List<string>() { booking.Player.Email },
                        UserName = booking.Player.FirstName,
                        Activity = "PitchBooking",
                        BookingDate = booking.BookingDate,
                        BookingTime = booking.Start.ToShortTimeString() + "-" + booking.End.ToShortTimeString(),
                        Location = booking.Location,
                        Sport = booking.Sport,
                        FacilityName = booking.Facility,
                        PricePerPlayer = booking.PricePerPlayer,
                        TotalAmount = booking.TotalAmount,
                        //ServiceFees = getBooking.ServiceFees,
                        UserId = booking.Player.UserId,
                        BookingId = booking.BookingId,
                        NotificationType = (int)ENotificationType.PitchBooking,
                    };


                    //if (!booking.IsFree)
                    //{
                    //    if (booking.TelRRefNo != null)
                    //    {
                    //        var checkPayment = telRService.CheckPayment(booking.TelRRefNo);
                    //        if (checkPayment.Payload.Order != null)
                    //        {
                    //            var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                    //            var paymentStatus = telRService.CapturePayment(getTelRRefNo, booking.TransactionNo, booking.TotalAmount + 100);
                    //            if (paymentStatus.Payload.IsSuccess)
                    //            {
                    //                // send email and notification here.

                    //                //var classDetails = await classRepository.GetIndividualClass(getBooking.ClassId);
                    //                //var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                    //                //var coachUserDetails = await userRepository.GetUser(classDetails.CoachId);
                    //                //var userDetailsBooked = await userRepository.GetUser(currentLogin);

                    //                //var EmailParam = _conf.MailConfig;
                    //                //string serviceFees = "0";

                    //                //EmailParam.To = new List<string>() { userDetailsBooked.Email };
                    //                //EmailParam.Subject = APIConfig.BookingNotificationConfig.IndividualCoachingBookingConfirmationEmailSubject;
                    //                //EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.IndividualCoachingBookingConfirmation,
                    //                //    userDetailsBooked.FirstName, "Individual Class",
                    //                //    coachUserDetails.FirstName,getBooking.Date.ToString("dd-MM-yyyy"),getBooking.StartTime
                    //                //    ,getBooking.Location,
                    //                //    getBooking.TotalAmount, serviceFees,
                    //                //    getBooking.TotalAmount);

                    //                //var sendStatus =EmailHelper.SendEmailByEmailAddress(new List<string>() { userDetailsBooked.Email }, EmailParam, loggerManager);
                    //                SendEmailNotification(booking, commonTemplate);

                    //                await bookingRepository.DeleteBookingId(booking);
                    //            }
                    //        }
                    //        //else
                    //        //{
                    //        //    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
                    //        //}
                    //    }
                    //}

                    SendEmailNotification(booking, commonTemplate);

                    await bookingRepository.DeleteBookingId(booking);
                }

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        private void SendEmailNotification(CancelPlayBookingViewModel booking, BookingNotificationCommonTemplate commonTemplate)
        {
            try
            {
                var startTime = booking.BookingDate;
                if (startTime.Subtract(DateTime.Now).TotalHours > 24)
                {
                    // send morethan 24 hours
                    EmailTemplatesHelper.PitchBookingCancellationFromFacilityMorethan24HoursToPlayer(APIConfig, APIConfig, loggerManager, commonTemplate);
                    userNotificationHandler.PitchBookingCancellationFromFacilityMorethan24HoursToAllPlayers(commonTemplate).GetAwaiter().GetResult();
                }
                else
                {
                    // send lessthan 24 hours
                    EmailTemplatesHelper.PitchBookingCancellationFromFacilityLessthan24HoursToPlayer(APIConfig, APIConfig, loggerManager, commonTemplate);
                    userNotificationHandler.PitchBookingCancellationFromFacilityLessthan24HoursToAllPlayers(commonTemplate).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SENDEMAIL);
                loggerManager.LogException(ex);
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SENDEMAIL);
            }

        }

        public async Task<APIResponse<TelRPaymentReponseViewModel>> IndividualBookingPaymentCancel(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetIndividualBooking(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var usernotification = await userNotificationRepository.GetNotificationByBookingId(getBooking.BookingId);
                if (usernotification != null)
                {
                    usernotification.BookingConfirmed = true;
                    await this.userNotificationRepository.InsertUpdateNotification(usernotification);
                }

                var classDetails = await classRepository.GetIndividualClass(getBooking.ClassId);
                var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);
                var playerDetails = await userRepository.GetUser(getBooking.TraineeId);

                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    Type = "Individual",
                    EmailTo = new List<string>() { playerDetails.Email },
                    UserName = playerDetails.FirstName,
                    CoachName = coachuserDetails.FirstName,
                    Activity = "Individual",
                    BookingDate = getBooking.Date,
                    BookingTime = getBooking.StartTime + "-" + getBooking.EndTime,
                    Location = getBooking.Location,
                    TotalAmount = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = playerDetails.UserId,
                    BookingId = bookingId,
                    PriceCoaching = getBooking.TotalAmount,
                    NotificationType = (int)ENotificationType.Individualbooking,
                    BookingConfirmed = true
                };
                var startTime = getBooking.Date.AddHours(Convert.ToDateTime(getBooking.StartTime).Hour);
                // check cancel done by coach is morethan 24 hours prior
                // check cancel done by coach or user
                bool Cancel24HoursWithin = false;
                EBookingStatus bookingStatus;
                if (currentLogin == classDetails.CoachId)
                {
                    bookingStatus = EBookingStatus.Declined;
                    // cancel done by coach
                    if (startTime.Subtract(DateTime.Now).TotalHours > 24)
                    {
                        // send morethan 24 hours
                        EmailTemplatesHelper.CancellationFromCoachMorethan24Hours(APIConfig, loggerManager, commonTemplate);
                        userNotificationHandler.CancellationCoachingFromCoachMorethan24HoursToPlayer(commonTemplate).GetAwaiter().GetResult();
                    }
                    else
                    {
                        // send lessthan 24 hours
                        Cancel24HoursWithin = true;
                        EmailTemplatesHelper.CancellationFromCoachLessthan24Hours(APIConfig, loggerManager, commonTemplate);
                        userNotificationHandler.CancellationCoachingFromCoachLessthan24HoursToPlayer(commonTemplate).GetAwaiter().GetResult();
                    }
                }
                else
                {
                    // cancel done by Player
                    // also send notificatin to coach  Individual Coaching Cancellation < 24 Hours , Individual Coaching Cancellation > 24 Hours
                    bookingStatus = EBookingStatus.Cancelled;
                    if (startTime.Subtract(DateTime.Now).TotalHours > 24)
                    {
                        // send morethan 24 hours
                        EmailTemplatesHelper.CancellationFromUserMorethan24HoursEmailSubject(APIConfig, loggerManager, commonTemplate);
                        await userNotificationHandler.CancellationCoachingFromUserMorethan24HoursToPlayer(commonTemplate);

                        // send notification to coach as user cancel request

                        commonTemplate.UserId = coachuserDetails.UserId;
                        commonTemplate.EmailTo = new List<string>() { coachuserDetails.Email };
                        commonTemplate.PlayerName = playerDetails.FirstName + "" + playerDetails.LastName;
                        await userNotificationHandler.IndividualCoachingCancellationMorethan24HoursToCoach(commonTemplate);
                    }
                    else
                    {
                        // send lessthan 24 hours
                        EmailTemplatesHelper.CancellationFromUserLessthan24Hours(APIConfig, loggerManager, commonTemplate);
                        await userNotificationHandler.CancellationCoachingFromUserLessthan24HoursToPlayer(commonTemplate);
                        Cancel24HoursWithin = true;
                        // send notification to coach as user cancel request
                        commonTemplate.UserId = coachuserDetails.UserId;
                        commonTemplate.EmailTo = new List<string>() { coachuserDetails.Email };
                        commonTemplate.PlayerName = playerDetails.FirstName + "" + playerDetails.LastName;
                        await userNotificationHandler.IndividualCoachingCancellationLessthan24HoursToCoach(commonTemplate);
                    }
                }

                var users = await userRepository.GetUser(getBooking.TraineeId);
                var checkPayment = telRService.CheckPayment(users.TelRRefNo);
                if (checkPayment.Payload.Order != null)
                {
                    telRService.CancelPayment(getBooking.DepositeAuthCode, getBooking.TransactionNo, 1);
                    if (Cancel24HoursWithin)
                    {
                        var paymentStatus = telRService.CapturePayment(getBooking.AuthCode, getBooking.TransactionNo, getBooking.TotalAmount);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.Status = bookingStatus;
                            await bookingRepository.UpdateIndividualBooking(Guid.Empty, getBooking);
                            return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                        else
                        {
                            //To-Do //send payment failed notification
                        }
                    }
                    else
                    {
                        var paymentStatus = telRService.CancelPayment(getBooking.AuthCode, getBooking.TransactionNo, getBooking.TotalAmount);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.Status = bookingStatus;
                            await bookingRepository.UpdateIndividualBooking(Guid.Empty, getBooking);
                            return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                        else
                        {
                            //To-Do //send payment failed notification
                        }
                    }
                }
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NoTelRTranAttached);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<TelRPaymentReponseViewModel>> ClassBookingPaymentCancel(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetGroupBookingDb(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var usernotification = await userNotificationRepository.GetNotificationByBookingId(getBooking.GroupBookingId);
                if (usernotification != null)
                {
                    usernotification.BookingConfirmed = true;
                    await this.userNotificationRepository.InsertUpdateNotification(usernotification);
                }

                var classDetails = await classRepository.GetGroupClass(getBooking.GroupClassId);

                var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);
                var playerDetails = await userRepository.GetUser(getBooking.ParticipantId);

                string locationArea = "";
                if (classDetails.GymId.HasValue)
                {
                    Gym gymDetails = await gymRepository.GetGym(classDetails.GymId.Value);
                    if (gymDetails != null && !string.IsNullOrWhiteSpace(gymDetails.GymName))
                        locationArea = gymDetails.GymName;
                }

                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    Type = "Group",
                    EmailTo = new List<string>() { playerDetails.Email },
                    UserName = playerDetails.FirstName,
                    CoachName = coachuserDetails.FirstName,
                    Activity = classDetails.Title,
                    BookingDate = getBooking.Date.Value,
                    BookingTime = classDetails.StartTime,
                    Location = locationArea,
                    TotalAmount = getBooking.TotalAmount,
                    PriceCoaching = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = playerDetails.UserId,
                    BookingId = getBooking.GroupBookingId,
                    NotificationType = (int)ENotificationType.Groupbooking
                };

                bool Cancel24HoursWithin = false;
                EBookingStatus bookingStatus;
                if (currentLogin == classDetails.CoachId)
                {
                    // cancel done by coach
                    bookingStatus = EBookingStatus.Declined;
                    if (classDetails.Start.Value.Subtract(DateTime.Now).TotalHours > 24)
                    {
                        // send morethan 24 hours
                        EmailTemplatesHelper.CancellationFromCoachMorethan24Hours(APIConfig, loggerManager, commonTemplate);
                        await userNotificationHandler.CancellationCoachingFromCoachMorethan24HoursToPlayer(commonTemplate);
                    }
                    else
                    {
                        // send lessthan 24 hours
                        EmailTemplatesHelper.CancellationFromCoachLessthan24Hours(APIConfig, loggerManager, commonTemplate);
                        await userNotificationHandler.CancellationCoachingFromCoachLessthan24HoursToPlayer(commonTemplate);
                    }
                }
                else
                {
                    // cancel done by Player   Group Coaching Cancellation < 24 Hours, Group Coaching Cancellation > 24 Hours
                    bookingStatus = EBookingStatus.Cancelled;
                    if (classDetails.Start.Value.Subtract(DateTime.Now).TotalHours > 24)
                    {
                        // send morethan 24 hours
                        EmailTemplatesHelper.CancellationFromUserMorethan24HoursEmailSubject(APIConfig, loggerManager, commonTemplate);
                        await userNotificationHandler.CancellationCoachingFromUserMorethan24HoursToPlayer(commonTemplate);

                        // send notification to coach as user cancel request
                        commonTemplate.UserId = coachuserDetails.UserId;
                        commonTemplate.EmailTo = new List<string>() { coachuserDetails.Email };
                        commonTemplate.PlayerName = playerDetails.FirstName + "" + playerDetails.LastName;
                        await userNotificationHandler.GroupCoachingCancellationMorethan24HoursToCoach(commonTemplate);
                    }
                    else
                    {
                        // send lessthan 24 hours
                        EmailTemplatesHelper.CancellationFromUserLessthan24Hours(APIConfig, loggerManager, commonTemplate);
                        await userNotificationHandler.CancellationCoachingFromUserLessthan24HoursToPlayer(commonTemplate);

                        // send notification to coach as user cancel request
                        commonTemplate.UserId = coachuserDetails.UserId;
                        commonTemplate.EmailTo = new List<string>() { coachuserDetails.Email };
                        commonTemplate.PlayerName = playerDetails.FirstName + "" + playerDetails.LastName;
                        await userNotificationHandler.GroupCoachingCancellationLessthan24HoursToCoach(commonTemplate);
                    }
                }
                var users = await userRepository.GetUser(getBooking.ParticipantId);
                var checkPayment = telRService.CheckPayment(users.TelRRefNo);
                if (checkPayment.Payload.Order != null)
                {
                    telRService.CancelPayment(getBooking.DepositeAuthCode, getBooking.TransactionNo, 1);
                    if (Cancel24HoursWithin)
                    {
                        var paymentStatus = telRService.CapturePayment(getBooking.AuthCode, getBooking.TransactionNo, getBooking.TotalAmount);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.Status = bookingStatus;
                            await bookingRepository.UpdateGroupBooking(Guid.Empty, getBooking);
                            return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                        else
                        {
                            //To-Do //send payment failed notification
                        }
                    }
                    else
                    {
                        var paymentStatus = telRService.CancelPayment(getBooking.AuthCode, getBooking.TransactionNo, getBooking.TotalAmount);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.Status = bookingStatus;
                            await bookingRepository.UpdateGroupBooking(Guid.Empty, getBooking);
                            return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                        else
                        {
                            //To-Do //send payment failed notification
                        }
                    }
                }


                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NoTelRTranAttached);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<GroupBookingViewModel>>> GetGroupBookingsPerUser(bool getLatest)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(getLatest);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetGroupBookingsByUser(currentLogin, getLatest);

                return new APIResponse<IEnumerable<GroupBookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<GroupBookingViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<GroupBookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
        public async Task<APIResponse<IEnumerable<GroupBookingViewModel>>> GetGroupBookingsPerCoach(bool getLatest)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(getLatest);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetGroupBookingsByCoach(currentLogin, getLatest);

                return new APIResponse<IEnumerable<GroupBookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<GroupBookingViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<GroupBookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
        public async Task<APIResponse<GroupBookingViewModel>> JoinGroupClass(Guid groupClassId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(groupClassId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getBooking = await classRepository.GetGroupClass(groupClassId);
                if (getBooking == null)
                {
                    return APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var isParticipantExist = (await bookingRepository.GetGroupBookingPerGroupClass(getBooking.GroupClassId))
                    .Where(x => x.ParticipantId == currentLogin
                             && x.Status == EBookingStatus.Approved);
                if (isParticipantExist.Any())
                {
                    return new APIResponse<GroupBookingViewModel>()
                    {
                        Message = Messages.AlreadyJoinedClass,
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                var date = Helper.GetDateTime();
                var countBookingsToday = await bookingRepository.GetCount_GroupBookingByDate(date);
                decimal groupCommission = 0;
                var commissionTrains = await this.commissionRepository.ComissionTrains();
                if (commissionTrains != null && commissionTrains.Payload != null && commissionTrains.Payload.CoachingGroupComission != 0)
                    groupCommission = Convert.ToDecimal(commissionTrains.Payload.CoachingGroupComission);
                decimal groupvatAmount = 0;
                groupvatAmount = getBooking.Price * 5 / 100;
                var booking = new GroupBooking()
                {
                    GroupClassId = groupClassId,
                    ParticipantId = currentLogin,
                    Date = getBooking.Start,
                    TotalAmount = getBooking.Price + groupCommission + groupvatAmount,
                    SideKickCommission = groupCommission,
                    ServiceFees = groupvatAmount,
                    TransactionNo = Helper.TransactionNoGenerator(countBookingsToday) + "-Group"
                };
                var groupBookingId = await bookingRepository.InsertGroupBooking(currentLogin, booking);

                var bookingDetails = await bookingRepository.GetGroupBooking(groupBookingId);

                var response = APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                if (bookingDetails != null && bookingDetails.Start != null)
                    bookingDetails.Date = Helper.GetDateTime().ToString("yyyy-MM-ddTHH:mm:ss");
                response.Payload = bookingDetails;


                return response;
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
        public async Task<APIResponse> ChangeStatusGroupBooking(GroupBooking_UpdateStatusViewModel booking)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(booking);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetGroupBookingDb(booking.GroupBookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }
                getBooking.Status = booking.Status;
                //var mappedGroupBooking = mapper.Map<GroupBooking>(booking);
                await bookingRepository.UpdateGroupBooking(currentLogin, getBooking);

                var usernotification = await userNotificationRepository.GetNotificationByBookingId(getBooking.GroupBookingId);
                if (usernotification != null)
                {
                    usernotification.BookingConfirmed = true;
                    await this.userNotificationRepository.InsertUpdateNotification(usernotification);
                }

                // send email to player...
                if (booking.Status == EBookingStatus.Confirmed)
                {
                    var classDetails = await classRepository.GetGroupClass(getBooking.GroupClassId);

                    var coachDetails = await coachRepository.GetCoach(classDetails.CoachId);
                    var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);
                    var playerDetails = await userRepository.GetUser(getBooking.ParticipantId);
                    string locationArea = "";
                    if (classDetails.GymId.HasValue)
                    {
                        Gym gymDetails = await gymRepository.GetGym(classDetails.GymId.Value);
                        if (gymDetails != null && !string.IsNullOrWhiteSpace(gymDetails.GymName))
                            locationArea = gymDetails.GymName;
                    }
                    BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                    {
                        EmailTo = new List<string>() { playerDetails.Email },
                        UserName = playerDetails.FirstName,
                        CoachName = coachuserDetails.FirstName,
                        Activity = classDetails.Title,
                        BookingDate = getBooking.Date.Value,
                        BookingTime = classDetails.StartTime,
                        Location = locationArea,
                        TotalAmount = getBooking.TotalAmount,
                        PriceCoaching = getBooking.TotalAmount,
                        ServiceFees = getBooking.ServiceFees,
                        UserId = playerDetails.UserId,
                        BookingId = getBooking.GroupBookingId,
                        NotificationType = (int)ENotificationType.Groupbooking
                    };

                    EmailTemplatesHelper.GroupCoachingBookingConfirmation(APIConfig, loggerManager, commonTemplate);
                    userNotificationHandler.GroupCoachingBookingConfirmationToPlayer(commonTemplate).GetAwaiter().GetResult();
                }

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.UpdateSuccess);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
        public async Task<APIResponse<TelRResponseViewModel>> GroupBookingPayment(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetGroupBookingDb(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var getGroupClass = await classRepository.GetGroupClass(getBooking.GroupClassId);
                var user = await userRepository.GetUser(currentLogin);
                var userAddress = await userRepository.GetUserAddress(currentLogin);

                var paymentRequestModel = new Payment()
                {
                    Amount = getBooking.TotalAmount,
                    BookingId = bookingId,
                    BookingType = (int)EBookingType.Group,
                    DatePaid = null,
                    SideKickCommission = getBooking.SideKickCommission,
                    Status = PaymentStatus.Pending,
                    TelRRefNo = getBooking.TelRRefNo,
                    TransactionNo = getBooking.TransactionNo
                };
                await paymentRepository.InsertUpdatePayment(paymentRequestModel);

                if (user.TelRRefNo != null)
                {
                    var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                    if (checkPayment.Payload.Order != null && checkPayment.Payload.Order.Transaction != null)
                    {
                        var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                        var paymentStatus = telRService.AuthPayment(getTelRRefNo, getBooking.TransactionNo, 1);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.DepositeAuthCode = paymentStatus.Payload.TelRefNo;
                            await bookingRepository.UpdateGroupBooking(currentLogin, getBooking);

                            await this.UpdateGroupBookingPaymentPaid(getBooking.GroupBookingId);

                            var response = new TelRResponseViewModel();
                            response.groupBookingDetails = await bookingRepository.GetGroupBooking(bookingId);
                            response.PaymentDone = true;
                            return new APIResponse<TelRResponseViewModel>
                            {
                                Message = Messages.SuccessInitPayment,
                                Status = Status.Success,
                                StatusCode = System.Net.HttpStatusCode.OK,
                                Payload = response
                            };

                        }
                    }
                }

                var payment = new TelRRequestViewModel()
                {
                    TransactionNo = getBooking.TransactionNo,
                    Amount = getBooking.TotalAmount,
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = userAddress?.CompleteAddress != null ? userAddress?.CompleteAddress : "",
                    City = userAddress?.City != null ? userAddress?.City : "",
                    CountryCode = userAddress?.CountryAlpha3Code != null ? userAddress?.CountryAlpha3Code : "",
                    Email = user.Email
                };

                var telRResponse = telRService.Payment(payment);
                if (telRResponse.Payload.Order != null)
                {
                    getBooking.TelRRefNo = telRResponse.Payload.Order.Ref;
                    await bookingRepository.UpdateGroupBooking(currentLogin, getBooking);
                }

                return telRResponse;
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
        public async Task<APIResponse<TelRPaymentReponseViewModel>> GroupBookingPaymentProcess(Guid bookingId, APIConfigurationManager _conf = null)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetGroupBookingDb(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                var getGroupClass = await classRepository.GetGroupClass(getBooking.GroupClassId);
                if (getBooking != null)
                {
                    var users = await userRepository.GetUser(getBooking.ParticipantId);
                    var checkPayment = telRService.CheckPayment(users.TelRRefNo);
                    if (checkPayment.Payload.Order != null)
                    {
                        var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                        var paymentStatus = telRService.CapturePayment(getBooking.AuthCode, getBooking.TransactionNo, getGroupClass.Price);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            telRService.CancelPayment(getTelRRefNo, getBooking.TransactionNo, 1);
                            getBooking.Status = EBookingStatus.Complete;
                            await bookingRepository.UpdateGroupBooking(Guid.Empty, getBooking);
                            var coachDetails = await coachRepository.GetCoach(getGroupClass.CoachId);
                            var coachUserDetails = await userRepository.GetUser(getGroupClass.CoachId);
                            var userDetailsBooked = await userRepository.GetUser(currentLogin);
                            string locationArea = "";
                            if (getGroupClass.LocationId.HasValue)
                            {
                                Location location = await locationRepository.GetLocation(getGroupClass.LocationId.Value);
                                if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                                    locationArea = location.Name;
                            }

                            var EmailParam = _conf.MailConfig;
                            string serviceFees = "0";
                            string bookingDate = getBooking.Date.HasValue ? getBooking.Date.Value.ToString("dd-MM-yyyy") : "";


                            EmailParam.To = new List<string>() { userDetailsBooked.Email };
                            EmailParam.Subject = APIConfig.BookingNotificationConfig.GroupCoachingBookingConfirmationEmailSubject;
                            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.GroupCoachingBookingConfirmation,
                                userDetailsBooked.FirstName, getGroupClass.Title,
                                coachUserDetails.FirstName, bookingDate
                                , locationArea,
                                getBooking.TotalAmount, serviceFees,
                                getBooking.TotalAmount);
                            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, loggerManager);
                            var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { userDetailsBooked.Email }, EmailParam, loggerManager);
                            return paymentStatus;
                        }
                        else
                        {
                            //Todo payment failed
                            await SendGroupClassPaymentFailNotification(currentLogin, getBooking, getGroupClass, users);
                        }
                    }
                    else
                    {
                        await SendGroupClassPaymentFailNotification(currentLogin, getBooking, getGroupClass, users);
                        return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
                    }
                }
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NoTelRTranAttached);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        private async Task SendGroupClassPaymentFailNotification(Guid currentLogin, GroupBooking getBooking, Model.Class.GroupClass getGroupClass, User users)
        {
            try
            {
                var coachDetails = await coachRepository.GetCoach(getGroupClass.CoachId);
                var coachUserDetails = await userRepository.GetUser(getGroupClass.CoachId);
                var userDetailsBooked = await userRepository.GetUser(currentLogin);
                string locationArea = "";
                if (getGroupClass.GymId.HasValue)
                {
                    Gym gymDetails = await gymRepository.GetGym(getGroupClass.GymId.Value);
                    if (gymDetails != null && !string.IsNullOrWhiteSpace(gymDetails.GymName))
                        locationArea = gymDetails.GymName;
                }
                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    Type = "Group",
                    EmailTo = new List<string>() { users.Email },
                    UserName = users.FirstName,
                    CoachName = coachUserDetails.FirstName,
                    Activity = getGroupClass.Title,
                    BookingDate = getBooking.Date.Value,
                    BookingTime = getGroupClass.StartTime,
                    Location = locationArea,
                    TotalAmount = getBooking.TotalAmount,
                    PriceCoaching = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = users.UserId,
                    BookingId = getBooking.GroupBookingId,
                    NotificationType = (int)ENotificationType.Groupbooking
                };
                var fcmTokenDetails = await userDevicesRepository.GetLatestDeviceFcmToken(users.UserId);
                if (fcmTokenDetails != null && !string.IsNullOrWhiteSpace(fcmTokenDetails.DeviceFCMToken))
                {
                    // send notification
                    List<string> DeviceFCMTokens = new List<string>() { fcmTokenDetails.DeviceFCMToken };
                    await pushNotificationTemplateRepository.PaymentFailTrain(APIConfig, loggerManager, DeviceFCMTokens, commonTemplate);
                }
                EmailTemplatesHelper.PaymentFailedForTrain(APIConfig, loggerManager, commonTemplate);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SENDEMAIL);
                loggerManager.LogException(ex);
            }
            
        }

        public async Task<APIResponse<TelRPaymentReponseViewModel>> GroupBookingAuthPaymentProcess(Guid bookingId, APIConfigurationManager _conf = null)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogDebugObject(bookingId);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var getBooking = await bookingRepository.GetGroupBookingDb(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }
                var getGroupClass = await classRepository.GetGroupClass(getBooking.GroupClassId);

                if (getBooking != null)
                {
                    var users = await userRepository.GetUser(getBooking.ParticipantId);
                    var checkPayment = telRService.CheckPayment(users.TelRRefNo);
                    if (checkPayment.Payload.Order != null)
                    {
                        var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                        var paymentStatus = telRService.AuthPayment(getTelRRefNo, getBooking.TransactionNo, getGroupClass.Price);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            getBooking.DatePaymentValidated = Helper.GetDateTime();
                            getBooking.IsPaymentValidated = true;
                            getBooking.AuthCode = paymentStatus.Payload.TelRefNo;
                            await bookingRepository.UpdateGroupBooking(Guid.Empty, getBooking);

                            var coachDetails = await coachRepository.GetCoach(getGroupClass.CoachId);
                            var coachUserDetails = await userRepository.GetUser(getGroupClass.CoachId);
                            var userDetailsBooked = await userRepository.GetUser(currentLogin);
                            string locationArea = "";
                            if (getGroupClass.LocationId.HasValue)
                            {
                                Location location = await locationRepository.GetLocation(getGroupClass.LocationId.Value);
                                if (location != null && !string.IsNullOrWhiteSpace(location.Name))
                                    locationArea = location.Name;
                            }

                            var EmailParam = _conf.MailConfig;
                            string serviceFees = "0";
                            string bookingDate = getBooking.Date.HasValue ? getBooking.Date.Value.ToString("dd-MM-yyyy") : "";


                            EmailParam.To = new List<string>() { userDetailsBooked.Email };
                            EmailParam.Subject = APIConfig.BookingNotificationConfig.GroupCoachingBookingConfirmationEmailSubject;
                            EmailParam.Body = String.Format(APIConfig.BookingNotificationConfig.GroupCoachingBookingConfirmation,
                                userDetailsBooked.FirstName, getGroupClass.Title,
                                coachUserDetails.FirstName, bookingDate
                                , locationArea,
                                getBooking.TotalAmount, serviceFees,
                                getBooking.TotalAmount);
                            EmailParam.Body = EmailTemplatesHelper.SetEmailHtmlTemplate(_conf, EmailParam.Body, loggerManager);
                            var sendStatus = EmailHelper.SendEmailByEmailAddress(new List<string>() { userDetailsBooked.Email }, EmailParam, loggerManager);
                            return paymentStatus;

                        }
                        else
                        {
                            //Todo Write code for payment failed
                        }


                    }
                    else
                    {
                        return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
                    }
                }
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NoTelRTranAttached);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<BookingViewModel>>> GetAllBookingBeforeSetTimeOfAppointment()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await bookingRepository.GetAllBookingBeforeSetTimeOfAppointment(Helper.GetDateTime());
                return new APIResponse<IEnumerable<BookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<BookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<BookingViewModel>>> GetAllBookingBefore48HoursSetTimeOfAppointment()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var response = await bookingRepository.GetAllBookingBefore48HoursSetTimeOfAppointment(Helper.GetDateTime());
                return new APIResponse<IEnumerable<BookingViewModel>>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<BookingViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse> UpdateGroupBookingPaymentValidation(Guid bookingId, bool isPaymentValidated)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(bookingId);
                var getBooking = await bookingRepository.GetGroupBookingDb(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var date = Helper.GetDateTime();
                getBooking.DatePaymentValidated = date;
                getBooking.IsPaymentValidated = isPaymentValidated;

                await bookingRepository.UpdateGroupBooking(Guid.NewGuid(), getBooking);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
        public async Task<APIResponse> UpdateGroupBookingPaymentPaid(Guid bookingId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.INSERT);
                loggerManager.LogDebugObject(bookingId);
                var getBooking = await bookingRepository.GetGroupBookingDb(bookingId);
                if (getBooking == null)
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var date = Helper.GetDateTime();
                getBooking.DatePaid = date;
                getBooking.IsPaid = true;
                getBooking.Status = EBookingStatus.Pending;
                await bookingRepository.UpdateGroupBooking(Guid.NewGuid(), getBooking);

                //update payment
                var getPayment = await paymentRepository.GetPaymentByBookingID(bookingId);
                if (getPayment != null)
                {
                    getPayment.Status = PaymentStatus.Paid;
                    getPayment.DatePaid = Helper.GetDateTime();
                    await paymentRepository.InsertUpdatePayment(getPayment);
                }

                //Add Notification to coach for request by coaching user
                var userDetailsBooked = await userRepository.GetUser(getBooking.ParticipantId);
                var classDetails = await classRepository.GetGroupClass(getBooking.GroupClassId);
                var coachuserDetails = await userRepository.GetUser(classDetails.CoachId);
                BookingNotificationCommonTemplate commonTemplate = new BookingNotificationCommonTemplate()
                {
                    UserName = userDetailsBooked.FirstName,
                    CoachName = coachuserDetails.FirstName,
                    Activity = classDetails.Title,
                    BookingDate = getBooking.Date.Value,
                    BookingTime = classDetails.StartTime,
                    TotalAmount = getBooking.TotalAmount,
                    PriceCoaching = getBooking.TotalAmount,
                    ServiceFees = getBooking.ServiceFees,
                    UserId = coachuserDetails.UserId,
                    BookingId = getBooking.GroupBookingId,
                    PlayerName = userDetailsBooked.FirstName + " " + userDetailsBooked.LastName,
                    NotificationType = (int)ENotificationType.Groupbooking
                };
                await userNotificationHandler.GroupCoachingBookingConfirmationToCoach(commonTemplate);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<GroupBookingViewModel>> GetGroupBookingsByTelRRefNo(string TelRRefNo)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogDebugObject(TelRRefNo);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var response = await bookingRepository.GetGroupBookingByTelRRefNo(TelRRefNo);
                return new APIResponse<GroupBookingViewModel>
                {
                    Status = Status.Success,
                    Payload = response,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.INSERT_UPDATE);
                loggerManager.LogException(ex);

                return APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<GroupBookingViewModel>> ConfirmGroupBooking(Guid groupbookingId)
        {
            var bookingDetails = await bookingRepository.GetGroupBooking(groupbookingId);

            var response = APIResponseHelper<GroupBookingViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
            if (bookingDetails != null && bookingDetails.Start != null)
                bookingDetails.Date = Helper.GetDateTime().ToString("yyyy-MM-ddTHH:mm:ss");
            response.Payload = bookingDetails;

            return response;
        }


    }
}
