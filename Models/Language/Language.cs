using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Language
{
    [Table("Languages")]
    public class Language: APIBaseModel
    {
        public Guid LanguageId { get; set; }
        
        [StringLength(250)]
        [Required]
        public string _Language { get; set; }

        public string Icon { get; set; }
    }
}
