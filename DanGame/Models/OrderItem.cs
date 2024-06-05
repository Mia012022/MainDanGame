using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class OrderItem
{
    public int OrderId { get; set; }

    public int AppId { get; set; }

    public int Price { get; set; }

    public virtual App App { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
