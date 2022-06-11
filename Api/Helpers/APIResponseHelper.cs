using Sidekick.Api.Configurations.Resources;
using Sidekick.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public static class APIResponseHelper<T> where T: class
    {
        public static APIResponse<T> ReturnAPIResponse(EResponseAction action)
        {
            switch (action)
            {
                case EResponseAction.Unauthorized:
                    return new APIResponse<T>()
                    {
                        Message = Messages.Unauthorized,
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                case EResponseAction.InternalServerError:
                    return new APIResponse<T>()
                    {
                        Message = Messages.SomethingWentWrong,
                        Status = Status.InternalServerError,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.SendEmailSuccess:
                    return new APIResponse<T>()
                    {
                        Message = Messages.SendEmailSuccess,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.SendEmailFailed:
                    return new APIResponse<T>()
                    {
                        Message = Messages.SendEmailFailed,
                        Status = Status.InternalServerError,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.NotExist:
                    return new APIResponse<T>()
                    {
                        Message = Messages.CannotFindRelatedInformation,
                        Status = Status.InternalServerError,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.UpdateSuccess:
                    return new APIResponse<T>()
                    {
                        Status = Status.Success,
                        Message = Messages.UpdateSuccess,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.InsertSuccess:
                    return new APIResponse<T>()
                    {
                        Status = Status.Success,
                        Message = Messages.InsertSuccess,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.RecordSuccess:
                    return new APIResponse<T>()
                    {
                        Status = Status.Success,
                        Message = Messages.RecordedSuccess,
                        StatusCode = System.Net.HttpStatusCode.OK,
                    };
                case EResponseAction.PaymentMethod_CardExpired:
                    return new APIResponse<T>()
                    {
                        Status = Status.Failed,
                        Message = Messages.PaymentMethod_CardExpired,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.NoTelRTranAttached:
                    return new APIResponse<T>()
                    {
                        Status = Status.Failed,
                        Message = Messages.NoTelRTranAttached,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.Error:
                    return new APIResponse<T>()
                    {
                        Status = Status.Failed,
                        Message = Messages.Error,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.PaymentAlreadyDone:
                    return new APIResponse<T>()
                    {
                        Status = Status.Failed,
                        Message = Messages.PaymentAlreadyDone,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
            }
            return new APIResponse<T>();
        }
    }

    public static class APIResponseHelper
    {
        public static APIResponse ReturnAPIResponse(EResponseAction action)
        {
            switch (action)
            {
                case EResponseAction.Unauthorized:
                    return new APIResponse()
                    {
                        Message = Messages.Unauthorized,
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                case EResponseAction.InternalServerError:
                    return new APIResponse()
                    {
                        Message = Messages.SomethingWentWrong,
                        Status = Status.InternalServerError,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.SendEmailSuccess:
                    return new APIResponse()
                    {
                        Message = Messages.SendEmailSuccess,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.SendEmailFailed:
                    return new APIResponse()
                    {
                        Message = Messages.SendEmailFailed,
                        Status = Status.InternalServerError,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.DeleteSuccess:
                    return new APIResponse()
                    {
                        Message = Messages.DeleteSuccess,
                        Status = Status.Success,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.NotExist:
                    return new APIResponse()
                    {
                        Message = Messages.CannotFindRelatedInformation,
                        Status = Status.InternalServerError,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.UpdateSuccess:
                    return new APIResponse
                    {
                        Status = Status.Success,
                        Message = Messages.UpdateSuccess,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.InsertSuccess:
                    return new APIResponse
                    {
                        Status = Status.Success,
                        Message = Messages.InsertSuccess,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.RecordSuccess:
                    return new APIResponse
                    {
                        Status = Status.Success,
                        Message = Messages.RecordedSuccess,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                case EResponseAction.NoTelRTranAttached:
                    return new APIResponse
                    {
                        Status = Status.Failed,
                        Message = Messages.NoTelRTranAttached,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.PaymentMethod_CardExpired:
                    return new APIResponse()
                    {
                        Status = Status.Failed,
                        Message = Messages.PaymentMethod_CardExpired,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.Error:
                    return new APIResponse()
                    {
                        Status = Status.Failed,
                        Message = Messages.Error,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                case EResponseAction.PaymentNotDone:
                    return new APIResponse()
                    {
                        Status = Status.Failed,
                        Message = Messages.PaymentNotDone,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
            }
            return new APIResponse();
        }
    }
}
