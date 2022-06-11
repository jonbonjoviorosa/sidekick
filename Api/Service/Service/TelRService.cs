using Newtonsoft.Json;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.Helpers;
using Sidekick.Api.Service.IService;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sidekick.Api.Service.Service
{
    public class TelRService : ITelRService
    {
        private readonly APIConfigurationManager apiConfig;

        public TelRService(APIConfigurationManager apiConfig)
        {
            this.apiConfig = apiConfig;
        }

        public APIResponse<TelRResponseViewModel> Payment(TelRRequestViewModel request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiConfig.PaymentConfig.BaseUrl);
                client.DefaultRequestHeaders.ExpectContinue = false;

                var result = client.PostAsync("gateway/order.json",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                        new KeyValuePair<string, string>("ivp_method", Constants.IVP_METHOD_CREATE),
                        new KeyValuePair<string, string>("ivp_store", apiConfig.PaymentConfig.ClientId),
                        new KeyValuePair<string, string>("ivp_authkey", apiConfig.PaymentConfig.ApiKey),
                        new KeyValuePair<string, string>("ivp_signature", apiConfig.TokenKeys.Key),
                        new KeyValuePair<string, string>("ivp_cart", request.TransactionNo),
                        new KeyValuePair<string, string>("ivp_desc", Constants.IVP_COMPANYNAME),
                        new KeyValuePair<string, string>("ivp_test", Constants.IVP_TEST),
                        new KeyValuePair<string, string>("ivp_trantype",Constants.IVP_TYPE_AUTH),
                        new KeyValuePair<string, string>("ivp_tranclass",Constants.IVP_CLASS_ECOM),
                        new KeyValuePair<string, string>("ivp_amount", string.Format("{0:N2}", request.Amount)),
                        new KeyValuePair<string, string>("ivp_currency", Constants.IVP_CURRENCY),
                        new KeyValuePair<string, string>("return_auth", apiConfig.PaymentConfig.ReturnAuthURL), // "Success Page"
                        new KeyValuePair<string, string>("return_can", apiConfig.PaymentConfig.ReturnCancelledURL), //"Cancel Page"
                        new KeyValuePair<string, string>("return_decl", apiConfig.PaymentConfig.ReturnDeclinedURL), //"Decline Page"

                        //new KeyValuePair<string, string>("bill_title", Constants.IVP_TITLE),
                        new KeyValuePair<string, string>("bill_fname", request.FirstName),
                        new KeyValuePair<string, string>("bill_sname", request.LastName),
                        new KeyValuePair<string, string>("bill_addr1", request.Address),
                        new KeyValuePair<string, string>("bill_city", request.City),
                        new KeyValuePair<string, string>("bill_region", request.Region),
                        new KeyValuePair<string, string>("bill_country", request.CountryCode),
                        new KeyValuePair<string, string>("bill_zip", request.Zip),
                        new KeyValuePair<string, string>("bill_email", request.Email),
                        //new KeyValuePair<string, string>("bill_custref", request.UserId.ToString()),
                        new KeyValuePair<string, string>("ivp_update_url", "www.urdomain.com"),
                        new KeyValuePair<string, string>("ivp_framed", Constants.IVP_FRAMED),

                })).Result;

                if (result.IsSuccessStatusCode)
                {
                    string data = (result.Content.ReadAsStringAsync().Result);
                    var response = JsonConvert.DeserializeObject<TelRResponseViewModel>(data);
                    response.TransactionNo = request.TransactionNo;
                    return new APIResponse<TelRResponseViewModel>
                    {
                        Message = Messages.SuccessInitPayment,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = response
                    };
                }
                else
                {
                    return APIResponseHelper<TelRResponseViewModel>.ReturnAPIResponse(EResponseAction.Error);
                }
            }
        }

        public APIResponse<TelRCheckPaymentResponseViewModel> CheckPayment(string telRRefNo)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiConfig.PaymentConfig.BaseUrl);
                client.DefaultRequestHeaders.ExpectContinue = false;

                var result = client.PostAsync("gateway/order.json",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                        new KeyValuePair<string, string>("ivp_method", "check"),
                        new KeyValuePair<string, string>("ivp_store", apiConfig.PaymentConfig.ClientId),
                        new KeyValuePair<string, string>("ivp_authkey", apiConfig.PaymentConfig.ApiKey),
                        new KeyValuePair<string, string>("ivp_signature", apiConfig.TokenKeys.Key),
                        new KeyValuePair<string, string>("order_ref", telRRefNo),

                })).Result;

                if (result.IsSuccessStatusCode)
                {
                    string data = (result.Content.ReadAsStringAsync().Result);
                    var response = JsonConvert.DeserializeObject<TelRCheckPaymentResponseViewModel>(data);
                    return new APIResponse<TelRCheckPaymentResponseViewModel>
                    {
                        Message = Messages.SuccessInitPayment,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = response
                    };
                }
                else
                {
                    return APIResponseHelper<TelRCheckPaymentResponseViewModel>.ReturnAPIResponse(EResponseAction.Error);
                }
            }
        }

        public APIResponse<TelRPaymentReponseViewModel> CapturePayment(string telRRefNo,
            string transactionNo,
            decimal amount)
        {
            return ActionPayment(Constants.IVP_TYPE_CAPTURE, telRRefNo, transactionNo, amount);
        }

        public APIResponse<TelRPaymentReponseViewModel> AuthPayment(string telRRefNo,
            string transactionNo,
            decimal amount)
        {
            return ActionPayment(Constants.IVP_TYPE_AUTH, telRRefNo, transactionNo, amount);
        }


        public APIResponse<TelRPaymentReponseViewModel> CancelPayment(string telRRefNo,
            string transactionNo,
            decimal amount)
        {
            return ActionPaymentCancel(Constants.IVP_TYPE_CANCEL, telRRefNo, transactionNo, amount);
        }

        public APIResponse<TelRPaymentReponseViewModel> RefundPayment(string telRRefNo, 
            string transactionNo, 
            decimal amount)
        {
            return ActionPayment(Constants.IVP_TYPE_REFUND, telRRefNo, transactionNo, amount);
        }

        private APIResponse<TelRPaymentReponseViewModel>  ActionPayment(string action,
            string telRRefNo,
            string transactionNo,
            decimal amount)
        {
            string newTelRefNo = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{apiConfig.PaymentConfig.BaseUrl}");
                var xml = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <remote>
                             <store>{apiConfig.PaymentConfig.ClientId}</store>
                             <key>{apiConfig.PaymentConfig.RemoteKey}</key>
                             <tran>
                              <type>{action}</type>
                              <class>{Constants.IVP_CLASS_CONT}</class>
                              <cartid>{transactionNo}</cartid>
                              <description>{Constants.IVP_COMPANYNAME}</description>
                              <test>{Constants.IVP_TRAN_TEST}</test>
                              <currency>{Constants.IVP_CURRENCY}</currency>
                              <amount>{amount.ToString()}</amount>
                              <ref>{telRRefNo}</ref>
                             </tran>
                            </remote>";
                var content = new StringContent(xml, Encoding.UTF8, "application/xml");
                var result = client.PostAsync("gateway/remote.xml", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    string data = (result.Content.ReadAsStringAsync().Result);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(data);

                    var isPaymentSuccess = (data.Contains("<message>Authorised</message>") && data.Contains("<status>A</status>")) ? true : false;
                    if (isPaymentSuccess)
                    {
                        newTelRefNo = xmlDoc.ChildNodes[1].ChildNodes[0].ChildNodes[3].InnerText;
                    }

                    var isSuccess = new TelRPaymentReponseViewModel()
                    {
                        IsSuccess = isPaymentSuccess,
                        TelRefNo = newTelRefNo
                    };
                    return new APIResponse<TelRPaymentReponseViewModel>
                    {
                        Message = Messages.Success,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = isSuccess
                    };
                }
                else
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Error);
                }
            }
        }

        private APIResponse<TelRPaymentReponseViewModel> ActionPaymentCancel(string action,
            string telRRefNo,
            string transactionNo,
            decimal amount)
        {
            string newTelRefNo = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{apiConfig.PaymentConfig.BaseUrl}");
                var xml = @$"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <remote>
                             <store>{apiConfig.PaymentConfig.ClientId}</store>
                             <key>{apiConfig.PaymentConfig.RemoteKey}</key>
                             <tran>
                              <type>{action}</type>
                              <class>{Constants.IVP_CLASS_ECOM}</class>
                              <cartid>{transactionNo}</cartid>
                              <description>{Constants.IVP_COMPANYNAME}</description>
                              <test>{Constants.IVP_TRAN_TEST}</test>
                              <currency>{Constants.IVP_CURRENCY}</currency>
                              <amount>{amount.ToString()}</amount>
                              <ref>{telRRefNo}</ref>
                             </tran>
                            </remote>";
                var content = new StringContent(xml, Encoding.UTF8, "application/xml");
                var result = client.PostAsync("gateway/remote.xml", content).Result;

                if (result.IsSuccessStatusCode)
                {
                    string data = (result.Content.ReadAsStringAsync().Result);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(data);

                    var isPaymentSuccess = (data.Contains("<message>Processed</message>") && data.Contains("<status>A</status>")) ? true : false;
                    if (isPaymentSuccess)
                    {
                        newTelRefNo = xmlDoc.ChildNodes[1].ChildNodes[0].ChildNodes[3].InnerText;
                    }

                    var isSuccess = new TelRPaymentReponseViewModel()
                    {
                        IsSuccess = isPaymentSuccess,
                        TelRefNo = newTelRefNo
                    };
                    return new APIResponse<TelRPaymentReponseViewModel>
                    {
                        Message = Messages.Success,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = isSuccess
                    };
                }
                else
                {
                    return APIResponseHelper<TelRPaymentReponseViewModel>.ReturnAPIResponse(EResponseAction.Error);
                }
            }
        }
    }
}
