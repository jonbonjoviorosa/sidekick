using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class Banner : APIBaseModel
    {
        public Guid BannerId { get; set; }
        public Guid FacilityId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BannerDto
    {
        public Guid BannerId { get; set; }
        public Guid FacilityId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class BannerList
    {
        public Guid BannerId { get; set; }
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
    public class BannerStatus
    {
        public Guid BannerId { get; set; }
        public bool IsEnabled { get; set; }
        public Guid IsEnabledBy { get; set; }
    }
}
