using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Play
{
    public class GamePlayerViewModel
    {
        public Guid? GameId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? FacilityPitchId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }
        public Guid UserFriendId { get; set; }
        public bool HasAccepted { get; set; }
        public bool? IsCaptain { get; set; }
        public string PlayerName { get; set; }
    }
}
