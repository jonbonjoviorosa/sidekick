using System.Collections.Generic;

namespace Sidekick.Model
{
    public class PaymentViewModel
    {
        public List<PaymentFacilityPitches> PaymentFacilityPitches { get; set; }
        public List<PaymentCoachingViewModel> PaymentCoachings { get; set; }
    }
}
