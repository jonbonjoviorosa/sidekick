using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model.SetupConfiguration.Level
{
    public class Level : APIBaseModel
    {
        public Guid LevelId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string Name { get; set; }
    }
}
