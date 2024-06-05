using System;
using System.Collections.Generic;

namespace DanGame.Models;

public partial class Subscription
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int SubscriptionPlanId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? SubscriptionStatus { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
