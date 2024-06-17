using System;
using System.Collections.Generic;

namespace DanGame.Models
{
    public class UserOrderViewModel
    {
        public User? User { get; set; }
        public List<OrderDetail>? Orders { get; set; }
    }

    public class OrderDetail
    {
        public Order? Order { get; set; }
        public List<AppDetail>? AppDetails { get; set; }
        public decimal TotalPrice { get; set; }
        public List<SubscriptionDetail>? Subscriptions { get; set; }
    }

    public class SubscriptionDetail
    {
        public Subscription? Subscription { get; set;}
        public SubscriptionPlan? SubscriptionPlan { get; set; }
    }
}

