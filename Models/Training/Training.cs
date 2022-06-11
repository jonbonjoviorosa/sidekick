using System;
using System.Collections.Generic;

namespace Sidekick.Model
{
    public class Training : APIBaseModel
    {
        public Guid CoachId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Duration { get; set; }
        public ETrainingType TrainingType { get; set; }
        //public virtual Coach Coach { get; set; }
    }

}
