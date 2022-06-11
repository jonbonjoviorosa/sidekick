using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Sidekick.Model.Payment;

namespace Sidekick.Model
{
    public class DasboardViewModel
    {
        public IEnumerable<FacilityPlayerViewModel> Players { get; set; }
        public IEnumerable<FacilityPlayer> FacilityPlayers { get; set; }
        public IEnumerable<Facility> Facilities { get; set; }
        public IEnumerable<CoachViewModel> Coaches { get; set; }
        public IEnumerable<User> Users { get; set; }
        public List<Payment.Payment> Payments { get; set; }
        public PaymentViewModel PaymentSummary { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalTrainRevenue { get; set; }
        public decimal TotalPlayRevenue { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<AddSlotViewModel> PlayBookings { get; set; }
        public List<ClassRenderViewModel> TrainBookings { get; set; }
        public List<SelectListItem> Years { get; set; }
        public IEnumerable<UserPitchBooking> Bookings { get; set; }
        public List<SlotRenderViewModel> MappedBookings { get; set; }
        #region Users Report
        public List<GroupYear> FacilityPlayerGroup { get; set; }
        public List<GroupYear> FacilitiesGroup { get; set; }
        public List<GroupYear> CoachesGroup { get; set; }
        public List<GroupLastMonthYear> FacilityPlayerGroupLastMonth { get; set; }
        public List<GroupLastMonthYear> FacilitiesGroupLastMonth { get; set; }
        public List<GroupLastMonthYear> CoachesGroupLastMonth { get; set; }
        public List<GroupLastMonthYear> FacilityPlayerGroupThisMonth { get; set; }
        public List<GroupLastMonthYear> FacilitiesGroupThisMonth { get; set; }
        public List<GroupLastMonthYear> CoachesGroupThisMonth { get; set; }
        public List<LastSevenDays> FacilityPlayerGroupLastSeven { get; set; }
        public List<LastSevenDays> FacilitiesGroupLastSeven { get; set; }
        public List<LastSevenDays> CoachesGroupLastSeven { get; set; }
        #endregion
        #region Bookings Report
        public List<GroupYear> BookingsGroupByYear { get; set; }
        public List<GroupLastMonthYear> BookingsGroupByLastMonth { get; set; }
        public List<GroupLastMonthYear> BookingsGroupByCurrentMonth { get; set; }
        public List<LastSevenDays> BookingsGroupByLastSeven { get; set; }
        #endregion

        public IEnumerable<FacilityPitchList> FacilityPitches { get; set; }
        public List<LastSevenDays> FacilityPitchesLastSeven { get; set; }

        public List<GroupYear> PlayBookingsThisYear { get; set; }
        public List<GroupLastMonthYear> PlayBookingsCurrentMonth { get; set; }
        public List<GroupLastMonthYear> PlayBookingsLastMonth { get; set; }
        public List<LastSevenDays> PlayBookingsLastSeven { get; set; }

        public List<GroupYear> TrainBookingsThisYear { get; set; }
        public List<GroupLastMonthYear> TrainBookingsCurrentMonth { get; set; }
        public List<GroupLastMonthYear> TrainBookingsLastMonth { get; set; }
        public List<LastSevenDays> TrainBookingsLastSeven { get; set; }


        public List<GroupYear> TrainPaymentsGroupYear { get; set; }
        public List<GroupYear> PlayPaymentsGroupYear { get; set; }
        public List<GroupLastMonthYear> TrainPaymentsGroupLastMonth { get; set; }
        public List<GroupLastMonthYear> PlayPaymentsGroupLastMonth { get; set; }
        public List<GroupLastMonthYear> TrainPaymentsGroupThisMonth { get; set; }
        public List<GroupLastMonthYear> PlayPaymentsGroupThisMonth { get; set; }
        public List<LastSevenDays> TrainPaymentLastSevenDays { get; set; }
        public List<LastSevenDays> PlayPaymentLastSevenDays { get; set; }

    }

    public class GroupYear
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int ObjectCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class GroupLastMonthYear
    {
        public int Year { get; set; }
        public int LastMonth { get; set; }
        public int WeekNumber { get; set; }
        public int ObjectCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class LastSevenDays
    {
        public int Day { get; set; }
        public int Year { get; set; }
        public int ObjectCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
