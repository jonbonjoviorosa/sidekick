using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using Sidekick.Model.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class PaymentRepository : APIBaseRepo, IPaymentRepository
    {
        readonly APIDBContext DbContext;
        private readonly IUserHelper userHelper;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private IMainHttpClient MainHttpClient { get; }

        public PaymentRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IMainHttpClient _mClient,
            IUserHelper userHelper)
        {
            DbContext = _dbCtxt;
            this.userHelper = userHelper;
            LogManager = _logManager;
            APIConfig = _apiCon;
            MainHttpClient = _mClient;
        }

        public APIResponse InitiatePayment(string auth, PaymentCheckout payload)
        {
            LogManager.LogInfo("-- Run::PaymentRepository::InitiatePayment --");
            LogManager.LogDebugObject(payload);

            try
            {
                UserLoginTransaction ULT = DbContext.UserLoginTransactions.FirstOrDefault(ultx => ultx.Token == auth);
                if (ULT == null)
                {
                    return new APIResponse
                    {
                        Message = "Error! Please login!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Status = "Failed"
                    };
                }
                string UserTitle = string.Empty;
                UserTitle = "Mr.";
                User PayingUser = DbContext.Users.FirstOrDefault(ux => ux.UserId == ULT.UserId);
                if (PayingUser.Gender == Genders.Female) { UserTitle = "Mrs."; }
                UserAddress PayingUserAdd = DbContext.UserAddresses.FirstOrDefault(ux => ux.UserId == ULT.UserId);
                //PaymentRequest newReq = new PaymentRequest
                //{
                //    bill_addr1 = PayingUserAdd.FloorNum + ", " + PayingUserAdd.DoorNum + ", " + PayingUserAdd.Street,
                //    bill_city = PayingUserAdd.City,
                //    bill_country = PayingUserAdd.CountryAlpha3Code,
                //    bill_email = PayingUser.Email,
                //    bill_fname = PayingUser.FirstName,
                //    bill_phone = PayingUser.MobileNumber,
                //    bill_region = PayingUserAdd.City,
                //    bill_sname = PayingUser.LastName,
                //    bill_zip = "000000",
                //    ivp_lang = "EN",
                //    ivp_trantype = "auth",
                //    bill_title = UserTitle,
                //    ivp_store = Convert.ToInt32(APIConfig.PaymentConfig.ClientId),
                //    ivp_amount = payload.CheckoutAmount,
                //    ivp_currency = "AED",
                //    ivp_test = RequestType.Test,
                //    ivp_cart = "75695675", //order id
                //    ivp_desc = "TEST ORDER",
                //    ivp_authkey = APIConfig.PaymentConfig.ApiKey,
                //    ivp_method = "create",
                //    ivp_signature = APIConfig.TokenKeys.Key,
                //    bill_custref = PayingUser.UserId,
                //    ivp_timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(),
                //    return_auth = APIConfig.PaymentConfig.ReturnAuthURL,
                //    return_can = APIConfig.PaymentConfig.ReturnCancelledURL,
                //   return_decl = APIConfig.PaymentConfig.ReturnDeclinedURL
                //};
                //string newStringcontent = JsonConvert.SerializeObject(newReq);
                //Dictionary<string, string> newDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newStringcontent);
                //PaymentResponse resp = JsonConvert.DeserializeObject<PaymentResponse>(MainHttpClient.TelrHttpClient(APIConfig.PaymentConfig.BaseUrl, newDict));

                //if (resp.Error != null)
                //{
                //    return new APIResponse {
                //        Payload = resp,
                //        Message = resp.Error.Note,
                //        Status = resp.Error.Message,
                //        StatusCode = System.Net.HttpStatusCode.BadRequest
                //    };
                //}
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://secure.telr.com/");
                    client.DefaultRequestHeaders.ExpectContinue = false;

                    var result = client.PostAsync("gateway/order.json",
                    new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("ivp_method", "create"),
                        new KeyValuePair<string, string>("ivp_store", APIConfig.PaymentConfig.ClientId),
                        new KeyValuePair<string, string>("ivp_authkey", APIConfig.PaymentConfig.ApiKey),
                        new KeyValuePair<string, string>("ivp_signature", APIConfig.TokenKeys.Key),
                        new KeyValuePair<string, string>("ivp_cart", "TRN001"),
                        new KeyValuePair<string, string>("ivp_desc", "Company Name"),
                        new KeyValuePair<string, string>("ivp_test", "1"),
                        new KeyValuePair<string, string>("ivp_amount", "100"),
                        new KeyValuePair<string, string>("ivp_currency", "AED"),
                        new KeyValuePair<string, string>("return_auth", APIConfig.PaymentConfig.ReturnAuthURL), // "Success Page"
                        new KeyValuePair<string, string>("return_can", APIConfig.PaymentConfig.ReturnCancelledURL), //"Cancel Page"
                        new KeyValuePair<string, string>("return_decl", APIConfig.PaymentConfig.ReturnDeclinedURL), //"Decline Page"

                        new KeyValuePair<string, string>("bill_title", "Telr Payment"),
                        new KeyValuePair<string, string>("bill_fname", "Prashant"),
                        new KeyValuePair<string, string>("bill_sname", "Nimbare"),
                        new KeyValuePair<string, string>("bill_addr1", "Test"),
                        new KeyValuePair<string, string>("bill_city", "Mumbai"),
                        new KeyValuePair<string, string>("bill_region", "Maharashtra"),
                        new KeyValuePair<string, string>("bill_country", "IN"),
                        new KeyValuePair<string, string>("bill_zip", "400002"),
                        new KeyValuePair<string, string>("bill_email", "test@test.com"),
                        new KeyValuePair<string, string>("ivp_update_url", "www.urdomain.com"),
                        new KeyValuePair<string, string>("ivp_framed", "2"),



                        //new KeyValuePair<string, string>("ivp_method", "create"),
                        //new KeyValuePair<string, string>("ivp_store", "25777"),
                        //new KeyValuePair<string, string>("ivp_authkey", "Q5Bk^HZdX5-2DXNv"),
                        //new KeyValuePair<string, string>("ivp_cart", "12345"),
                        //new KeyValuePair<string, string>("ivp_desc", "Desc"),
                        //new KeyValuePair<string, string>("ivp_test", "1"),
                        //new KeyValuePair<string, string>("ivp_amount", "100.00"),
                        //new KeyValuePair<string, string>("ivp_currency", "AED"),



                        new KeyValuePair<string, string>("ivp_cn", "4000 0000 0000 0002"),
                        new KeyValuePair<string, string>("ivp_exm", "02"),
                        new KeyValuePair<string, string>("ivp_exy", "2029"),
                        new KeyValuePair<string, string>("ivp_cv", "123"),





                    })).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        string data = (result.Content.ReadAsStringAsync().Result);
                        data = JsonConvert.DeserializeObject(data).ToString();
                        string[] arrayResponse = data.Split(':');

                        string[] arr = arrayResponse[4].Split('\"');
                        string val = arr[1];

                        string URL = "https://secure.telr.com/gateway/process.html?o=" + val;
                        //Response.Redirect(URL);
                        return new APIResponse
                        {
                            Message = "Success Init Payment",
                            Status = "Success",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Payload = result
                        };
                    }
                    else
                    {
                        return new APIResponse
                        {
                            Message = "Error",
                            Status = "Failed",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::PaymentRepository::InitiatePayment --");
                LogManager.LogError(ex.Message);
                LogManager.LogDebugObject(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Error",
                    Status = "Failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task InsertUpdatePayment(Payment payment)
        {
            var dateNow = Helper.GetDateTime();
            var currentLogin = userHelper.GetCurrentUserGuidLogin();
            var getPayment = await GetPayment(payment.PaymentId);
            if (getPayment != null)
            {
                getPayment.LastEditedBy = currentLogin;
                getPayment.LastEditedDate = dateNow;
                DbContext.Payment.Update(getPayment);
            }
            else
            {
                payment.PaymentId = Guid.NewGuid();
                payment.IsEnabled = true;
                payment.IsEnabledBy = currentLogin;
                payment.DateEnabled = dateNow;
                payment.CreatedBy = currentLogin;
                payment.CreatedDate = dateNow;
                DbContext.Payment.Add(payment);
            }
            await DbContext.SaveChangesAsync();
        }

        public async Task<Payment> GetPayment(Guid paymentId)
        {
            var data = await DbContext.Payment
                .Where(x => x.PaymentId == paymentId)
                .FirstOrDefaultAsync();
            if (data == null)
            {
                return null;
            }
            return data;
        }

        public async Task<Payment> GetPaymentByBookingID(Guid bookingID)
        {
            var data = await DbContext.Payment
                .Where(x => x.BookingId == bookingID)
                .FirstOrDefaultAsync();
            if (data == null)
            {
                return null;
            }
            return data;
        }

        public async Task<APIResponse> PaymentSummaries()
        {
            var response = new APIResponse();
            try
            {
                LogManager.LogInfo("-- Run::PaymentRepository::PaymentSummaries --");

                var payments = await DbContext.Payment.ToListAsync();
                var viewModel = new PaymentViewModel();
                if (payments.Any())
                {
                    var users = await DbContext.Users.ToListAsync();
                    var groupBookings = await DbContext.GroupBookings.ToListAsync();
                    var groupClasses = await DbContext.GroupClasses.ToListAsync();
                    var individualBookings = await DbContext.IndividualBookings.ToListAsync();
                    var individualClasses = await DbContext.IndividualClasses.ToListAsync();
                    var individualClassDetails = await DbContext.IndividualClassDetails.ToListAsync();
                    var userPitchBookings = await DbContext.UserPitchBookings.ToListAsync();
                    var pitches = await DbContext.FacilityPitches.ToListAsync();

                    viewModel.PaymentCoachings = new List<PaymentCoachingViewModel>();
                    viewModel.PaymentFacilityPitches = new List<PaymentFacilityPitches>();
                    foreach (var item in payments)
                    {
                        if (item.BookingType == (int)EBookingType.Group)
                        {
                            var coachingPayment = new PaymentCoachingViewModel();
                            coachingPayment.ReferenceNumber = item.TransactionNo;
                            var user = users.Where(u => u.UserId == item.CreatedBy).FirstOrDefault();
                            if (user != null)
                            {
                                coachingPayment.ParticipantName = $"{user.FirstName} {user.LastName}";
                            }

                            var groupBooking = groupBookings.Where(g => g.GroupBookingId == item.BookingId).FirstOrDefault();
                            if (groupBooking != null)
                            {
                                var groupClass = groupClasses.Where(g => g.GroupClassId == groupBooking.GroupClassId).FirstOrDefault();
                                if (groupClass != null)
                                {
                                    var coachUser = users.Where(u => u.UserId == groupClass.CoachId).FirstOrDefault();
                                    if (coachUser != null)
                                    {
                                        coachingPayment.CoachName = coachUser.FirstName + " " + coachUser.LastName;
                                    }

                                    coachingPayment.DateBooked = groupBooking.Date.Value;
                                    coachingPayment.Slot = groupClass.Start.Value.ToString("HH:mm") + " - " + groupClass.Start.Value.AddHours(groupClass.Duration.Value).ToString("HH:mm");
                                }
                            }

                            coachingPayment.PaymentDate = item.CreatedDate.Value;
                            coachingPayment.AmountPaid = item.Amount;
                            coachingPayment.Status = item.Status;

                            viewModel.PaymentCoachings.Add(coachingPayment);
                        }
                        else if (item.BookingType == (int)EBookingType.Individual)
                        {
                            var coachingPayment = new PaymentCoachingViewModel();
                            coachingPayment.ReferenceNumber = item.TransactionNo;
                            var user = users.Where(u => u.UserId == item.CreatedBy).FirstOrDefault();
                            if (user != null)
                            {
                                coachingPayment.ParticipantName = $"{user.FirstName} {user.LastName}";
                            }

                            var individualBooking = individualBookings.Where(i => i.BookingId == item.BookingId).FirstOrDefault();
                            if (individualBooking != null)
                            {
                                var individualClass = individualClasses.Where(i => i.ClassId == individualBooking.ClassId).FirstOrDefault();
                                if (individualClass != null)
                                {
                                    var coachUser = users.Where(u => u.UserId == individualClass.CoachId).FirstOrDefault();
                                    if (coachUser != null)
                                    {
                                        coachingPayment.CoachName = coachUser.FirstName + " " + coachUser.LastName;
                                    }

                                    //var detail = individualClassDetails.Where(i => i.IndividualClassId == individualClass.ClassId).FirstOrDefault();
                                    //if (detail != null)
                                    //{
                                    //    coachingPayment.DateBooked = individualBooking.Date;
                                    //    coachingPayment.Slot = individualBooking.StartTime + " - " + individualBooking.EndTime;
                                    //}
                                    coachingPayment.DateBooked = individualBooking.Date;
                                    coachingPayment.Slot = individualBooking.StartTime + " - " + individualBooking.EndTime;
                                }
                            }

                            coachingPayment.PaymentDate = item.CreatedDate.Value;
                            coachingPayment.AmountPaid = item.Amount;
                            coachingPayment.Status = item.Status;

                            viewModel.PaymentCoachings.Add(coachingPayment);
                        }
                        else if(item.BookingType == (int)EBookingType.Play)
                        {
                            var coachingPayment = new PaymentFacilityPitches();
                            coachingPayment.ReferenceNumber = item.TransactionNo;
                            var user = users.Where(u => u.UserId == item.CreatedBy).FirstOrDefault();
                            if (user != null)
                            {
                                coachingPayment.PlayerName = $"{user.FirstName} {user.LastName}";
                            }

                            var userPitchBooking = userPitchBookings.Where(d => d.BookingId == item.BookingId).FirstOrDefault();
                            if(userPitchBooking != null)
                            {
                                var pitch = pitches.Where(f => f.FacilityPitchId == userPitchBooking.FacilityPitchId).FirstOrDefault();
                                if(pitch != null)
                                {
                                    coachingPayment.FacilityPitchName = pitch.Name;
                                }

                                coachingPayment.DateBooked = userPitchBooking.PitchStart.Date;
                                coachingPayment.Slot = userPitchBooking.PitchStart.ToShortTimeString() + " - " + userPitchBooking.PitchEnd.ToShortTimeString();
                            }

                            coachingPayment.PaymentDate = item.CreatedDate.Value;
                            coachingPayment.AmountPaid = item.Amount;
                            coachingPayment.Status = item.Status;

                            viewModel.PaymentFacilityPitches.Add(coachingPayment);
                        }
                    }
                }

                return new APIResponse
                {
                    Message = "Retrieved Successfully",
                    Payload = viewModel,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::PaymentRepository::PaymentSummaries --");
                LogManager.LogError(ex.Message);
                LogManager.LogDebugObject(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Error",
                    Status = "Failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<APIResponse> GetPaymentSummaries()
        {
            var response = new APIResponse();
            try
            {
                LogManager.LogInfo("-- Run::PaymentRepository::GetPaymentSummaries --");
                var payments = await DbContext.Payment.ToListAsync();
                if (payments.Any())
                {
                    return new APIResponse
                    {
                        Message = "Retrieved Payments",
                        Payload = payments.Where(p => p.Status == PaymentStatus.Paid).ToList(),
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                
            }
            catch(Exception ex)
            {
                LogManager.LogInfo("-- Error::PaymentRepository::GetPaymentSummaries --");
                LogManager.LogError(ex.Message);
                LogManager.LogDebugObject(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Error",
                    Status = "Failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            return response;
        }

        public async Task<APIResponse> GetPlayPaymentHistory()
        {
            var response = new APIResponse();
            try
            {
                LogManager.LogInfo("-- Run::PaymentRepository::GetPlayPaymentHistory --");
                var payments = await DbContext.Payment.Where(p => p.BookingType == (int)EBookingType.Play).ToListAsync();
                if (payments.Any())
                {
                    var userPitchBookings = await DbContext.UserPitchBookings.ToListAsync();
                    var pitches = await DbContext.FacilityPitches.ToListAsync();
                    var paymentHistories = new List<PlayPaymentHistory>();
                    foreach (var payment in payments)
                    {
                        var paymentHistory = new PlayPaymentHistory();
                        var userBooking = userPitchBookings.Where(u => u.BookingId == payment.BookingId).FirstOrDefault();
                        if(userBooking != null)
                        {
                            paymentHistory.Id = userBooking.Id;
                            paymentHistory.BookingId = userBooking.BookingId;
                            paymentHistory.DateBooked = userBooking.CreatedDate.Value;
                            paymentHistory.DatePlayed = userBooking.PitchStart;
                            paymentHistory.SlotPlayed = userBooking.PitchStart.ToShortTimeString() + " - " + userBooking.PitchEnd.ToShortTimeString();
                            var pitch = pitches.Where(p => p.FacilityPitchId == userBooking.FacilityPitchId).FirstOrDefault();
                            if(pitch != null)
                            {
                                paymentHistory.PitchNo = pitch.Id;
                                paymentHistory.PitchName = pitch.Name;                              
                            }
                        }
                        paymentHistory.Status = payment.Status;
                        paymentHistories.Add(paymentHistory);
                    }
                    return new APIResponse
                    {
                        Message = "Retrieved Payments",
                        Payload = paymentHistories.Any() ?  paymentHistories.OrderByDescending(p => p.DateBooked).ToList() : paymentHistories,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                return new APIResponse
                {
                    Message = "No Payment History Available for Play",
                    Payload = new List<PlayPaymentHistory>(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("-- Error::PaymentRepository::GetPlayPaymentHistory --");
                LogManager.LogError(ex.Message);
                LogManager.LogDebugObject(ex.StackTrace);

                return new APIResponse
                {
                    Message = "Error",
                    Status = "Failed",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }
    }
}
