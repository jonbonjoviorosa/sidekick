using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.ViewModel
{
    public class TelRResponseViewModel
    {
        public string Method { get; set; }

        public string Trace { get; set; }

        public string TransactionNo { get; set; }
        public TelRResponseOrderViewModel Order { get; set; }
        public TelRResponseErrorViewModel Error { get; set; }

        public IndividualConfirmBookingViewModel bookingDetails { get; set; }
        public GroupBookingViewModel groupBookingDetails { get; set; }
        public PlayBookingModel playBookingDetails { get; set; }

        public bool PaymentDone { get; set; }

    }

    public class TelRResponseOrderViewModel
    {
        public string Ref { get; set; }
        public string Url { get; set; }
    }
}
