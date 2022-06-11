using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    public class FacilityDashboardViewModel
    {
        public int PitchBookedToday { get; set; }
        public string PitchName { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
