using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class Sport : APIBaseModel
    {
        public Guid SportId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public int MaxPlayers { get; set; }
        public decimal MaxPrice { get; set; }
    }

    public class SportDto
    {
        public Guid SportId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Icon { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public int MaxPlayers { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public decimal MaxPrice { get; set; }
        public bool? IsEnabled { get; set; }
    }
}
