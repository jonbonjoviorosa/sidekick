using Sidekick.Model.Notification;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetNotification();
        Task InsertUpdateNotification(Notification notification);
    }
}
