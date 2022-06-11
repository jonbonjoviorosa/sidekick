using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class Surface : APIBaseModel
    {    
        public Guid SurfaceId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }
    }
}
