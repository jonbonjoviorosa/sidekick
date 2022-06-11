using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Sidekick.Model
{
    public class APIResponse
    {
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        private object _payload;
        public APIResponseCode? ResponseCode { get; set; }
        public object Payload
        {
            set
            {
                if (value == null)
                {
                    this._payload = value;
                }
                else if (value.GetType() == typeof(UserContext))
                {
                    UserContext userctxt = value as UserContext;
                    if (userctxt != null)
                    {
                        userctxt.UserInfo.Password = "";
                        this._payload = userctxt as object;
                    }
                }
                else if (value.GetType() == typeof(User))
                {
                    User userInfo = value as User;
                    if (userInfo != null)
                    {
                        userInfo.Password = "";
                        this._payload = userInfo as object;
                    }
                    this._payload = value;
                }
                else if (value.GetType() == typeof(AdminUserContext))
                {
                    AdminUserContext adminContext = value as AdminUserContext;
                    if (adminContext != null)
                    {
                        adminContext.AdminInfo.Password = "";
                        this._payload = adminContext as object;
                    }
                    this._payload = value;
                }
                else
                {
                    this._payload = value;
                }
            }
            get { return this._payload; }
        }
        public IEnumerable<KeyValuePair<string, string[]>> ModelError { get; set; }
    }

    public class APIResponse<T> where T:  class
    {
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public APIResponseCode? ResponseCode { get; set; }
        public T Payload { get; set; }
        public IEnumerable<KeyValuePair<string, string[]>> ModelError { get; set; }
    }

    public static class ModelStateHelper
    {
        public static IEnumerable<KeyValuePair<string, string[]>> Errors(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                return modelState
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray())
                    .Where(m => m.Value.Any());
            }

            return null;
        }
    }
}
