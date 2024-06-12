using System.Numerics;

namespace DanGame.Models
{
    public class GameViewModel
    {
        public App[] Apps { get; set; }
        public AppDetail AppDetail { get; set; }
        public GenreTag GenreTag { get; set; }

        public SubscriptionPlan SubscriptionPlan { get; set; }
    }
}
