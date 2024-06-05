using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ChatRoom
{
    public int ChatRoomId { get; set; }

    public string ChatRoomType { get; set; } = null!;

    public DateTime CreateTime { get; set; }

    public DateTime? LastMessageTime { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatRoomMember> ChatRoomMembers { get; set; } = new List<ChatRoomMember>();
}
