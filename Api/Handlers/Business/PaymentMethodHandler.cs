using AutoMapper;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Api.Service.IService;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.PaymentMethod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class PaymentMethodHandler : IPaymentMethodHandler
    {
        private readonly IUserHelper userHelper;
        private readonly ILoggerManager loggerManager;
        private readonly IMapper mapper;
        private readonly ITelRService telRService;
        private readonly IUserRepository userRepository;
        private readonly IPaymentMethodRepository paymentMethodRepository;

        public PaymentMethodHandler(IUserHelper userHelper,
            ILoggerManager loggerManager,
            IMapper mapper,
            IUserRepository userRepository,
            ITelRService telRService,
            IPaymentMethodRepository paymentMethodRepository)
        {
            this.userHelper = userHelper;
            this.loggerManager = loggerManager;
            this.mapper = mapper;
            this.telRService = telRService;
            this.userRepository = userRepository;
            this.paymentMethodRepository = paymentMethodRepository;
        }

        public async Task<APIResponse<IEnumerable<PaymentMethod_CardViewModel>>> GetPaymentMethodCards()
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var getCards = await paymentMethodRepository.GetPaymentMethodCards(currentLogin);
                var mappedCards = mapper.Map<IEnumerable<PaymentMethod_CardViewModel>>(getCards);

                return new APIResponse<IEnumerable<PaymentMethod_CardViewModel>>
                {
                    Status = Status.Success,
                    Payload = mappedCards,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<IEnumerable<PaymentMethod_CardViewModel>>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);

                return APIResponseHelper<IEnumerable<PaymentMethod_CardViewModel>>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse<PaymentMethod_CardViewModel>> GetPaymentMethodCard(Guid cardId)
        {
            try
            {
                loggerManager.LogInfo(ETransaction.RUN, Helper.GetCurrentMethodName(), EOperation.SELECT);
                var getCard = await paymentMethodRepository.GetPaymentMethodCard(cardId);
                var mappedCard = mapper.Map<PaymentMethod_CardViewModel>(getCard);

                return new APIResponse<PaymentMethod_CardViewModel>
                {
                    Status = Status.Success,
                    Payload = mappedCard,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<PaymentMethod_CardViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogError(ex.InnerException.Message);
                loggerManager.LogError(ex.StackTrace);

                return APIResponseHelper<PaymentMethod_CardViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }

        public async Task<APIResponse> InsertUpdatePaymentMethodCard(PaymentMethod_CardViewModel card)
        {
            try
            {
                var currentDate = Helper.GetDateTime();
                var _2DigitYr = Convert.ToInt32(card.ExpirationDate_Year);
                var _4DigitYr = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(_2DigitYr);
                if (currentDate.Year <= _4DigitYr)
                {
                    if (currentDate.Year == _4DigitYr)
                    {
                        if (currentDate.Month > Convert.ToInt32(card.ExpirationDate_Month))
                        {
                            return APIResponseHelper.ReturnAPIResponse(EResponseAction.PaymentMethod_CardExpired);
                        }
                    }
                }
                else
                {
                    return APIResponseHelper.ReturnAPIResponse(EResponseAction.PaymentMethod_CardExpired);
                }
                var mappedCard = mapper.Map<PaymentMethod_Card>(card);
                await paymentMethodRepository.InsertUpdatePaymentMethodCard(mappedCard);


                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var user = await userRepository.GetUser(currentLogin);

                user.TelRRefNo = card.TelRRefNo;
                user.TransactionNo = card.TransactionNo;
                await userRepository.UpdateUser(user);

                return new APIResponse
                {
                    Status = Status.Success,
                    Message = Messages.RecordedSuccess,
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


        public async Task<APIResponse<TelRResponseViewModel>> InitiatePaymentMethod()
        {
            try
            {
                var currentLogin = userHelper.GetCurrentUserGuidLogin();
                var user = await userRepository.GetUser(currentLogin);
                var userAddress = await userRepository.GetUserAddress(currentLogin);

                var payment = new TelRRequestViewModel()
                {
                    TransactionNo = Helper.TransactionNoGeneratorForUser(),
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
                    user.TelRRefNo = telRResponse.Payload.Order.Ref;
                    user.TransactionNo = telRResponse.Payload.TransactionNo;
                    await userRepository.UpdateUser(user);
                }

                return telRResponse;
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
    }
}
