using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Play
{
   public class FacilitySendContactMessageToPlayerRequestModel
    {
        public Guid BookingId { get; set; }

        public string Message { get; set; }

        
    }
}
