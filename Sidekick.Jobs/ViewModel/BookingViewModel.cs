using Sidekick.Jobs.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Jobs.ViewModel
{
    public class BookingViewModel
    {
        public EBookingType BookingType { get; set; }
        public Guid BookingId { get; set; }
    }
}
