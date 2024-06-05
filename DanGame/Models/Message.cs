using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ChatRoomId { get; set; }

    public DateTime CreatedTime { get; set; }

    public string MessageContent { get; set; } = null!;

    public int UserId { get; set; }
}
