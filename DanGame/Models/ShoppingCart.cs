using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class ShoppingCart
{
    public int UserId { get; set; }

    public int AppId { get; set; }

    public DateTime AddedTime { get; set; }

    public virtual App App { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
