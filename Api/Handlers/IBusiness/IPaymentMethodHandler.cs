using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.PaymentMethod;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IPaymentMethodHandler
    {
        Task<APIResponse<IEnumerable<PaymentMethod_CardViewModel>>> GetPaymentMethodCards();
        Task<APIResponse<PaymentMethod_CardViewModel>> GetPaymentMethodCard(Guid cardId);
        Task<APIResponse> InsertUpdatePaymentMethodCard(PaymentMethod_CardViewModel card);
        Task<APIResponse<TelRResponseViewModel>> InitiatePaymentMethod();
    }
}
