using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model
{
    [Table("UserGoals")]
    public class UserGoal: APIBaseModel
    {
        public Guid UserGoalId { get; set; }
        public Guid UserId { get; set; }

        public Guid GoalId { get; set; }
    }
}
