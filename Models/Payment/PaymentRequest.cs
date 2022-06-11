using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Model.Payment
{
    public class PaymentRequest
    {
        public string ivp_method { get; set; }
        public int ivp_store { get; set; }
        public long ivp_timestamp { get; set; }
        public string ivp_authkey { get; set; }
        public decimal ivp_amount { get; set; }
        public string ivp_currency { get; set; }
        public RequestType ivp_test { get; set; }
        public string ivp_cart { get; set; }
        public string ivp_desc { get; set; }
        public string return_auth { get; set; }
        public string return_decl { get; set; }
        public string return_can { get; set; }
        public string ivp_signature { get; set; }
        public string bill_title { get; set; }
        public string bill_fname { get; set; }
        public string bill_sname { get; set; }
        public string bill_addr1 { get; set; }
        public string bill_phone { get; set; }
        public string bill_city { get; set; }
        public string bill_region { get; set; }
        public string bill_country { get; set; }
        public string bill_zip { get; set; }
        public string bill_email { get; set; }
        public Guid bill_custref { get; set; }
        public string ivp_lang { get; set; }
        public string ivp_trantype { get; set; }
    }

    public class PaymentCheckout 
    {
        public string FacilityID { get; set; }
        public string CoachId { get; set; }
        public decimal CheckoutAmount { get; set; }
    }

    public class PaymentResponse
    {
        public string Method { get; set; }
        public PaymentResponseError Error { get; set; }
        public PaymentOrder Order { get; set; }
    }

    public class PaymentOrder
    {
        public string Ref { get; set; }
        public string Url { get; set; }
    }

    public class PaymentResponseError 
    {
        public string Message { get; set; }
        public string Note { get; set; }
    }

    public enum RequestType {
        Live = 0,
        Test = 1
    }
}
