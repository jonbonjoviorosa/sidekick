using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Specialty
{
    [Table("Specialties")]
    public class Specialty : APIBaseModel
    {
        public Guid SpecialtyId { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [StringLength(200)]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}
