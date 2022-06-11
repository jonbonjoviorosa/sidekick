using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Nationality
{
    [Table("Nationalities")]
    public class Nationality: APIBaseModel
    {
        public Guid NationalityId { get; set; }

        [StringLength(200)]
        [Required]
        public string _Nationality { get; set; }
    }
}
