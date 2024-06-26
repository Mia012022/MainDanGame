using System;
using System.Collections.Generic;

namespace DanGame.Models
{
    public class UserProfileViewModel
    {
        public User? User { get; set; }
        public UserProfile? UserProfile { get; set; }
        public List<CreditCardInfo>? CreditCardInfos { get; set; }
        public Subscription? UserSubscription { get; set; }
    }
}