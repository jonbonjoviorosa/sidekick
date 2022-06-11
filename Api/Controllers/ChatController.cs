using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sidekick.Api.DataAccessLayer.Interfaces;
using System;
using System.Threading.Tasks;
using System.Net;
using Sidekick.Model.Chat;

namespace Sidekick.Api.Controllers
{
    [Route("api/Chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _messageRepository;
        public ChatController(IChatRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [HttpGet("GetConversation/{bookingId}")]
        public async Task<IActionResult> GetConversationPitch(Guid bookingId)
        {
            var response = await _messageRepository.GetConversation(bookingId);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ConversationViewModel convo)
        {
            var response = await _messageRepository.SendMessage(convo);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("SendMessage/{conversionId}")]
        public async Task<IActionResult> SendMessageConversion([FromBody] ConversationViewModel convo)
        {
            var response = await _messageRepository.SendMessageConversion(convo);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("MessageList/{senderId}")]
        public async Task<IActionResult> MessageList(Guid senderId)
        {
            return Ok(await _messageRepository.GetMessageList(senderId));
        }

        [HttpGet("ConversionList")]
        public async Task<IActionResult> ConversionList()
        {
            return Ok(await _messageRepository.GetConversionList());
        }

        [HttpGet("UnreadCount")]
        public async Task<IActionResult> UnreadCount()
        {
            return Ok(await _messageRepository.GetUnreadCount());
        }

    }
}
