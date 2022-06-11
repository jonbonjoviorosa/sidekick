using System;
using System.ComponentModel.DataAnnotations;

namespace Sidekick.Model
{
    public class FacilityUserType : APIBaseModel
    {
        public Guid FacilityRoleId { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string FacilityRoleName { get; set; }
    }
}
