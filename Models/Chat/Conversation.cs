using Sidekick.Model.Enums;
using System;

namespace Sidekick.Model.Chat
{
    public class Conversation : APIBaseModel
    {
        public int ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public int ConversationChatsId { get; set; }
        public string Message { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime CreatedMessageAt { get; set; }
    }

    public class Playconversations : APIBaseModel
    {
        public Guid BookingId { get; set; }
        public Guid SenderId { get; set; }
        public string Message { get; set; }
        public MessageStatus Status { get; set; }
        public DateTime CreatedMessageAt { get; set; }
    }

    public class ChatConversations : APIBaseModel
    {
        public int ConversationsId { get; set; }
        public Guid UserId { get; set; }
        public bool IsUnRead { get; set; }
    }

}
