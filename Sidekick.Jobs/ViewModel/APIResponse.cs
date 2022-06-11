using Sidekick.Jobs.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sidekick.Jobs.ViewModel
{

    public class APIResponse
    {
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public APIResponseCode? ResponseCode { get; set; }
        public object Payload { get; set; }
        public IEnumerable<KeyValuePair<string, string[]>> ModelError { get; set; }
    }

    public class APIResponse<T> where T : class
    {
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Status { get; set; }
        public APIResponseCode? ResponseCode { get; set; }
        public T Payload { get; set; }
        public IEnumerable<KeyValuePair<string, string[]>> ModelError { get; set; }
    }
}
