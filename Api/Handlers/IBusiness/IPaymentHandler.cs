using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface IPaymentHandler
    {
        Task<APIResponse<TelRCheckPaymentResponseViewModel>> CheckStatusPayment(string orderRef);
    }
}
