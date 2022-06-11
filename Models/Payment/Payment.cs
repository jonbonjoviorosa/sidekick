using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Sidekick.Model.Payment
{
    [Table("Payments")]
    public class Payment : APIBaseModel
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public int BookingType { get; set; }
        public decimal Amount { get; set; }

        [StringLength(200)]
        public string TelRRefNo { get; set; }

        [StringLength(50)]
        public string TransactionNo { get; set; }

        public decimal SideKickCommission { get; set; }

        public DateTime? DatePaid { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
