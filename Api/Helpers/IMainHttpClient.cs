
using System.Threading.Tasks;

namespace Sidekick.Api.Helpers
{
    public interface IMainHttpClient
    {
        string PostHttpClientRequest(string _rEndPoint, object _content, int? _platform);
        string GetHttpClientRequest(string requestEndPoint);
        string TelrHttpClient(string url, object content);
        Task<string> SendSmsHttpClientRequestAsync(string _smsParameters);
        string PostAppleHttpClientRequest(string requestEndPoint, object content);
    }
}
