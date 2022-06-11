using Sidekick.Model.Class;
using Sidekick.Model.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    public class Coach : APIBaseModel
    {
        public Guid? CoachUserId { get; set; }
        public int Experience { get; set; }
        public string LocationLong { get; set; }
        public string LocationLat { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }

    public class CoachSpecialty : APIBaseModel
    {
        public Guid CoachUserId { get; set; }
        public Guid SpecialtyId { get; set; }
    }

    public class CoachLanguage : APIBaseModel
    {
        public Guid CoachUserId { get; set; }
        public Guid LanguageId { get; set; }
    }

    public class CoachGymAccess : APIBaseModel
    {
        public Guid CoachUserId { get; set; }
        public Guid GymID { get; set; }
    }

    public class CoachProfile : Coach
    {
        public List<CoachSpecialty> Specialties { get; set; }
        public List<CoachLanguage> Languages { get; set; }
        public List<CoachGymAccess> Gyms { get; set; }
        public CoachUpdateProfile Profile { get; set; }
    }

    public class CoachProfileView
    {
        public CoachDetail Profile { get; set; }
        public Notation Rating { get; set; }
        public List<CoachingSpecialties> Specialties { get; set; }
        public List<CoachingBadges> Badges { get; set; }
    }

    public class CoachDetail
    {
        public int UserID { get; set; }
        public string ImageUrl { get; set; }
        public string ProfileName { get; set; }        
        public decimal Experience { get; set; }
        public int Age { get; set; }
        public decimal CoachingPrice { get; set; }
        public decimal Duration { get; set; }
        public decimal ParticipantOffer { get; set; }
        public decimal FriendCount { get; set; }
        public string Location { get; set; }
        public string LocationLong { get; set; }
        public string LocationLat { get; set; }
        public Guid NationalityId { get; set; }
        public decimal Rating { get; set; }
        public string Description { get; set; }
    }

    [Table("CoachEverydaySchedules")]
    public class CoachEverydaySchedule : APIBaseModel
    {
        public Guid CoachId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    [Table("CoachCustomSchedules")]
    public class CoachCustomSchedule : APIBaseModel
    {
        public Guid CoachId { get; set; }
        public CoachingDay Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class CoachUpdateProfile
    {
        public Guid UserID { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Experience { get; set; }
        public string Location { get; set; }
        public Genders Gender { get; set; }
        public List<Guid> Languages { get; set; }
        public List<Guid> Specialties { get; set; }
        public List<Guid> GymsAccess { get; set; }
        public string Description { get; set; }
        public bool IsWeekPersonalized { get; set; }
        public List<CoachCustomScheduleViewModel> CustomSched { get; set; }
        public CoachEverydayScheduleViewModel EverydaySched { get; set; }
        public bool IsActive { get; set; }

        //public CoachSceduleViewModel Schedule { get; set; }
    }

    public class CoachCustomScheduleViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public CoachingDay Day { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public string EndTime { get; set; }
    }



    public class CoachEverydayScheduleViewModel
    {
        [Required(ErrorMessage = "{0} is required!")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        public string EndTime { get; set; }
    }

    public class CoachScheduleViewModel
    {
        public bool IsWeekPersonalized { get; set; }

        public IEnumerable<CoachCustomScheduleViewModel> CustomSchedule { get; set; }

        public CoachEverydayScheduleViewModel EverydaySchedule { get; set; }
    }

    public class CoachProfileViewModel
    {
        public string ImageUrl { get; set; }
        //public string Url { get; set; }
        public double Rating { get; set; }
        public int FriendCount { get; set; }
        public decimal PriceHours { get; set; }
        public string Duration { get; set; }
        public string Offer { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public List<string> CoachLanguages { get; set; }
        public List<string> Expert { get; set; }
        public List<string> Badges { get; set; }
        public string Description { get; set; }
        public Review review { get; set; }
        public int AreaId { get; set; }

    }

    public class Review {
        public string UserImg { get; set; }
        public string UserName { get; set; }
        public DateTime ReviewDate { get; set; }
    }
    public class CoachInfoViewModel
    {
        public string ImageUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string mobile { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int AreaId { get; set; }
        public string countryAlpha3Code { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public IEnumerable<LanguageViewModel> Languages { get; set; }
        public IEnumerable<ExpertViewModel> Expert { get; set; }
        public List<string> Budges { get; set; }
        public string Description { get; set; }
        public IEnumerable<GymAccessViewModel> gymAccesses { get; set; }

    }
    public class GymAccessViewModel
    {
        public Guid GymId { get; set; }
        public string Gym { get; set; }
        public string Image { get; set; }
    }
    public class ExpertViewModel
    {
        public Guid ExpertId { get; set; }
        public string Name { get; set; }
    }

    public class CoachEditProfileFormModel
    {
        public string ImageUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string mobile { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountryAlpha3Code { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public IEnumerable<LanguageFormModel> Languages { get; set; }
        public IEnumerable<GymAccessFormModel> gymAccesses { get; set; }
        public IEnumerable<ExpertFormModel> Expert { get; set; }
        public string Description { get; set; }
        public int AreaId { get; set; }

    }

    public class LanguageFormModel {
        public Guid LanguageId { get; set; }
    }

    public class GymAccessFormModel
    {
        public Guid GymId { get; set; }
    }

    public class ExpertFormModel
    {
        public Guid ExpertId { get; set; }
    }

    public class MyCoachingGroupListViewModel
    {
        public List<UpcomingClassesViewModel> upcomingClasses { get; set; }

        public List<CoachingHistoryViewModel> coachingHistory { get; set; }
    }
    public class UpcomingClassesViewModel
    {
        public string title { get; set; }
        public decimal price { get; set; }
        public string time { get; set; }
        public string createdDate { get; set; }
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public ReviewType BookingType { get; set; }
    }
    public class CoachingHistoryViewModel
    {
        public string title { get; set; }
        public decimal price { get; set; }
        public string time { get; set; }
        public string createdDate { get; set; }
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public ReviewType BookingType { get; set; }
    }

    public class MyIndividualGroupListViewModel
    {
        public List<IndividualUpcomingClassesViewModel> upcomingClasses { get; set; }

        public List<IndividualCoachingHistoryViewModel> coachingHistory { get; set; }
    }

    public class IndividualUpcomingClassesViewModel
    {
        public string title { get; set; }
        public string time { get; set; }
        public string createdDate { get; set; }
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public ReviewType BookingType { get; set; }
    }
    public class IndividualCoachingHistoryViewModel
    {
        public string title { get; set; }
        public string time { get; set; }
        public string createdDate { get; set; }
        public int Id { get; set; }
        public Guid BookingId { get; set; }
        public ReviewType BookingType { get; set; }
    }

    public class GetCoachAbsentSlotViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string time { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string city { get; set; }
        public string day { get; set; }
        public string type { get; set; }
        public Guid UniqueID { get; set; }
    }

    public class CoachAbsentSlotViewModel
    {
        public CoachAbsentSlotViewModel()
        {
            once = new List<OnceViewModel>();
            punctual = new List<PunctualViewModel>();
        }
        public string day { get; set; }
        public List<OnceViewModel> once { get; set; }
        public List<PunctualViewModel> punctual { get; set; }
    }

    public class OnceViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string time { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string city { get; set; }
    }

    public class PunctualViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string time { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string city { get; set; }
    }


    [Table("CoachAbsences")]
    public class CoachAbsences 
    {
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }
        public Guid? LastEditedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? LastEditedDate { get; set; }
        public Guid? CreatedBy { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }
        public Guid CoachUserId { get; set; }
        public Guid UniqueID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AllDay { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public Guid? LocationId { get; set; }
        public string Note { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int RepeatWeek { get; set; }
    }

    public class CoachCoachAbsentSlotViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AllDay { get; set; }
        public int Type { get; set; }
        public string Title { get; set; }
        public Guid? LocationId { get; set; }
        public string Note { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int RepeatWeek { get; set; }
    }

    public class GetCoachAbsentSlotDetailsViewModel
    {
        public int id { get; set; }
        public int allDays { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public string starttime { get; set; }
        public string endtime { get; set; }
        public int duration { get; set; }
        public int type { get; set; }
        public string title { get; set; }
        public Guid locationId { get; set; }
        public string location { get; set; }
        public string note { get; set; }
        public int repeatWeek { get; set; }

    }

    public class GetInsightActivitiesViewModel
    {

        public GetInsightActivitiesViewModel()
        {
            coachCustomerChartCountViewModels = new List<CoachCustomerChartCountViewModel>();
        }
        public decimal individualCoachingIncome { get; set; }
        public int individualBooking { get; set; }
        public decimal groupCoachingIncome { get; set; }
        public int groupBooking { get; set; }
        public int newCustomers { get; set; }
        public int completedBooking { get; set; }
        public int cancelBooking { get; set; }
        public int newCustomerCount { get; set; }
        public List<CoachCustomerChartCountViewModel> coachCustomerChartCountViewModels { get; set; }

    }

    public class CoachCustomerListViewModel
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string profilePicture { get; set; }
    }
    public class CoachCustomerListCountViewModel
    {
        public Guid id { get; set; }
        public string day { get; set; }
        public string month { get; set; }
        public DateTime? createdDate { get; set; }
    }

    public class CoachCustomerChartCountViewModel
    {
        public string dayWeekMonth { get; set; }
        public int  customerCount{ get; set; }
    }

    public class CoachingConfirmation
    {
        public int BookingID { get; set; }
        public string ImageUrl { get; set; }
        public string ProfileName { get; set; }
        public ReviewType Type { get; set; }
        public string Status { get; set; }
        public decimal Experience { get; set; }
        public decimal FriendCount { get; set; }
        public int Age { get; set; }
        public string Location { get; set; }
        public string LocationLong { get; set; }
        public string LocationLat { get; set; }
        public Guid NationalityId { get; set; }
        public decimal Rating { get; set; }
        public CoachingDetail Detail { get; set; }
        public List<CoachingSpecialties> Specialties { get; set; }
        public List<CoachingBadges> Badges { get; set; }
        public string Description { get; set; }
        public Notation Notation { get; set; }
    }

    public class CoachingDetail
    {
        public Guid CoachId { get; set; }
        public EBookingStatus Status { get; set; }
        public string Date { get; set; }
        public string Slot { get; set; }
        public string Price { get; set; }
        public string Commision { get; set; }
        public string PriceHour { get; set; }
        public string TotalPrice { get; set; }
        public Guid BookingId { get; set; }
    }

    public class CoachingSpecialties
    {
        public string Icon { get; set; }
        public string Name { get; set; }
    }

    public class CoachingBadges
    {
        public string Icon { get; set; }
    }
}
