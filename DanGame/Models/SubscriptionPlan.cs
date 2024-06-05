using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Duration { get; set; }

    public int Price { get; set; }

    public string ThemeColor { get; set; } = null!;

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
