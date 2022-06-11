using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.ViewModel
{
    public class TelRCheckPaymentResponseViewModel
    {
        public string Check { get; set; }
        public string Trace { get; set; }
        public TelRCheckPaymentOrderResponseViewModel Order { get; set; }
        public IndividualConfirmBookingViewModel BookingDetails { get; set; }
        public GroupBookingViewModel GroupBookingDetails { get; set; }
        public PlayBookingModel PlayBookingDetails { get; set; }
        public TelRResponseErrorViewModel Error { get; set; }
    }

    public class TelRCheckPaymentOrderResponseViewModel
    {
        public string Ref { get; set; }
        public string Cartid { get; set; }
        public string Test { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public TelRCheckPaymentStatusResponseViewModel Status { get; set; }
        public TelRCheckPaymentTransactionResponseViewModel Transaction { get; set; }
        public string Paymethod { get; set; }
        public cardModel card { get; set; }
        public customerModel customer { get; set; }
    }

    public class TelRCheckPaymentStatusResponseViewModel
    {
        public string Code { get; set; }
        public string text { get; set; }
    }

    public class TelRCheckPaymentTransactionResponseViewModel
    {
        public string Ref { get; set; }
        public string date { get; set; }
        public string type { get; set; }
        public string Class { get; set; }
        public string status { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public cardModel card { get; set; }
        public customerModel customer { get; set; }
    }

    public class cardModel
    {
        public string type { get; set; }
        public string last4 { get; set; }
        public string country { get; set; }
        public string first6 { get; set; }
        public cardExpireModel expiry { get; set; }
    }

    public class cardExpireModel
    {
        public string month { get; set; }
        public string year { get; set; }
    }

    public class customerModel
    {
        public string email { get; set; }
        public customerNameModel name { get; set; }
    }

    public class customerNameModel
    {
        public string forenames { get; set; }
        public string surname { get; set; }
    }



}
