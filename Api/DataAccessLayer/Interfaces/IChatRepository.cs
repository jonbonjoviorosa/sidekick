using Sidekick.Model;
using Sidekick.Model.Chat;
using System;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Interfaces
{
    public interface IChatRepository
    {
        Task<APIResponse> GetConversation(Guid receiverId);
        Task<APIResponse> SendMessage(ConversationViewModel convo);
        Task<APIResponse> SendMessageConversion(ConversationViewModel convo);
        Task<APIResponse> GetMessageList(Guid senderID);
        Task<APIResponse> GetConversionList();
        Task<APIResponse> GetUnreadCount();
    }
}
