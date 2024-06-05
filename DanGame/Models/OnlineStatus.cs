using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class OnlineStatus
{
    public int UserId { get; set; }

    public DateTime OnlineTime { get; set; }

    public DateTime OfflineTime { get; set; }
}
