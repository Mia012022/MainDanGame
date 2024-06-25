
namespace DanGame.Models
{
    public class SubscriptionCheckViewModel
    {
        public Dictionary<string, string> Parameters { get; internal set; }
        public List<SubscriptionPlan> SubscriptionPlan { get; internal set; }
    }
}
