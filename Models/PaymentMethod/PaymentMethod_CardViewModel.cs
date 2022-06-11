using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.PaymentMethod
{
    public class PaymentMethod_CardViewModel
    {
        public Guid? PaymentMethod_CardId { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(300, ErrorMessage = "The maximum characters is {0}!")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        //[StringLength(16, MinimumLength = 16, ErrorMessage = "It should be {1} characters long!")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "It should be {1} characters long!")]
        public string ExpirationDate_Month { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "It should be {1} characters long!")]
        public string ExpirationDate_Year { get; set; }

        //[Required(ErrorMessage = "{0} is required!")]
        //[StringLength(3, MinimumLength = 3, ErrorMessage = "It should be {1} characters long!")]
        public string CVV { get; set; }

        public string CardType { get; set; }

        public string TelRRefNo { get; set; }

        public string TransactionNo { get; set; }

    }
}
