using Sidekick.Model.Enums;
using System;

namespace Sidekick.Model
{
    public  class CoachRequestViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
        public RequestStatus Status { get; set; }
    }
}
