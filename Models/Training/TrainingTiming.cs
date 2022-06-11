using System;

namespace Sidekick.Model
{
    public class TrainingTiming : APIBaseModel
    {
        public int TrainingTimingId { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public decimal CustomPrice { get; set; }

        //public virtual Training Training { get; set; }
    }

}
