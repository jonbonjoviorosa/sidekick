using Sidekick.Model.Enums;
using System;

namespace Sidekick.Model.Chat
{
    public class ConversationViewModel
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public int SenderChatId { get; set; }
        public int ReceiverChatId { get; set; }
        public string Message { get; set; }
        public string UserImage { get; set; }
        public string UserName { get; set; }
        public string ReceiverUserImage { get; set; }
        public string ReceiverUserName { get; set; }
        public bool IsRead { get; set; }
        public int ConversationChatsId { get; set; }
        public MessageStatus Status { get; set; }
        public string CreatedMessageAt { get; set; }
    }
}
