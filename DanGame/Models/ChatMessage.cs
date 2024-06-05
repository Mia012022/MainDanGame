using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ChatMessage
{
    public int ChatRoomId { get; set; }

    public int MessageId { get; set; }

    public int SenderId { get; set; }

    public string MessageContent { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
