using Sidekick.Api.ViewModel;

namespace Sidekick.Api.Helpers.IHelpers
{
    public interface ISendEmailHelper
    {
        void SendEmail(EmailViewModel email);
    }
}
