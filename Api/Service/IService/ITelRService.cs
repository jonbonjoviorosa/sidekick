using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Service.IService
{
    public interface ITelRService
    {
        APIResponse<TelRResponseViewModel> Payment(TelRRequestViewModel request);
        APIResponse<TelRCheckPaymentResponseViewModel> CheckPayment(string telRRefNo);

        APIResponse<TelRPaymentReponseViewModel> CapturePayment(string telRRefNo, string transactionNo, decimal amount);

        APIResponse<TelRPaymentReponseViewModel> AuthPayment(string telRRefNo, string transactionNo, decimal amount);
        APIResponse<TelRPaymentReponseViewModel> CancelPayment(string telRRefNo, string transactionNo, decimal amount);
        APIResponse<TelRPaymentReponseViewModel> RefundPayment(string telRRefNo,
            string transactionNo,
            decimal amount);
    }
}
