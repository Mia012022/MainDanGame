using DanGame.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecentGameController : ControllerBase
    {
        private readonly DanGameContext _context;

        public RecentGameController(DanGameContext context)
        {
            _context = context;
        }


        [HttpGet("dynamic")]
        public async Task<IActionResult> GetFriendDynamic()
        {
            // 確認Session中有UserId
            var userId = HttpContext.Session.GetInt32("UserId");

            // 獲取好友的ID列表
            var friendIds = await _context.Friendships
                .Where(f => (f.UserId == userId || userId == f.FriendUserId) && f.Status == "Accepted")
                .Select(f => f.UserId == userId ? f.FriendUserId : f.UserId)
                .ToListAsync();

            if (!friendIds.Any())
            {
                return NotFound("找不到好友");
            }

            // 獲取好友的所有訂單及其詳細資訊
            var result = await (from friendship in _context.Friendships
                                where (friendship.UserId == userId || userId == friendship.FriendUserId)
                                where friendship.Status == "Accepted"
                                select (friendship.UserId == userId ? friendship.FriendUser.Orders : friendship.User.Orders)).ToArrayAsync();

            var OrderIDs = result.SelectMany(a => a.Select(b => b.OrderId));
            var query = from orderItem in _context.OrderItems
                        where OrderIDs.Contains(orderItem.OrderId)
                        select orderItem;

            //from orderItem in order.OrderItems
            //select new
            //{
            //    ProfilePictureUrl = (friendship.UserId == userId ? friendship.FriendUser.UserProfile.ProfilePictureUrl : friendship.User.UserProfile.ProfilePictureUrl),
            //    order.OrderDate,
            //    orderItem.App.AppName,
            //    orderItem.App.AppDetail.HeaderImage
            //}).ToListAsync();

            var results = await (from o in _context.Orders
                                 join f in _context.Friendships on o.UserId equals f.FriendUserId
                                 join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                                 join a in _context.Apps on oi.AppId equals a.AppId
                                 join ad in _context.AppDetails on a.AppId equals ad.AppId
                                 join up in _context.UserProfiles on o.UserId equals up.UserId
                                 where friendIds.Contains(o.UserId)
                                 orderby o.OrderDate descending
                                 select new
                                 {
                                     ProfilePictureUrl = up.ProfilePictureUrl,
                                     OrderDate = o.OrderDate,
                                     UserId = o.UserId,
                                     AppId = a.AppId,
                                     AppName = a.AppName,
                                     HeaderImage = ad.HeaderImage
                                 })
                                 .ToListAsync();
            var distinctResults = results
                .GroupBy(x => new { x.UserId, x.AppId })
                .Select(g => g.First())
                .OrderByDescending(x => x.OrderDate)
                .ToList();


            if (!distinctResults.Any())
            {
                return NotFound("找不到訂單");
            }

            // 合併結果到一個回應中
            return Ok(distinctResults);
        }
        [HttpGet]
        public ActionResult GetrecentGame()
        {
            var randomGames = _context.AppDetails.Where(app => app.AppType == "game").OrderBy(u => Guid.NewGuid()).Take(3).ToList();

            return Ok(randomGames);
        }
        [HttpGet("room")]
        public ActionResult GetGamesRoom()
        {
            var gameRoom = _context.AppDetails.OrderBy(u => Guid.NewGuid()).Take(3).ToList();
            return Ok(gameRoom);
        }
        [HttpGet("hotgame")]
        public async Task<ActionResult<IEnumerable<AppDetail>>> GetAppDetails()
        {
            var appDetails = await _context.AppDetails
                                           .Where(app => app.AppType == "game")
                                           .OrderByDescending(app => app.Downloaded)
                                           .Select(app => new
                                           {
                                               app.AppName,
                                               app.HeaderImage,
                                               app.Downloaded
                                           })
                                           .Take(5).ToListAsync();

            return Ok(appDetails);
        }
    }
}
