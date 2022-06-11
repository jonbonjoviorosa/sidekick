using Sidekick.Api.Handlers.Business;
using Sidekick.Model;
using Sidekick.Model.Notification;
using System.Threading.Tasks;

namespace Sidekick.Api.Handlers.IBusiness
{
    public interface INotificationHandler
    {
        Task<APIResponse<NotificationViewModel>> GetNotifcation();
        Task<APIResponse> InsertUpdateNotification(NotificationViewModel notification);
    }
}
