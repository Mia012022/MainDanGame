using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;

public class AuthorizeUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            context.Result = new UnauthorizedResult();
        }else
        {
            // 若在後續代碼中使用userId，可以將其添加到 context.HttpContext.Items 中
            context.HttpContext.Items["UserId"] = userId.Value;
        }
        base.OnActionExecuting(context);
    }
}

namespace DanGame.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiController(DanGameContext dbContext) : ControllerBase
    {
        private readonly DanGameContext _context = dbContext;

        // API: GET api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // API: GET api/user/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // API: GET API/User/Profile 登入後取得個人 Profile
        [HttpGet("User/Profile")]
        [AuthorizeUser]
        async public Task<UserProfile?> GetUserProfile()
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));

            var query = from profile in _context.UserProfiles
                        where profile.UserId == userId
                        select profile;

            return await query.FirstOrDefaultAsync();
        }

        // API: GET API/User/ChatRooms 登入後取得個人所有聊天室
        [HttpGet("User/ChatRooms")]
        [AuthorizeUser]
        async public Task<dynamic> GetUserChatRooms()
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));

            var query = from chatRoom in _context.ChatRooms
                        from member in chatRoom.ChatRoomMembers
                        where member.UserId == userId
                        select new { chatRoomId = chatRoom.ChatRoomId, members = chatRoom.ChatRoomMembers };

            return await query.ToArrayAsync();
        }

        // API: GET API/User/Friends 登入後取得個人所有好友
        [HttpGet("User/Friends")]
        [AuthorizeUser]
        async public Task<dynamic> GetUserFriends()
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));

            var userFriend = from friend in _context.Friendships
                             where (friend.UserId == userId || friend.FriendUserId == userId)
                             select new
                             {
                                 profile = (friend.UserId == userId) ? friend.FriendUser.UserProfile : friend.User.UserProfile,
                                 status = friend.Status
                             };
            return new
            {
                Accepted = await userFriend.Where(u => u.status == "Accepted").Select(u => u.profile).ToArrayAsync(),
                Pending = await userFriend.Where(u => u.status == "Pending").Select(u => u.profile).ToArrayAsync()
            };

        }

        // API: GET API/User/ShoppingCart 取得個人購物車品項
        [HttpGet("User/ShoppingCart")]
        [AuthorizeUser]
        async public Task<ShoppingCart[]> GetUserShoppingItem()
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));

            var query = from shoppingCart in _context.ShoppingCarts
                        where shoppingCart.UserId == userId
                        select shoppingCart;

            return await query.ToArrayAsync();
        }

        // API: POST API/User/ShoppingCart 新增個人購物車品項
        [HttpPost("User/ShoppingCart")]
        [AuthorizeUser]
        async public Task<ShoppingCart[]> AddUserShoppingItem([FromBody] int appId)
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));

            var newShoppingItem = new ShoppingCart()
            {
                UserId = userId,
                AppId = appId,
                AddedTime = DateTime.UtcNow,
            };

            _context.ShoppingCarts.Add(newShoppingItem);

            var query = from shoppingCart in _context.ShoppingCarts
                        where shoppingCart.UserId == userId
                        select shoppingCart;

            await _context.SaveChangesAsync();

            return await query.ToArrayAsync();
        }

        // API: DELETE API/User/ShoppingCart 刪除個人購物車品項
        [HttpDelete("User/ShoppingCart")]
        [AuthorizeUser]
        async public Task<ShoppingCart[]> DelUserShoppingItem([FromBody] int appId)
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));

            var query = from shoppingCart in _context.ShoppingCarts
                        where shoppingCart.UserId == userId
                        select shoppingCart;

            var target = query.Where((c) => c.AppId == appId).First();

            if (target != null)
            {
                _context.ShoppingCarts.Remove(target);
                await _context.SaveChangesAsync();
            }

            return await query.ToArrayAsync();
        }

        // API: GET API/App/All-Games 取得所有類型為 game 的 App
        [HttpGet("App/All-Games")]
        public async Task<App[]> GetGames()
        {
            var query = from app in _context.Apps
                        where app.AppDetail != null && app.AppDetail.AppType == "game"
                        select app;
            return await query.ToArrayAsync();
        }

        // API: GET API/App/All-Tags 取得所有 Tag
        [HttpGet("App/All-Tags")]
        public async Task<GenreTag[]> GetTags()
        {
            var query = from tag in _context.GenreTags
                        select tag;
            return await query.ToArrayAsync();
        }

        // API: POST API/App/AppsDetail 取得 App 的詳細資料 
        [HttpPost("App/Detail")]
        public async Task<AppDetail[]> AppsDetail([FromBody] params int[] appIds)
        {
            var query = from app in _context.AppDetails
                        where appIds.Contains(app.AppId)
                        select app;
            return await query.ToArrayAsync();
        }

        // API: POST API/App/DLCs 取得 App DLC 詳細資料 
        [HttpPost("App/DLCs")]
        public async Task<dynamic> GetAppDLCsDetail([FromBody] int[] appIds)
        {
            var query = from app in _context.Apps
                        where appIds.Contains(app.AppId)
                        from dlcApp in app.Dlcapps
                        select new {
                            FullGameID = app.AppId,
                            FullGameName = app.AppName,
                            DLCs = dlcApp.AppDetail 
                        };

            return await query.ToArrayAsync();
        }

        // API: POST API/App/By-Tag 取得所有含有此 Tag 的 App
        [HttpPost("App/By-Tag")]
        public async Task<dynamic> GetAppsByTags([FromBody] int[] tagIds)
        {
            var query = from tag in _context.GenreTags
                         where tagIds.Contains(tag.TagId)
                         select new { tagId = tag.TagId, tagName = tag.TagName, apps = tag.Apps };

            return await query.ToArrayAsync();
        }

		// 其他API动作方法...
		[HttpGet("SubscriptionPlan")]
		public async Task<ActionResult<IEnumerable<SubscriptionPlan>>> GetSubscriptionPlans()
		{
			var subscriptionPlans = await _context.SubscriptionPlans.ToListAsync();
			if (subscriptionPlans == null || subscriptionPlans.Count == 0)
			{
				return NotFound();
			}
			return subscriptionPlans;
		}
    }
}
