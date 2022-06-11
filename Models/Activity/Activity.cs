
using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class Activity : APIBaseModel
    {
        public Guid ActivityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UserActivities
    {
        public List<ActivityList> Activities { get; set; }
        public List<ActivityList> ActivityRequest { get; set; }
        public List<WhatsNewList> WhatsNew { get; set; }
        public List<string> BookingDates { get; set; }
        public List<Banner> Banner { get; set; }
    }

    public class ActivityList
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Locations { get; set; }
        public decimal Price { get; set; }
        public string StartIn { get; set; }
        public string CreatedDate { get; set; }
        public ReviewType Type { get; set; }
    }

    public class WhatsNewList
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string CreatedDate { get; set; }
    }

    public class UserReviewList
    {
        public Ratings Ratings { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public DateTime? Date { get; set; }
    }

    public class UpcomingBooking
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }
        public string Time { get; set; }
        public string Image { get; set; }
    }

    public class CoachHomeResponse
    {
        public List<CoachHome> CoachHomes { get; set; }
        public List<string> BookingDates { get; set; }
    }

    public class CoachHome
    {
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public string ProfileName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReviewType BookingType { get; set; }
        public ReviewType Status { get; set; }
    }

    public class BookingDetail
    {
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid GroupClassId { get; set; }
        public string ProfileName { get; set; }
        public string Activity { get; set; }
        public string ImageUrl { get; set; }
        public string Date { get; set; }
        public string Slot { get; set; }
        public string Coaching { get; set; }
        public bool Frequency { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
        public decimal Total { get; set; }
        public string TotalParticipant { get; set; }
        public int Participants { get; set; }
        public List<Participant> Participant { get; set; }
    }

    public class Participant
    {
        public Guid UserId { get; set; }
        public string ImageURL { get; set; }
        public string ProfileName { get; set; }
        public string Status { get; set; }
    }
}
