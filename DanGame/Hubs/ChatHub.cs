using DanGame.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Messaging;

namespace DanGame.Hubs
{
    public class ChatHub : Hub
    {
        // Dictionary to map userId to connectionId
        private static Dictionary<int, HashSet<string>> chatRoomConnections = [];
        public static HashSet<int> onlineUsers = [];
        private readonly DanGameContext _context;

        public ChatHub(DanGameContext context)
        {
            _context = context;
        }

        // Method to send a message from one user to another
        public async Task SendMessage(int chatRoomID, string message)
        {
            if (chatRoomConnections.TryGetValue(chatRoomID, out HashSet<string>? connectionIDs))
            {
                int senderID = GetUserIdentifier();
                var query = from chatRoom in _context.ChatRooms
                            where chatRoom.ChatRoomId == chatRoomID && chatRoom.ChatRoomMembers.Any((m) => m.UserId == senderID)
                            select chatRoom;
                if (query.FirstOrDefault() != null)
                {
                    foreach (string connectID in connectionIDs)
                    {
                        await Clients.Client(connectID).SendAsync("ReceiveMessage", chatRoomID, senderID, message);
                    }
                    ChatMessage chatMessage = new()
                    {
                        ChatRoomId = chatRoomID,
                        SenderId = senderID,
                        MessageContent = message,
                        CreatedTime = DateTime.UtcNow
                    };
                    await _context.ChatMessages.AddAsync(chatMessage);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            int userId = GetUserIdentifier();
            var userChatRoomIDs = (from user in _context.Users
                                where user.UserId == userId
                                select user.ChatRoomMembers.Select((u) => u.ChatRoomId)).FirstOrDefault();
            if (userChatRoomIDs != null)
            {
                onlineUsers.Add(userId);
                foreach (int chatRoomID in userChatRoomIDs)
                {
                    if (!chatRoomConnections.ContainsKey(chatRoomID))
                    {
                        chatRoomConnections[chatRoomID] = [];
                    }
                    chatRoomConnections[chatRoomID].Add(Context.ConnectionId);
                }

                foreach (string connection in chatRoomConnections.Values.SelectMany(a => a))
                {
                    await Clients.Client(connection).SendAsync("UserStatusChange", userId, "online");
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            int userId = GetUserIdentifier();
            onlineUsers.Remove(userId);
            foreach (var connections in chatRoomConnections.Values)
            {
                connections.Remove(Context.ConnectionId);
            }

            foreach (string connection in chatRoomConnections.Values.SelectMany(a => a))
            {
                await Clients.Client(connection).SendAsync("UserStatusChange", userId, "offline");
            }

            await base.OnDisconnectedAsync(exception);
        }

        private int GetUserIdentifier()
        {
            var httpContext = Context.GetHttpContext();
            int? userId = httpContext?.Session?.GetInt32("UserId");
            if (userId == null)
            {
                throw new Exception("Unknown User");
            }
            else
            {
                return userId.Value;
            }
        }
    }

}
