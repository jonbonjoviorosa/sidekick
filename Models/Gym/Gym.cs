using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model.Gym
{
    [Table("Gyms")]
    public class Gym: APIBaseModel
    {
        public Guid GymId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(250)]
        public string GymName { get; set; }
        public string Icon { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(500)]
        public string GymAddress { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public float GymLat { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public float GymLong { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public int AreaId { get; set; }

    }
}
