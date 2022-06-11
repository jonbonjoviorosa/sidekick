using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.Service.IService;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Payment;
using Sidekick.Model.Play;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class PlayHandler : IPlayHandler
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IPlayRepository playRepository;
        private readonly ILoggerManager loggerManager;
        private readonly IUserHelper userHelper;
        private readonly IMapper mapper;
        private readonly ITelRService telRService;
        private readonly IUserRepository userRepository;
        private readonly IPaymentRepository paymentRepository;
        private readonly IUserNotificationHandler userNotificationHandler;
        private readonly ISportRepository sportRepository;
        private readonly IUserPitchBookingRepository userPitchBookingRepository;
        private readonly IFacilityRepository facilityRepository;
        private readonly IFacilityPlayerRepository facilityPlayerRepository;


        public PlayHandler(IUnitOfWork unitOfWork,
            IPlayRepository playRepository,
            ILoggerManager loggerManager,
            IUserHelper userHelper,
            IMapper mapper,
            ITelRService telRService,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository, IUserNotificationHandler userNotificationHandler, ISportRepository sportRepository, IUserPitchBookingRepository userPitchBookingRepository, IFacilityRepository facilityRepository, IFacilityPlayerRepository facilityPlayerRepository)
        {
            this.unitOfWork = unitOfWork;
            this.playRepository = playRepository;
            this.paymentRepository = paymentRepository;
            this.loggerManager = loggerManager;
            this.userHelper = userHelper;
            this.mapper = mapper;
            this.telRService = telRService;
            this.userRepository = userRepository;
            this.userNotificationHandler = userNotificationHandler;
            this.sportRepository = sportRepository;
            this.userPitchBookingRepository = userPitchBookingRepository;
            this.facilityRepository = facilityRepository;
            this.facilityPlayerRepository = facilityPlayerRepository;
        }

        public async Task<APIResponse<IEnumerable<PlayFacilitiesModel>>> FilterPlayFacilities(IEnumerable<PlayFilterViewModel> filters, Guid _sportId, string facilityName)
        {
            try
            {

                var getPlayFacilities = await playRepository.FilterFacility(filters, _sportId, facilityName);

                return new APIResponse<IEnumerable<PlayFacilitiesModel>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getPlayFacilities
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<PlayFacilitiesModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<IEnumerable<PlayFacilitiesModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<PlayFacilitiesViewModel>> GetPlayFacilities(Guid _facilityId, DateTime bookDate, Guid _sportId)
        {
            try
            {
                var getPlayFacilities = await playRepository.GetFacility(_facilityId, bookDate, _sportId);

                return new APIResponse<PlayFacilitiesViewModel>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getPlayFacilities
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<PlayFacilitiesViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<PlayFacilitiesViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public APIResponse<IEnumerable<PlayFacilitiesModel>> SearchFacilityByName(string facilityName)
        {
            try
            {

                var getPlayFacilities = playRepository.SearchFacilityByName(facilityName);

                return new APIResponse<IEnumerable<PlayFacilitiesModel>>
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = getPlayFacilities
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<PlayFacilitiesModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<IEnumerable<PlayFacilitiesModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> OrganizeFreeGame(FreeGame freeGame)
        {
            try
            {

                return await playRepository.SaveFreeGame(freeGame);

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

        public async Task<APIResponse> PlayRequest(PlayRequest playRequest, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            try
            {
                return await playRepository.SubmitPlayRequest(playRequest, userHelper.GetCurrentUserGuidLogin(), _mhttp, _conf);
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

        public async Task<APIResponse> GetPitchBookingBySportId(IEnumerable<PlayFilterViewModel> filters, Guid sportId, string facilityName)
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var pitchBookings = await playRepository.GetPitchBookingBySportId(filters, sportId, currentLogin, facilityName);
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = pitchBookings
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
        public async Task<APIResponse> GetPitchBookings(Guid facilityPitchId)
        {
            try
            {
                var pitchBookings = await playRepository.GetPitchBookings(facilityPitchId);
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = pitchBookings
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
        public async Task<APIResponse> PitchBooking(UserPitchBookingModel pitchBooking, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            try
            {
                return await playRepository.PitchBooking(pitchBooking, userHelper.GetCurrentUserGuidLogin(), _mhttp, _conf);
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

        public async Task<APIResponse<Filters>> GetAllFilters()
        {
            try
            {
                var getAllFilters = await playRepository.GetFilters();

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

        public async Task<APIResponse> GetAllPitchBookings()
        {
            try
            {
                var pitchBookings = await playRepository.GetAllPitchBookings();
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = pitchBookings
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

        public async Task<APIResponse> GetFacilityPitchBookingsDate(string dateFrom, string dateTo)
        {
            try
            {
                var pitchBookings = await playRepository.GetFacilityPitchBookingsDate(dateFrom, dateTo);
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = pitchBookings
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


        public async Task<APIResponse> GetPitchBooking(Guid pitchBookingId)
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var pitchBookings = await playRepository.GetPitchBookingsDetails(pitchBookingId, currentLogin);
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = pitchBookings
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

        public async Task<APIResponse> GetPitchBookingByTelRRefNo(string TelRRefNo)
        {
            try
            {
                var pitchBookings = await playRepository.GetPitchBookingsDetailsByTelRRefNo(TelRRefNo);
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = pitchBookings
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

        public async Task<APIResponse<TelRResponseViewModel>> Payment(Guid bookingId)
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var pitchBookings = await playRepository.GetPitchBooking(bookingId);
                if (pitchBookings == null)
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var playerDetails = await playRepository.GetGamePlayer(bookingId, currentLogin);
                if (playerDetails == null)
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }



                var paymentRequestModel = new Payment()
                {
                    BookingId = bookingId,
                    Amount = playerDetails.TotalAmount,
                    BookingType = (int)EBookingType.Play,
                    DatePaid = Helper.GetDateTime(),
                    SideKickCommission = pitchBookings.Commission.GetValueOrDefault(),
                    Status = PaymentStatus.Pending,
                    TelRRefNo = string.Empty,
                    TransactionNo = string.Empty
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
                        var paymentStatus = telRService.AuthPayment(getTelRRefNo, checkPayment.Payload.Order.Transaction.code, 1);
                        if (paymentStatus.Payload.IsSuccess)
                        {
                            await this.UpdateGameplayerToPaid(playerDetails.FacilityPlayerId);
                            playerDetails.DepositeAuthCode = paymentStatus.Payload.TelRefNo;
                            await playRepository.UpdateGamePlayer(playerDetails);

                            var responsePlay = new TelRResponseViewModel();
                            responsePlay.playBookingDetails = await playRepository.GetPitchBookingsDetails(bookingId, currentLogin);
                            responsePlay.PaymentDone = true;

                            // check for caption and send notificationtoFacility...
                            await playRepository.PitchBookingConfirmationToFacility(bookingId, currentLogin);


                            return new APIResponse<TelRResponseViewModel>
                            {
                                Message = Messages.SuccessInitPayment,
                                Status = Status.Success,
                                StatusCode = System.Net.HttpStatusCode.OK,
                                Payload = responsePlay
                            };

                            //return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.RecordSuccess);
                        }
                    }
                }

                var request = new TelRRequestViewModel()
                {
                    TransactionNo = Guid.NewGuid().ToString(),
                    Amount = 1, //(pitchBookings.PricePerUser.Value + pitchBookings.PricePerUserVat.Value + pitchBookings.Commission.Value) * pitchBookings.PlayerCount,
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = userAddress?.CompleteAddress != null ? userAddress?.CompleteAddress : "",
                    City = userAddress?.City != null ? userAddress?.City : "",
                    CountryCode = userAddress?.CountryAlpha3Code != null ? userAddress?.CountryAlpha3Code : "",
                    Email = user.Email
                };
                var response = telRService.Payment(request);

                // playerDetails.DepositAmount = 1;
                playerDetails.TelRRefNo = response.Payload.Order.Ref;
                //playerDetails.InitialTransactionNo = request.TransactionNo;
                //playerDetails.TotalAmount = (pitchBookings.PricePerUser.Value * pitchBookings.PlayerCount) - playerDetails.DepositAmount;
                await playRepository.UpdateGamePlayer(playerDetails);
                return response;
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> CapturePayment(bool forDeposit, Guid pitchBookingId)
        {
            try
            {
                var telRResponse = new APIResponse<TelRPaymentReponseViewModel>();

                var bookings = await playRepository.GetPitchBooking(pitchBookingId);
                if (bookings != null)
                {
                    decimal totalBookingAmount = 100;
                    decimal amountPerPlayer = 0;
                    var players = await playRepository.GetGamePlayers(pitchBookingId);
                    if (players.Any())
                    {
                        amountPerPlayer = totalBookingAmount / players.Count();
                    }
                    foreach (var player in players)
                    {
                        var user = await userRepository.GetUser(player.UserId);
                        if (user.TelRRefNo != null && amountPerPlayer > 0)
                        {
                            var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                            if (checkPayment.Payload.Order != null)
                            {
                                var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                                var paymentStatus = telRService.CapturePayment(getTelRRefNo, user.TransactionNo, amountPerPlayer);
                                if (paymentStatus.Payload.IsSuccess)
                                {
                                    player.PlayerStatus = EGamePlayerStatus.Complete;
                                    await playRepository.UpdateGamePlayer(player);
                                }
                            }
                        }
                    }
                }
                /*
                foreach (var player in players)
                    {
                        var checkPayment = telRService.CheckPayment(player.TelRRefNo);
                        if (checkPayment.Payload.Order != null)
                        {
                            var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                            if (forDeposit)
                            {
                                telRResponse = telRService.CapturePayment(getTelRRefNo, player.InitialTransactionNo, player.TotalAmount);
                            }
                            else
                            {
                                telRResponse = telRService.CapturePayment(getTelRRefNo, player.InitialTransactionNo, player.TotalAmount);
                            }


                            if (telRResponse.Payload.IsSuccess)
                            {
                                if (forDeposit)
                                {
                                    player.TelRRefNo = telRResponse.Payload.TelRefNo;
                                    player.DatePaid = DateTime.Now;
                                    player.IsPaid = true;
                                }
                                else
                                {
                                    player.IsPaid = true;
                                    player.TelRRefNo = telRResponse.Payload.TelRefNo;
                                    player.DatePaid = DateTime.Now;
                                }
                                //await playRepository.UpdateGamePlayer(player);
                            }
                        }
                    }
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
                */
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.NotExist);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> PaymentAuthProcess(Guid pitchBookingId)
        {
            try
            {
                var telRResponse = new APIResponse<TelRPaymentReponseViewModel>();

                var bookings = await playRepository.GetPitchBooking(pitchBookingId);
                if (bookings != null)
                {

                    var players = await playRepository.GetGamePlayers(pitchBookingId);

                    foreach (var player in players.Where(p => p.IsPaymentValidated = false))
                    {
                        decimal totalBookingAmount = player.TotalAmount;
                        decimal amountPerPlayer = 0;
                        if (players.Any())
                        {
                            amountPerPlayer = totalBookingAmount / players.Count();
                        }

                        var user = await userRepository.GetUser(player.UserId);
                        if (user.TelRRefNo != null && amountPerPlayer > 0)
                        {
                            var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                            if (checkPayment.Payload.Order != null)
                            {
                                var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                                var paymentStatus = telRService.AuthPayment(getTelRRefNo, user.TransactionNo, amountPerPlayer);
                                if (paymentStatus.Payload.IsSuccess)
                                {
                                    player.DatePaymentValidated = Helper.GetDateTime();
                                    player.IsPaymentValidated = true;
                                    player.AuthCode = paymentStatus.Payload.TelRefNo;
                                    player.AuthorizedAmount = amountPerPlayer;
                                    await playRepository.UpdateGamePlayer(player);
                                }
                                else
                                {
                                    await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                                }
                            }
                            else
                            {
                                await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                            }
                        }
                        else
                        {
                            await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                        }
                    }
                }
                /*
                foreach (var player in players)
                    {
                        var checkPayment = telRService.CheckPayment(player.TelRRefNo);
                        if (checkPayment.Payload.Order != null)
                        {
                            var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                            if (forDeposit)
                            {
                                telRResponse = telRService.CapturePayment(getTelRRefNo, player.InitialTransactionNo, player.TotalAmount);
                            }
                            else
                            {
                                telRResponse = telRService.CapturePayment(getTelRRefNo, player.InitialTransactionNo, player.TotalAmount);
                            }


                            if (telRResponse.Payload.IsSuccess)
                            {
                                if (forDeposit)
                                {
                                    player.TelRRefNo = telRResponse.Payload.TelRefNo;
                                    player.DatePaid = DateTime.Now;
                                    player.IsPaid = true;
                                }
                                else
                                {
                                    player.IsPaid = true;
                                    player.TelRRefNo = telRResponse.Payload.TelRefNo;
                                    player.DatePaid = DateTime.Now;
                                }
                                //await playRepository.UpdateGamePlayer(player);
                            }
                        }
                    }
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
                */
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> PaymentCaptureProcess(Guid pitchBookingId)
        {
            try
            {
                var telRResponse = new APIResponse<TelRPaymentReponseViewModel>();

                var bookings = await playRepository.GetPitchBooking(pitchBookingId);
                if (bookings != null)
                {

                    var players = await playRepository.GetGamePlayers(pitchBookingId);

                    foreach (var player in players.Where(p => p.IsPaymentValidated == true))
                    {
                        var user = await userRepository.GetUser(player.UserId);
                        decimal amountPerPlayer = 0;
                        decimal totalBookingAmount = player.TotalAmount;
                        if (players.Any())
                        {
                            amountPerPlayer = totalBookingAmount / players.Count();
                        }

                        if (user.TelRRefNo != null && amountPerPlayer > 0)
                        {
                            if (amountPerPlayer == player.AuthorizedAmount || amountPerPlayer > player.AuthorizedAmount)
                            {
                                var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                                if (checkPayment.Payload.Order != null)
                                {
                                    var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                                    var paymentStatus = telRService.CapturePayment(player.AuthCode, user.TransactionNo, amountPerPlayer);
                                    if (paymentStatus.Payload.IsSuccess)
                                    {
                                        telRService.CancelPayment(player.DepositeAuthCode, user.TransactionNo, 1);
                                        player.PlayerStatus = EGamePlayerStatus.Complete;
                                        await playRepository.UpdateGamePlayer(player);
                                    }
                                    else
                                    {
                                        await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                                    }
                                }
                                if (amountPerPlayer > player.AuthorizedAmount)
                                {
                                    var extraAmount = amountPerPlayer - player.AuthorizedAmount;
                                    if (user.TelRRefNo != null && extraAmount > 0)
                                    {
                                        var checkPaymentExtra = telRService.CheckPayment(user.TelRRefNo);
                                        if (checkPaymentExtra.Payload.Order != null)
                                        {
                                            var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                                            var paymentStatus = telRService.AuthPayment(getTelRRefNo, user.TransactionNo, extraAmount);
                                            if (paymentStatus.Payload.IsSuccess)
                                            {
                                                var paymentStatusExtra = telRService.CapturePayment(paymentStatus.Payload.TelRefNo, user.TransactionNo, extraAmount);
                                                if (paymentStatusExtra.Payload.IsSuccess)
                                                {
                                                    player.PlayerStatus = EGamePlayerStatus.Complete;
                                                    await playRepository.UpdateGamePlayer(player);
                                                }
                                                else
                                                {
                                                    await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                                                }
                                            }
                                            else
                                            {
                                                await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                                            }
                                        }
                                        else
                                        {
                                            await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                                        }
                                    }
                                    else
                                    {
                                        await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                                    }
                                }
                            }
                            else
                            {
                                var AmountToCapture = amountPerPlayer - player.AuthorizedAmount;
                                var checkPayment = telRService.CheckPayment(user.TelRRefNo);
                                if (checkPayment.Payload.Order != null)
                                {
                                    var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                                    var paymentStatus = telRService.CapturePayment(player.AuthCode, user.TransactionNo, AmountToCapture);
                                    if (paymentStatus.Payload.IsSuccess)
                                    {
                                        telRService.CancelPayment(player.AuthCode, user.TransactionNo, player.AuthorizedAmount - AmountToCapture);
                                        telRService.CancelPayment(player.DepositeAuthCode, user.TransactionNo, 1);
                                        player.PlayerStatus = EGamePlayerStatus.Complete;
                                        await playRepository.UpdateGamePlayer(player);
                                    }
                                }
                            }
                        }
                        else
                        {
                            await playRepository.SendPlayPaymentFailNotification(pitchBookingId, user.UserId);
                        }
                    }
                }
                /*
                foreach (var player in players)
                    {
                        var checkPayment = telRService.CheckPayment(player.TelRRefNo);
                        if (checkPayment.Payload.Order != null)
                        {
                            var getTelRRefNo = checkPayment.Payload.Order.Transaction.Ref;
                            if (forDeposit)
                            {
                                telRResponse = telRService.CapturePayment(getTelRRefNo, player.InitialTransactionNo, player.TotalAmount);
                            }
                            else
                            {
                                telRResponse = telRService.CapturePayment(getTelRRefNo, player.InitialTransactionNo, player.TotalAmount);
                            }


                            if (telRResponse.Payload.IsSuccess)
                            {
                                if (forDeposit)
                                {
                                    player.TelRRefNo = telRResponse.Payload.TelRefNo;
                                    player.DatePaid = DateTime.Now;
                                    player.IsPaid = true;
                                }
                                else
                                {
                                    player.IsPaid = true;
                                    player.TelRRefNo = telRResponse.Payload.TelRefNo;
                                    player.DatePaid = DateTime.Now;
                                }
                                //await playRepository.UpdateGamePlayer(player);
                            }
                        }
                    }
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
                */
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.RecordSuccess);
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<TelRPaymentReponseViewModel>> RefundPayment(Guid bookingId)
        {
            try
            {
                return await _RefundPayment(bookingId);
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<UserPitchBooking>>> GetAllPitchBookingPriorToStartDate()
        {
            try
            {
                var response = await playRepository.GetAllPitchBookings5MinsPriorToStartDate();
                return new APIResponse<IEnumerable<UserPitchBooking>>()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = response
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<IEnumerable<UserPitchBooking>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<IEnumerable<UserPitchBooking>>> GetAllPitchBookingPrior48hrsToStartDate()
        {
            try
            {
                var response = await playRepository.GetAllPitchBookingPrior48hrsToStartDate();
                return new APIResponse<IEnumerable<UserPitchBooking>>()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = response
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<IEnumerable<UserPitchBooking>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }


        public async Task<APIResponse<TelRResponseViewModel>> RemainingPayment(Guid bookingId)
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();

                var pitchBookings = await playRepository.GetPitchBooking(bookingId);
                if (pitchBookings == null)
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                var playerDetails = await playRepository.GetGamePlayer(bookingId, currentLogin);
                if (playerDetails == null)
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                }

                //if (playerDetails.IsBalancePaid)
                //{
                //    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentAlreadyDone);
                //}

                var user = await userRepository.GetUser(currentLogin);
                var userAddress = await userRepository.GetUserAddress(currentLogin);

                var request = new TelRRequestViewModel()
                {
                    TransactionNo = Guid.NewGuid().ToString(),
                    Amount = pitchBookings.PricePerUser.Value - playerDetails.TotalAmount,
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = userAddress?.CompleteAddress != null ? userAddress?.CompleteAddress : "",
                    City = userAddress?.City != null ? userAddress?.City : "",
                    CountryCode = userAddress?.CountryAlpha3Code != null ? userAddress?.CountryAlpha3Code : "",
                    Email = user.Email
                };
                var response = telRService.Payment(request);

                //playerDetails.BalanceAmount = request.Amount;
                //playerDetails.BalanceTelRRefNo = response.Payload.Order.Ref;
                //playerDetails.BalanceTransactionNo = request.TransactionNo;
                await playRepository.UpdateGamePlayer(playerDetails);
                return response;
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        private async Task<APIResponse<TelRPaymentReponseViewModel>> _RefundPayment(Guid bookingId)
        {
            var currentLogin = userHelper.GetCurrentUserGuidLogin();

            var pitchBookings = await playRepository.GetPitchBooking(bookingId);
            if (pitchBookings == null)
            {
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
            }

            var playerDetails = await playRepository.GetGamePlayer(bookingId, currentLogin);
            if (playerDetails == null)
            {
                return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
            }

            //if (!playerDetails.IsDepositPaid)
            //{
            //    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentNotDone);
            //}

            //var response = telRService.RefundPayment(playerDetails.DepositTelRRefNo, playerDetails.InitialTransactionNo, playerDetails.DepositAmount);
            //if (response.Payload.IsSuccess)
            //{
            //    //playerDetails.IsDepositRefunded = true;
            //    //playerDetails.RefundTelRTransactionNo = response.Payload.TelRefNo;
            //    //playerDetails.DateDepositRefunded = DateTime.Now;
            //    await playRepository.UpdateGamePlayer(playerDetails);
            //}
            return null;
        }

        public async Task<APIResponse<IEnumerable<PlayRequestViewModel>>> GetPlayRequest(Guid bookingId)
        {
            try
            {
                var response = await playRepository.GetPlayRequestByBooking(bookingId);
                return new APIResponse<IEnumerable<PlayRequestViewModel>>()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = response
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<IEnumerable<PlayRequestViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> UpdateGameplayerToPaid(Guid playerId)
        {
            try
            {
                await playRepository.UpdateGameplayerToPaid(playerId);
                // check player is captian

                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = true
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

        public async Task<APIResponse> UpdateGamePlayer(FacilityPlayer facilityPlayer)
        {
            try
            {
                await playRepository.UpdateGamePlayer(facilityPlayer);
                // check player is captian

                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = true
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

        public async Task<APIResponse<InviteRequestResponseModel>> GetInviteRequest(Guid bookingId, string search)
        {
            try
            {
                var response = await playRepository.GetInviteRequestByBooking(bookingId, search);
                return new APIResponse<InviteRequestResponseModel>()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = response
                };
            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<InviteRequestResponseModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> SentInviteRequest(List<string> userSids, Guid bookingId)
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                return await playRepository.SentInviteRequest(bookingId, userSids, currentLogin);

            }
            catch (Exception ex)
            {
                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> MyPlayBooking()
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var myBookings = await playRepository.MyPlayBooking(currentLogin);
                return new APIResponse()
                {
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = myBookings
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

        public async Task<APIResponse> FacilitySendContactMessageToPlayer(FacilitySendContactMessageToPlayerRequestModel messageToPlayerRequestModel)
        {
            try
            {
                return await playRepository.FacilitySendContactMessageToPlayer(messageToPlayerRequestModel);
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
    }
}
