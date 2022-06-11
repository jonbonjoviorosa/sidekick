using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.PaymentMethod
{
    [Table("PaymentMethod_Cards")]
    public class PaymentMethod_Card: APIBaseModel
    {
        public Guid PaymentMethod_CardId { get; set; }

        public Guid UserId { get; set; }

        [StringLength(1000)]
        [Required]
        public string FullName { get; set; }

        [StringLength(1000)]
        [Required]
        public string CardNumber { get; set; }

        [StringLength(1000)]
        [Required]
        public string ExpirationDate_Month { get; set; }

        [StringLength(1000)]
        [Required]
        public string ExpirationDate_Year { get; set; }

        [Required]
        [StringLength(1000)]
        public string CVV { get; set; }

        public string CardType { get; set; }

        [StringLength(1000)]
        public string PostCode { get; set; }
    }
}
