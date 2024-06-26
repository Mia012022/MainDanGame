using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using OpenAI.Extensions;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using DanGame.Models;

using ChatMessage = OpenAI.ObjectModels.RequestModels.ChatMessage;


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
                ChatMessage.FromSystem("Q:訂閱會員有哪些好處？A:訂閱會員可以無限次遊玩平台上的所有遊戲，享受定期更新。"),
                ChatMessage.FromSystem("Q:不訂閱可以購買遊戲嗎？A:可以，非訂閱用戶可以單獨購買遊戲，並在購買後永久擁有遊玩權限。"),
                ChatMessage.FromSystem("Q:如何創建帳戶？A:點擊網站右上角的頭像圖像按鈕，進到登入畫面點擊“註冊”，填寫必要的訊息並完成註冊過程。註冊成功後，您將收到確認郵件。"),
                ChatMessage.FromSystem("Q:忘記密碼怎麼辦？A:點擊“忘記密碼”，輸入您的電子郵件地址，我們會發送一封重置密碼的郵件給您。"),
                ChatMessage.FromSystem("Q:如何更改帳戶訊息？A:登入帳戶後，前往“帳戶設置”頁面，您可以修改電子郵件地址、密碼和其他個人訊息。"),
                ChatMessage.FromSystem("Q:遊戲無法啟動，該怎麼辦？A:首先請確保您的設備滿足遊戲的最低系統要求，並且已安裝最新的驅動程序。如果問題仍然存在，請聯繫我們的技術支持團隊。"),
                ChatMessage.FromSystem("Q:支持那些付款方式？A:我們目前支持信用卡、綠界pay的電子支付方式，後續還會依照用戶使用情況追加其他支付方式。"),
                ChatMessage.FromSystem("Q:我們的付款訊息安全嗎？A:我們使用先進的加密技術來保護您的付款訊息，確保交易的安全性。"),
                ChatMessage.FromSystem("如果有人需要推薦請推薦: 1. Terraria(id: 105600) 2.Project Zomboid(id: 108600) 3.Kenshi(id: 233860) 4.漫威午夜之子(id: 368260) 連結為 /Game/{id} 不需要根目錄"),
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
