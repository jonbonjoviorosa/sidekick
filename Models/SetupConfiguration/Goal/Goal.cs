using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model.SetupConfiguration.Goals
{
    public class Goal : APIBaseModel
    {
        public Guid GoalId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }
    }
}
