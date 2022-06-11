using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Payment
{
    public class PaymentRequestViewModel
    {
        public Guid PaymentId { get; set; }
        public int BookingType { get; set; }
        public decimal Amount { get; set; }
        public string TelRRefNo { get; set; }
        public string TransactionNo { get; set; }
        public decimal SideKickCommission { get; set; }
        public DateTime? DatePaid { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
