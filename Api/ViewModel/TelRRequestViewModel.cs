using Sidekick.Model;
using Sidekick.Model.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.ViewModel
{
    public class TelRRequestViewModel
    {
        public string TransactionNo { get; set; }

        public decimal Amount { get; set; }

        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string CountryCode { get; set; }

        public string Zip { get; set; }

        public string Email { get; set; }



    }
}
