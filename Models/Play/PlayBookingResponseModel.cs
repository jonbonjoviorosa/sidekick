using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Play
{
    public class MyPlayBookingResponseModel
    {
        public MyPlayBookingResponseModel()
        {
            UpComingBooking = new List<PlayBookingResponseModel>();
            BookingHistory = new List<PlayBookingResponseModel>();
        }
       public List<PlayBookingResponseModel> UpComingBooking { get; set; }
       public List<PlayBookingResponseModel> BookingHistory { get; set; }
    }
    public class PlayBookingResponseModel
    {
        public Guid BookingId { get; set; }
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }
        public DateTime PitchStart { get; set; }
        public DateTime PitchEnd { get; set; }
        public string Date { get; set; }
        
        public decimal PricePerPlayer { get; set; }
        public decimal TotalPrice { get; set; }
        public string SportName { get; set; }
        public string SportImageUrl { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsPaid { get; set; }
    }
}
