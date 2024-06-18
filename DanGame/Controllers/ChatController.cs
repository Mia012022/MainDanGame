using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using System;
using OpenAI.Extensions;
using OpenAI.ObjectModels;
using Microsoft.AspNetCore.Cors;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IOpenAIService _openAiService;

        public ChatController(IOpenAIService openAiService)
        {
            _openAiService = openAiService;
            _openAiService.SetDefaultModelId(OpenAI.ObjectModels.Models.Gpt_4o_2024_05_13);
        }

        public class ChatContent
        {
            public ChatMessage ToChatMessage()
            {
                return Role switch
                {
                    "System" => ChatMessage.FromSystem(Content),
                    "Assistant" => ChatMessage.FromAssistant(Content),
                    _ => ChatMessage.FromUser(Content),
                };
            }
            public required string Content { get; set; }
            public required string Role { get; set; }
        }

        [HttpPost("Assistan")]
        public async Task<string> Chat([FromBody] ChatContent[] contents)
        {
            string? result;

            var chatRequests = new ChatCompletionCreateRequest();

            chatRequests.Messages = [
                ChatMessage.FromSystem("你是一個名為DanGame遊戲訂閱平台的AI助理，你必須回答使用者問題並使用繁體中文回答"),
            ];
            foreach (ChatContent content in contents)
            {
                chatRequests.Messages.Add(content.ToChatMessage());
            }

            var completionResult = await _openAiService.ChatCompletion.CreateCompletion(chatRequests);

            if (completionResult.Successful)
            {
                result = completionResult.Choices.First().Message.Content;
            } else
            {
                result = completionResult.Error?.Message;
            }

            return (result != null) ? result : "Error" ;
        }
    }
}
