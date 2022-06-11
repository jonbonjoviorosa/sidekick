using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    [Table("UserTrainBadges")]
    public class UserTrainBadge: APIBaseModel
    {
        public Guid UserId { get; set; }

        public Guid SpecialtyId { get; set; }
    }
}
