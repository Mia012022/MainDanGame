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
            if (!int.TryParse(Request.HttpContext.Session.GetString("UserId"), out int userId))
            {
                return Unauthorized("用戶未登錄");
            }

            // 獲取好友的ID列表
            var friendIds = await _context.Friendships
                .Where(f => f.UserId == userId && f.Status == "Accepted")
                .Select(f => f.FriendUserId)
                .ToListAsync();

            if (!friendIds.Any())
            {
                return NotFound("找不到好友");
            }

            // 獲取好友的所有訂單及其詳細資訊
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
                                     AppName = a.AppName,
                                     HeaderImage = ad.HeaderImage
                                 }).ToListAsync();


            if (!results.Any())
            {
                return NotFound("找不到訂單");
            }

            // 合併結果到一個回應中
            return Ok(results);
        }
        [HttpGet]
        public ActionResult GetrecentGame()
        {
            var randomGames = _context.AppDetails.OrderBy(u => Guid.NewGuid()).Take(3).ToList();

            return Ok(randomGames);
        }
        [HttpGet("room")]
        public ActionResult GetGamesRoom()
        {
            var gameRoom = _context.AppDetails.OrderBy(u => Guid.NewGuid()).Take(3).ToList();
            return Ok(gameRoom);
        }
    }
}
