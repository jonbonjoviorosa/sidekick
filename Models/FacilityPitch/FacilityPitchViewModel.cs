using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class FacilityPitchDto
    {
        public Guid? FacilityPitchId { get; set; }
        //[Required(ErrorMessage = "This field is required.")]
        public Guid? FacilityId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public Guid SportId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "This field is required.")]
        public string Description { get; set; }

        //[Required(ErrorMessage = "This field is required.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public int MaxPlayers { get; set; }

        public bool IsFacilityTime { get; set; } // to be removed??
        public bool IsFixedPrice { get; set; }
        public decimal FixedPrice { get; set; }
        public int Divisions { get; set; } // number of divisions, minimum should be 1 ??

        public bool IsEnabled { get; set; }
        public Guid SurfaceId { get; set; }
        public Guid TeamSize { get; set; }
        public Guid LocationId { get; set; }
        public string PlayerIds { get; set; }

        public List<FacilityPitchTimings> FacilityPitchTimings { get; set; }
        public List<FacilityPlayer> FacilityPlayers { get; set; }
        public bool IsAddSlot { get; set; }
        public string FacilityPitchTimingIds { get; set; }
        public int Id { get; set; }
        public string TimingIdsToRemove { get; set; }

        public string SportName { get; set; }

        public string LocationName { get; set; }

        public string SurfaceName { get; set; }

        public string TeamSizeName { get; set; }
    }

    public class FacilityPitchTimings
    {
        public Guid FacilityPitchId { get; set; }
        public Guid FacilityPitchTimingId { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public decimal CustomPrice { get; set; }
        public DateTime Date { get; set; }
        public bool IsRepeatEveryWeek { get; set; }
        public bool IsFree { get; set; }
        public bool IsBooked { get; set; }
        public string PlayerIds { get; set; }
    }

    public class FacilityPitchList
    {
        public Guid? FacilityId { get; set; }
        public Guid? FacilityPitchId { get; set; }
        public Guid SportId { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public string ImageUrl { get; set; }
        public string OpeningHours { get; set; }
        public string Location { get; set; }
        public int CurrentBooking { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? IsEnabled { get; set; }
        public string Sport { get; set; }
        public List<FacilityPitchTiming> FacilityPitchTimings { get; set; }
        public List<UserPitchBooking> Bookings { get; set; }
        public List<SlotRenderViewModel> MappedBookings { get; set; }
        public string FacilityPitchTimingIds { get; set; }
    }

    public class FacilityPitchVM
    {
        public int Id { get; set; }
        public Guid? FacilityPitchId { get; set; }
        public Guid? FacilityId { get; set; }
        public Guid SportId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsFacilityTime { get; set; }
        public bool IsFixedPrice { get; set; }
        public int Divisions { get; set; } // number of divisions, minimum should be 1
        public decimal FixedPrice { get; set; }
        public Guid SurfaceId { get; set; }
        public Guid TeamSize { get; set; }
        public Guid LocationId { get; set; }
        public string Location { get; set; }
        public Guid AreaId { get; set; }
        public bool IsEnabled { get; set; }
        public string PlayerIds { get; set; }
        public List<FacilityPitchTiming> FacilityPitchTimings { get; set; }
        public List<FacilityPitchTiming> BookingPitchTimings { get; set; }
        public List<UserPitchBooking> Bookings { get; set; }
        public bool IsAddSlot { get; set; }
        public List<SlotRenderViewModel> MappedBookings { get; set; }
        public string TimingIdsToRemove { get; set; }
        public List<PlayerPitchViewModel> OnLoadPlayerDetails { get; set; }
    }

    public class SlotRenderViewModel
    {
        public Guid GuidId { get; set; }
        public Guid BoookingID { get; set; }
        public DateTime PitchDate { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public decimal Price { get; set; }
        public string PitchName { get; set; }
        public int MaxPlayers { get; set; }
        public List<PlayerPitchViewModel> Players { get; set; }
        public int PlayerCount { get; set; }
        public Guid FacilityID { get; set; }
        public Guid FacilityPitchID { get; set; }
        public Guid SportID { get; set; }
        public Guid FacilityPitchTimingID { get; set; }
    }

    public class PlayerPitchViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsPaid { get; set; }
        public string Email { get; set; }
        public string ProfileImgUrl { get; set; }
        public bool IsCaptain { get; set; }
    }

    public class DisplayPitch
    {
        public List<PlayerPitchViewModel> Players { get; set; }
        public FacilityPitchTiming Timing { get; set; }
    }
}
