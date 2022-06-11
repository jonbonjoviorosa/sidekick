using Microsoft.EntityFrameworkCore;
using Sidekick.Api.Configurations.Resources;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.Helpers.IHelpers;
using Sidekick.Model;
using Sidekick.Model.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class ChatRepository : APIBaseRepo, IChatRepository
    {
        readonly APIDBContext _dbContext;
        private readonly IUserHelper _userHelper;
        ILoggerManager _loggerManager { get; }

        public ChatRepository(APIDBContext dbContext,
            IUserHelper userHelper,
            ILoggerManager loggerManager)
        {
            _dbContext = dbContext;
            _userHelper = userHelper;
            _loggerManager = loggerManager;
        }

        public async Task<APIResponse> GetConversation(Guid receiverId)
        {
            var apiResp = new APIResponse();

            _loggerManager.LogInfo("-- Run::ChatRepository::GetConversation --");
            _loggerManager.LogDebugObject(receiverId);

            try
            {
                var currentUser = _userHelper.GetCurrentUserGuidLogin();
                if (currentUser != Guid.Empty)
                {
                    var conversations = (from conversation in _dbContext.Playconversations
                                         join profile in _dbContext.Users
                                         on conversation.SenderId equals profile.UserId
                                         where conversation.BookingId == receiverId
                                         orderby conversation.CreatedMessageAt
                                         select new ConversationViewModel
                                         {
                                             Message = conversation.Message,
                                             Status = conversation.Status,
                                             UserImage = profile.ImageUrl,
                                             SenderId = conversation.SenderId,
                                             ReceiverId = receiverId,
                                             UserName = profile.FirstName + " " + profile.LastName,
                                             CreatedMessageAt = Helper.DisplayDateTime(conversation.CreatedMessageAt)
                                         }).ToList();

                    return new APIResponse
                    {
                        Message = "Message Retrieved!",
                        Status = Status.Success,
                        Payload = conversations,
                        StatusCode = System.Net.HttpStatusCode.OK,
                    };


                }

                return new APIResponse
                {
                    Message = "Current User not Exists!",
                    Status = Status.Failed,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogInfo("-- Error::ChatRepository::GetConversation --");
                _loggerManager.LogError(ex.InnerException.Message);
                _loggerManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> SendMessage(ConversationViewModel convo)
        {
            var apiResp = new APIResponse();

            _loggerManager.LogInfo("-- Run::ChatRepository::SendMessage --");
            _loggerManager.LogDebugObject(convo);

            try
            {
                var currentUser = _userHelper.GetCurrentUserGuidLogin();
                if (currentUser == Guid.Empty)
                {
                    return new APIResponse
                    {
                        Message = "Current User not Exists!",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    };
                }

                int receiverUser = _dbContext.Users.Where(u => u.UserId == convo.ReceiverId).FirstOrDefault().Id;
                int loggedinUser = _dbContext.Users.Where(u => u.UserId == currentUser).FirstOrDefault().Id;
                string roomName = string.Empty;
                if (loggedinUser > receiverUser)
                    roomName = receiverUser.ToString() + loggedinUser.ToString();
                else
                    roomName = loggedinUser.ToString() + receiverUser.ToString();

                var conversation = new Conversation
                {
                    SenderId = currentUser,
                    ReceiverId = convo.ReceiverId,
                    Message = convo.Message,
                    CreatedMessageAt = DateTime.UtcNow,
                    Status = Model.Enums.MessageStatus.Sent,
                    ConversationChatsId = Convert.ToInt32(roomName),
                    LastEditedBy = currentUser,
                    LastEditedDate = DateTime.Now,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.Now,
                    IsEnabled = true,
                    IsEnabledBy = currentUser,
                    DateEnabled = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now,
                };

                _dbContext.Conversations.Add(conversation);
                await _dbContext.SaveChangesAsync();


                var chatconversion = _dbContext.ChatConversations.Where(c => c.ConversationsId == Convert.ToInt32(roomName) && c.UserId == currentUser).FirstOrDefault();
                if (chatconversion == null)
                {
                    var chatconversation = new ChatConversations
                    {
                        UserId = currentUser,
                        IsUnRead = true,
                        ConversationsId = Convert.ToInt32(roomName),
                        LastEditedBy = currentUser,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsEnabledBy = currentUser,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                    };
                    _dbContext.ChatConversations.Add(chatconversation);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    chatconversion.IsUnRead = false;
                    _dbContext.ChatConversations.Update(chatconversion);
                    await _dbContext.SaveChangesAsync();
                }

                var chatconversionReceiver = _dbContext.ChatConversations.Where(c => c.ConversationsId == Convert.ToInt32(roomName) && c.UserId == convo.ReceiverId).FirstOrDefault();
                if (chatconversionReceiver == null)
                {
                    var chatconversationReceiver = new ChatConversations
                    {
                        UserId = convo.ReceiverId,
                        IsUnRead = false,
                        ConversationsId = Convert.ToInt32(roomName),
                        LastEditedBy = currentUser,
                        LastEditedDate = DateTime.Now,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.Now,
                        IsEnabled = true,
                        IsEnabledBy = currentUser,
                        DateEnabled = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                    };
                    _dbContext.ChatConversations.Add(chatconversationReceiver);
                    await _dbContext.SaveChangesAsync();
                }



                return new APIResponse
                {
                    Message = $"Message Sent to {conversation.ReceiverId}",
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogInfo("-- Error::ChatRepository::SendMessage --");
                _loggerManager.LogError(ex.InnerException.Message);
                _loggerManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> SendMessageConversion(ConversationViewModel convo)
        {
            var apiResp = new APIResponse();

            _loggerManager.LogInfo("-- Run::ChatRepository::SendMessage --");
            _loggerManager.LogDebugObject(convo);

            try
            {
                var currentUser = _userHelper.GetCurrentUserGuidLogin();
                if (currentUser == Guid.Empty)
                {
                    return new APIResponse
                    {
                        Message = "Current User not Exists!",
                        Status = Status.Failed,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                    };
                }

                var conversation = new Playconversations
                {
                    SenderId = currentUser,
                    BookingId = convo.ReceiverId,
                    Message = convo.Message,
                    CreatedMessageAt = DateTime.UtcNow,
                    Status = Model.Enums.MessageStatus.Sent,

                    LastEditedBy = currentUser,
                    LastEditedDate = DateTime.Now,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.Now,
                    IsEnabled = true,
                    IsEnabledBy = currentUser,
                    DateEnabled = DateTime.Now,
                    IsLocked = false,
                    LockedDateTime = DateTime.Now,
                };

                _dbContext.Playconversations.Add(conversation);
                await _dbContext.SaveChangesAsync();

                return new APIResponse
                {
                    Message = $"Message Sent to {conversation.BookingId}",
                    Status = Status.Success,
                    StatusCode = System.Net.HttpStatusCode.OK,
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogInfo("-- Error::ChatRepository::SendMessage --");
                _loggerManager.LogError(ex.InnerException.Message);
                _loggerManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetMessageList(Guid senderID)
        {
            var apiResp = new APIResponse();
            _loggerManager.LogInfo("-- Run::ChatRepository::GetMessageList --");

            try
            {
                var currentUser = _userHelper.GetCurrentUserGuidLogin();
                int receiverUser = _dbContext.Users.Where(u => u.UserId == senderID).FirstOrDefault().Id;
                int loggedinUser = _dbContext.Users.Where(u => u.UserId == currentUser).FirstOrDefault().Id;

                if (currentUser != Guid.Empty)
                {
                    var conversations = (from conversation in _dbContext.Conversations
                                         join profile in _dbContext.Users
                                         on conversation.SenderId equals profile.UserId
                                         join profileReceiver in _dbContext.Users
                                         on conversation.ReceiverId equals profileReceiver.UserId
                                         where (conversation.SenderId == senderID && conversation.ReceiverId == currentUser) || (conversation.SenderId == currentUser && conversation.ReceiverId == senderID)
                                         orderby conversation.CreatedMessageAt
                                         select new ConversationViewModel
                                         {
                                             Message = conversation.Message,
                                             SenderId = conversation.SenderId,
                                             Status = conversation.Status,
                                             UserImage = profile.ImageUrl,
                                             SenderChatId = profile.Id,
                                             ReceiverChatId = profileReceiver.Id,
                                             ReceiverId = conversation.ReceiverId,
                                             UserName = profile.FirstName + " " + profile.LastName,
                                             ReceiverUserImage = profileReceiver.ImageUrl,
                                             ReceiverUserName = profileReceiver.FirstName + " " + profileReceiver.LastName,
                                             CreatedMessageAt = Helper.DisplayDateTime(conversation.CreatedMessageAt)
                                         }).ToList();


                    string roomName = string.Empty;
                    if (receiverUser > loggedinUser)
                        roomName = loggedinUser.ToString() + receiverUser.ToString();
                    else
                        roomName = receiverUser.ToString() + loggedinUser.ToString();

                    var chatconversion = _dbContext.ChatConversations.Where(c => c.ConversationsId == Convert.ToInt32(roomName) && c.UserId == currentUser).FirstOrDefault();
                    if (chatconversion == null)
                    {
                        var chatconversation = new ChatConversations
                        {
                            UserId = currentUser,
                            IsUnRead = true,
                            ConversationsId = Convert.ToInt32(roomName),
                            LastEditedBy = currentUser,
                            LastEditedDate = DateTime.Now,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = currentUser,
                            DateEnabled = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                        };
                        _dbContext.ChatConversations.Add(chatconversation);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        chatconversion.IsUnRead = true;
                        _dbContext.ChatConversations.Update(chatconversion);
                        await _dbContext.SaveChangesAsync();
                    }


                    return new APIResponse
                    {
                        Message = "Message List!",
                        Status = Status.Success,
                        Payload = conversations,
                        StatusCode = System.Net.HttpStatusCode.OK,
                    };




                }

                return new APIResponse
                {
                    Message = "Current User not Exists!",
                    Status = Status.Failed,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogInfo("-- Error::ChatRepository::GetConversation --");
                _loggerManager.LogError(ex.InnerException.Message);
                _loggerManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public async Task<APIResponse> GetConversionList()
        {
            var apiResp = new APIResponse();
            _loggerManager.LogInfo("-- Run::ChatRepository::GetMessageList --");

            try
            {
                var currentUser = _userHelper.GetCurrentUserGuidLogin();
                if (currentUser != Guid.Empty)
                {
                    var conversation = _dbContext.Conversations.Where(c => c.ReceiverId == currentUser || c.SenderId == currentUser)
                                                                      .OrderByDescending(c => c.CreatedMessageAt)
                                                                      .ToList()
                                                                      .GroupBy(c => c.ConversationChatsId)
                                                                      .Select(c => c.FirstOrDefault());


                    var conversionSid = conversation.Select(c => c.Id).ToList();

                    var conversations = (from conversationdb in _dbContext.Conversations
                                         join profile in _dbContext.Users
                                         on conversationdb.SenderId equals profile.UserId
                                         join profileReceiver in _dbContext.Users
                                         on conversationdb.ReceiverId equals profileReceiver.UserId
                                         where conversionSid.Contains(conversationdb.Id)
                                         select new ConversationViewModel
                                         {
                                             Message = conversationdb.Message,
                                             Status = conversationdb.Status,
                                             SenderChatId = profile.Id,
                                             UserImage = profile.ImageUrl,
                                             ReceiverId = conversationdb.ReceiverId,
                                             SenderId = conversationdb.SenderId,
                                             IsRead = false,
                                             ConversationChatsId = conversationdb.ConversationChatsId,
                                             UserName = profile.FirstName + " " + profile.LastName,
                                             ReceiverChatId = profileReceiver.Id,
                                             ReceiverUserImage = profileReceiver.ImageUrl,
                                             ReceiverUserName = profileReceiver.FirstName + " " + profileReceiver.LastName,
                                             CreatedMessageAt = Helper.DisplayDateTime(conversationdb.CreatedMessageAt)
                                         }).ToList();


                    var listConversions = new List<ConversationViewModel>();
                    foreach (var itemConversion in conversations)
                    {
                        var listConversion = new ConversationViewModel();
                        listConversion.Message = itemConversion.Message;
                        listConversion.Status = itemConversion.Status;
                        listConversion.SenderChatId = itemConversion.SenderChatId;
                        listConversion.UserImage = itemConversion.UserImage;
                        listConversion.ReceiverId = itemConversion.ReceiverId;
                        listConversion.SenderId = itemConversion.SenderId;
                        listConversion.IsRead = getReadStatus(itemConversion.ConversationChatsId);
                        listConversion.ConversationChatsId = itemConversion.ConversationChatsId;
                        listConversion.UserName = itemConversion.UserName;
                        listConversion.ReceiverChatId = itemConversion.ReceiverChatId;
                        listConversion.ReceiverUserImage = itemConversion.ReceiverUserImage;
                        listConversion.ReceiverUserName = itemConversion.ReceiverUserName;
                        listConversion.CreatedMessageAt = itemConversion.CreatedMessageAt;
                        listConversions.Add(listConversion);
                    }

                    return new APIResponse
                    {
                        Message = "Message List!",
                        Status = Status.Success,
                        Payload = listConversions,
                        StatusCode = System.Net.HttpStatusCode.OK,
                    };
                }

                return new APIResponse
                {
                    Message = "Current User not Exists!",
                    Status = Status.Failed,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                };
            }
            catch (Exception ex)
            {
                _loggerManager.LogInfo("-- Error::ChatRepository::GetConversation --");
                _loggerManager.LogError(ex.InnerException.Message);
                _loggerManager.LogError(ex.StackTrace);

                apiResp.Message = "Something went wrong!";
                apiResp.Status = "Internal Server Error";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }

            return apiResp;
        }

        public bool getReadStatus(int ReceiverId)
        {
            string roomName = string.Empty;
            bool readStatus = false;
            var currentUser = _userHelper.GetCurrentUserGuidLogin();

            var chatconversion = _dbContext.ChatConversations.Where(c => c.ConversationsId == ReceiverId && c.UserId == currentUser);
            if (chatconversion != null)
            {
                readStatus = chatconversion.FirstOrDefault().IsUnRead;
            }
            return readStatus;
        }

        public async Task<APIResponse> GetUnreadCount()
        {
            var apiResp = new APIResponse();
            _loggerManager.LogInfo("-- Run::ChatRepository::GetUnreadCount --");

            int unReadCount = 0;

            var currentUser = _userHelper.GetCurrentUserGuidLogin();
            unReadCount = _dbContext.ChatConversations.Where(c => c.UserId == currentUser && c.IsUnRead == false).Count();

            return new APIResponse
            {
                Message = "Unread count fetched successfully",
                Status = Status.Success,
                StatusCode = System.Net.HttpStatusCode.OK,
                Payload = unReadCount
            };

        }
    }
}
