using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Class
{
    public class GroupClassParticipantsViewModel
    {
        public Guid ParticipantId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Image { get; set; }
    }
}
