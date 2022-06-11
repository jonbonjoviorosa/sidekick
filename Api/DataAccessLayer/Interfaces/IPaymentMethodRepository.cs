using Sidekick.Model.PaymentMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod_Card> GetPaymentMethodCard(Guid paymentMethodCardId);
        Task<PaymentMethod_Card> GetPaymentMethodCardForUser(Guid paymentMethodCardId);
        Task<IEnumerable<PaymentMethod_Card>> GetPaymentMethodCards(Guid userId);
        Task InsertUpdatePaymentMethodCard(PaymentMethod_Card paymentMethod);
        Task RemovePaymentMethodCard(Guid paymentMethodCardId);
    }
}
