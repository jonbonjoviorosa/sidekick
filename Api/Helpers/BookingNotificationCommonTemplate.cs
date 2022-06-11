using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public class BookingNotificationCommonTemplate
    {
        public int NotificationType { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }

        public Guid FacilityId { get; set; }
        public bool BookingConfirmed { get; set; }

        /// <summary>
        /// Individual or group class name
        /// </summary>
        public string Type { get; set; }
        public List<string> EmailTo { get; set; }
        public string UserName { get; set; }

        /// <summary>
        ///  Individual,Group Class = Name of the Class,Pitch Booking = Name of the Sport/Game
        /// </summary>
        public string Activity { get; set; }
        public string CoachName { get; set; }

        public string PlayerName { get; set; }

        public DateTime BookingDate { get; set; }
        public string BookingTime { get; set; }

        public string Location { get; set; }

        public decimal PriceCoaching { get; set; }

        public decimal TotalAmount { get; set; }
        

        public decimal ServiceFees { get; set; }

        public string Sport { get; set; }

        public string FacilityName { get; set; }

        public decimal PricePitch { get; set; }

        public decimal PricePerPlayer { get; set; }

        public string CaptainName { get; set; }

        public string PitchName { get; set; }

        public string Message { get; set; }
    }
}
