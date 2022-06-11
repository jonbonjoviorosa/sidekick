using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class CommissionTrain : APIBaseModel
    {
        public decimal CoachingGroupComission { get; set; }
        public decimal CoachingIndividualComission { get; set; }
    }

    public class CommisionReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double TotalSalesAmount { get; set; }
        public double CommissionAmount { get; set; }
        public double VatAmount { get; set; }
        public EBookingType BookingType { get; set; }
    }
}
