using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sidekick.Model
{
    [Table("UserTeams")]
    public class UserTeam: APIBaseModel
    {
        public Guid UserTeamId { get; set; }

        [ForeignKey("Fk_UserId")]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(250)]
        public string TeamName { get; set; }

        public User Fk_UserId { get; set; }
    }
}
