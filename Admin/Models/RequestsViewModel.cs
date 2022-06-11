using Sidekick.Model;
using System.Collections.Generic;

namespace Sidekick.Admin.Models
{
    public class RequestsViewModel
    {
        public IEnumerable<UserRequestViewModel> UserRequests { get; set; }
        public IEnumerable<CoachRequestViewModel> CoachRequests { get; set; }
        public IEnumerable<ReportDto> Reports { get; set; }
    }
}
