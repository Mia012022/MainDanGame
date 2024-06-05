using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ArticalView
{
    public int ArticalViewId { get; set; }

    public int ArticalId { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
