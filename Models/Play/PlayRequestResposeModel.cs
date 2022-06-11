using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Play
{
    public class PlayRequestResposeModel
    {
        public Guid BookingId { get; set; }
        public Guid SportId { get; set; }
        public Guid UserId { get; set; }

        public string UserImage { get; set; }
        public string UserName { get; set; }

        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityImage { get; set; }
        public string Location { get; set; }

        public string SurfaceName { get; set; }
        public string TeamSizeName { get; set; }
        public string SportName { get; set; }

        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }

        public decimal PricePerPlayer { get; set; }

        public DateTime Date { get; set; }

        public string PitchName { get; set; }

        public decimal PriceIncludingVat { get; set; }

        public decimal SideKickCommission { get; set; }
    }
}
