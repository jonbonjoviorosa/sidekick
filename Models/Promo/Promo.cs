using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model.Promo
{
    public class Promo : APIBaseModel
    {
        public Guid PromoId { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public PromoType PromoType { get; set; }
        [Required]
        [RegularExpression(@"^[0-9]+\.?[0-9]*$", ErrorMessage = "Please enter a numeric value.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Please enter a numeric value.")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime StartsFrom { get; set; }
        [Required]
        public DateTime ValidTo { get; set; }
        public Guid CoachId { get; set; }
        public bool AllCoaches { get; set; }
        public string ByFacility { get; set; }
        public Guid FacilityId { get; set; }
        [Required]
        public byte EventType { get; set; }
    }
}
