using Microsoft.AspNetCore.Mvc.Rendering;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class ClassRenderViewModel
    {
        public Guid GroupClassId { get; set; }
        public List<SelectListItem> Levels { get; set; }
        public List<SelectListItem> Gyms { get; set; }
        public List<SelectListItem> Areas { get; set; }
        public List<SelectListItem> Coaches { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public Guid LevelId { get; set; }

        public int AreaId { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public Guid GymId { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public Guid CoachId { get; set; }

        [Required(ErrorMessage = "This is required!")]
        [StringLength(250)]
        public string Title { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public string TrainingType { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public DateTime Date { get; set; }

        public DateTime ScheduleFrom { get; set; }
        public DateTime ScheduleTo { get; set; }
        public bool IsRepeat { get; set; }
        public bool IsLocation { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public int Participants { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public decimal Price { get; set; }
        [StringLength(2000)]
        public string Description { get; set; }
        public decimal Commission { get; set; }
        public string CoachUserEmail { get; set; }
        public bool IsEnabled { get; set; }
        public string CoachName { get; set; }
        public List<FacilityPlayer> Players { get; set; }
        public string ImageUrl { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime CreatedDate { get; set; }

        [Required(ErrorMessage = "This is required!")]
        public int? Duration { get; set; }
    }
    
    public class TrainRenderViewModel
    {
        public List<ClassRenderViewModel> Classes { get; set; }
        public List<IndividualBookingViewModel> Bookings { get; set; }
    }

}
