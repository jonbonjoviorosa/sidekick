using Sidekick.Model;
using Sidekick.Model.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IPaymentRepository
    {
        APIResponse InitiatePayment(string auth, PaymentCheckout payload);
        Task<Payment> GetPayment(Guid paymentId);
        Task<Payment> GetPaymentByBookingID(Guid bookingID);
        Task InsertUpdatePayment(Payment payment);
        Task<APIResponse> PaymentSummaries();
        Task<APIResponse> GetPaymentSummaries();
        Task<APIResponse> GetPlayPaymentHistory();
    }
}
