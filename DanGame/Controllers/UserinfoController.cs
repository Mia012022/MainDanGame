using DanGame.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserinfoController : ControllerBase
    {
        private DanGameContext _context;

        public UserinfoController(DanGameContext dbContext)
        {
            _context = dbContext;
        }
        [HttpGet]
        public IActionResult GetuserUrl()
        {
            var id = HttpContext.Session.GetInt32("UserId");

            var totalMonths = _context.Subscriptions
            .Where(subscriptions => subscriptions.UserId == id)
            .Sum(subscriptions => subscriptions.SubscriptionPlanId == 1 ? 3 : 1);

            var totalAppIds = _context.Orders
            .Where(order => order.UserId == id)
            .SelectMany(order => order.OrderItems)
            .Select(orderItem => orderItem.AppId)
            .Distinct()
            .Count();

            var totalFriends = _context.Friendships
            .Where(f => (f.UserId == id || f.FriendUserId == id) && f.Status == "Accepted")
            .Select(f => f.UserId == id ? f.FriendUserId : f.UserId)
            .Distinct()
            .Count();

            var q = from user in _context.Users
                    join profile in _context.UserProfiles on user.UserId equals profile.UserId
                    where user.UserId == id

                    select new
                    {
                        userId = user.UserId,
                        userName = user.UserName,
                        birthDay = profile.DateOfbirth,
                        profilePictureUrl = profile.ProfilePictureUrl,
                        subscriptionMonth = totalMonths,
                        totalAppIds = totalAppIds,
                        totalFriends = totalFriends
                    };

            return Ok(q.FirstOrDefault());

        }
        [HttpGet("{id}")]
        public IActionResult GetUserInfo(int id) 
        {
            var totalFriends = _context.Friendships
            .Where(f => (f.UserId == id || f.FriendUserId == id) && f.Status == "Accepted")
            .Select(f => f.UserId == id ? f.FriendUserId : f.UserId)
            .Distinct()
            .Count();

            return Ok(totalFriends);
        }
    }
}
