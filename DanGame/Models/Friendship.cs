using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class Friendship
{
    public int UserId { get; set; }

    public int FriendUserId { get; set; }

    public string Status { get; set; } = null!;

    public virtual User FriendUser { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
