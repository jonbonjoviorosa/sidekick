using Sidekick.Api.Handlers.IBusiness;
using Sidekick.Api.Helpers;
using Sidekick.Api.Service.IService;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.PaymentMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.Business
{
    public class PaymentHandler : IPaymentHandler
    {
        private readonly ILoggerManager loggerManager;
        private readonly ITelRService telRService;
        private readonly IBookingHandler bookingHandler;
        private readonly IPlayHandler playHandler;
        private readonly IPaymentMethodHandler paymentMethodHandler;

        public PaymentHandler(ILoggerManager loggerManager, ITelRService telRService, IBookingHandler bookingHandler, IPlayHandler playHandler, IPaymentMethodHandler paymentMethodHandler)
        {
            this.loggerManager = loggerManager;
            this.telRService = telRService;
            this.bookingHandler = bookingHandler;
            this.playHandler = playHandler;
            this.paymentMethodHandler = paymentMethodHandler;
        }

        public async Task<APIResponse<TelRCheckPaymentResponseViewModel>> CheckStatusPayment(string orderRef)
        {
            try
            {
                //var currentLogin = userHelper.GetCurrentUserGuidLogin();

                //var pitchBookings = await playRepository.GetPitchBooking(bookingId);
                //if (pitchBookings == null)
                //{
                //    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                //}

                //var playerDetails = await playRepository.GetGamePlayer(bookingId, currentLogin);
                //if (playerDetails == null)
                //{
                //    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.NotExist);
                //}

                //if (playerDetails.IsDepositPaid)
                //{
                //    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.PaymentAlreadyDone);
                //}

                //var user = await userRepository.GetUser(currentLogin);
                //var userAddress = await userRepository.GetUserAddress(currentLogin);

                //var request = new TelRRequestViewModel()
                //{
                //    TransactionNo = Guid.NewGuid().ToString(),
                //    Amount = pitchBookings.PricePerUser.Value,
                //    UserId = user.UserId,
                //    FirstName = user.FirstName,
                //    LastName = user.LastName,
                //    Address = userAddress.CompleteAddress,
                //    City = userAddress.City,
                //    CountryCode = userAddress.CountryAlpha3Code,
                //    Email = user.Email
                //};

                var response = telRService.CheckPayment(orderRef);
                if (response.Payload.Order.Status.Code == "2" || response.Payload.Order.Status.Code == "3")
                {
                    var bookingDetail = await this.bookingHandler.GetIndivdualBookingsByTelRRefNo(orderRef);
                    if (bookingDetail.Payload != null)
                    {
                        await this.bookingHandler.UpdateIndividualBookingPaymentPaid(bookingDetail.Payload.BookingId);
                        var confirmBookingResponse = await this.bookingHandler.ConfirmIndividualBooking(bookingDetail.Payload.BookingId);
                        if (confirmBookingResponse.Payload != null)
                        {
                            var paymentMethod_CardViewModel = new PaymentMethod_CardViewModel();
                            if (response.Payload.Order.card != null)
                            {
                                paymentMethod_CardViewModel.CardNumber = response.Payload.Order.card.first6 + "XX" + response.Payload.Order.card.last4;
                                paymentMethod_CardViewModel.ExpirationDate_Month = response.Payload.Order.card.expiry.month;
                                paymentMethod_CardViewModel.ExpirationDate_Year = response.Payload.Order.card.expiry.year;
                                paymentMethod_CardViewModel.CardType = response.Payload.Order.card.type;
                                paymentMethod_CardViewModel.TelRRefNo = orderRef;
                                paymentMethod_CardViewModel.TransactionNo = response.Payload.Order.Transaction.Ref;
                                paymentMethod_CardViewModel.FullName = response.Payload.Order.customer.name.forenames + " " + response.Payload.Order.customer.name.surname;
                                await paymentMethodHandler.InsertUpdatePaymentMethodCard(paymentMethod_CardViewModel);
                            }

                            response.Payload.BookingDetails = confirmBookingResponse.Payload;
                        }
                    }
                    else
                    {
                        var groupbookingDetail = await this.bookingHandler.GetGroupBookingsByTelRRefNo(orderRef);
                        if (groupbookingDetail.Payload != null)
                        {
                            await this.bookingHandler.UpdateGroupBookingPaymentPaid(groupbookingDetail.Payload.GroupBookingId);
                            response.Payload.GroupBookingDetails = groupbookingDetail.Payload;

                            var paymentMethod_CardViewModel = new PaymentMethod_CardViewModel();
                            if (response.Payload.Order.card != null)
                            {
                                paymentMethod_CardViewModel.CardNumber = response.Payload.Order.card.first6 + "XX" + response.Payload.Order.card.last4;
                                paymentMethod_CardViewModel.ExpirationDate_Month = response.Payload.Order.card.expiry.month;
                                paymentMethod_CardViewModel.ExpirationDate_Year = response.Payload.Order.card.expiry.year;
                                paymentMethod_CardViewModel.CardType = response.Payload.Order.card.type;
                                paymentMethod_CardViewModel.TelRRefNo = orderRef;
                                paymentMethod_CardViewModel.TransactionNo = response.Payload.Order.Transaction.Ref;
                                paymentMethod_CardViewModel.FullName = response.Payload.Order.customer.name.forenames + " " + response.Payload.Order.customer.name.surname;
                                await paymentMethodHandler.InsertUpdatePaymentMethodCard(paymentMethod_CardViewModel);
                            }

                        }
                        else
                        {
                            var playbookingDetail = await this.playHandler.GetPitchBookingByTelRRefNo(orderRef);
                            var PlaybookingDetails = (PlayBookingModel)playbookingDetail.Payload;
                            if (PlaybookingDetails.BookingId != null && PlaybookingDetails.BookingId != Guid.Empty)
                            {
                                var paymentMethod_CardViewModel = new PaymentMethod_CardViewModel();
                                if (response.Payload.Order.card != null)
                                {
                                    paymentMethod_CardViewModel.CardNumber = response.Payload.Order.card.first6 + "XX" + response.Payload.Order.card.last4;
                                    paymentMethod_CardViewModel.ExpirationDate_Month = response.Payload.Order.card.expiry.month;
                                    paymentMethod_CardViewModel.ExpirationDate_Year = response.Payload.Order.card.expiry.year;
                                    paymentMethod_CardViewModel.CardType = response.Payload.Order.card.type;
                                    paymentMethod_CardViewModel.TelRRefNo = orderRef;
                                    paymentMethod_CardViewModel.TransactionNo = response.Payload.Order.Transaction.Ref;
                                    paymentMethod_CardViewModel.FullName = response.Payload.Order.customer.name.forenames + " " + response.Payload.Order.customer.name.surname;
                                    await paymentMethodHandler.InsertUpdatePaymentMethodCard(paymentMethod_CardViewModel);
                                }


                                response.Payload.PlayBookingDetails = (PlayBookingModel)playbookingDetail.Payload;
                                await this.playHandler.UpdateGameplayerToPaid(response.Payload.PlayBookingDetails.GamePlayerId);
                            }
                            else
                            {
                                var paymentMethod_CardViewModel = new PaymentMethod_CardViewModel();
                                if(response.Payload.Order.card != null)
                                {
                                    paymentMethod_CardViewModel.CardNumber = response.Payload.Order.card.first6 + "XX" + response.Payload.Order.card.last4;
                                    paymentMethod_CardViewModel.ExpirationDate_Month = response.Payload.Order.card.expiry.month;
                                    paymentMethod_CardViewModel.ExpirationDate_Year = response.Payload.Order.card.expiry.year;
                                    paymentMethod_CardViewModel.CardType = response.Payload.Order.card.type;

                                    paymentMethod_CardViewModel.TelRRefNo = orderRef;
                                    paymentMethod_CardViewModel.TransactionNo = response.Payload.Order.Transaction.Ref;

                                    paymentMethod_CardViewModel.FullName = response.Payload.Order.customer.name.forenames + " " + response.Payload.Order.customer.name.surname;
                                    await paymentMethodHandler.InsertUpdatePaymentMethodCard(paymentMethod_CardViewModel);
                                }
                                
                            }
                        }


                    }


                }

                //playerDetails.DepositAmount = 1;
                //playerDetails.TelRInitialTransactionNo = response.Payload.Order.Ref;
                //playerDetails.InitialTransactionNo = request.TransactionNo;

                //playerDetails.BalanceAmount = pitchBookings.PricePerUser.Value - playerDetails.DepositAmount;
                //await playRepository.UpdateGamePlayer(playerDetails);
                return response;
            }
            catch (Exception ex)
            {
                if (ex.Message == EResponseAction.Unauthorized.ToString())
                    return APIResponseHelper<TelRCheckPaymentResponseViewModel>.ReturnAPIResponse(EResponseAction.Unauthorized);

                loggerManager.LogInfo(ETransaction.FAILED, Helper.GetCurrentMethodName(), EOperation.SELECT);
                loggerManager.LogException(ex);
                return APIResponseHelper<TelRCheckPaymentResponseViewModel>.ReturnAPIResponse(EResponseAction.InternalServerError);
            }
        }
    }
}
