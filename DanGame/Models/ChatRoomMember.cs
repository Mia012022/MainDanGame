using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ChatRoomMember
{
    public int ChatRoomId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
