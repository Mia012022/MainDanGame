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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorizeUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userIdStr = session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out _))
        {
            context.Result = new UnauthorizedResult();
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

        // API: GET API/App/All-Games 取得所有類型為 game 的 App
        [HttpGet("App/All-Games")]
        public async Task<App[]> GetGames()
        {
            var query = from app in _context.Apps
                        where app.AppDetail.AppType == "game"
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

        /// <summary>
        /// 取得指定 App 的所有可下載內容 (DLC) 的詳細資料。
        /// </summary>
        /// <remarks>
        /// 此 API 端點用於獲取多個指定應用程式 (App) 的所有可下載內容 (DLC) 的詳細資訊。
        /// 需要提供 App 的 ID 陣列，將傳回應用程式的所有 DLC 的詳細資料。
        /// </remarks>
        /// <param name="appIds"> App ID 陣列。</param>
        /// <returns>
        /// 回傳一個包含動態物件的陣列，其中每個物件包含應用程式的 ID、名稱及其 DLC 的詳細資訊。
        /// 若指定的 App ID 不存在，將傳回空陣列。
        /// </returns>
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

        /// <summary>
        /// 根據 Tag ID 取得對應的 App 列表。
        /// </summary>
        /// <remarks>
        /// 此 API 端點用於根據多個標籤 ID 獲取對應的應用程式 (Apps) 清單。
        /// 需要提供 Tag ID 陣列，將傳回每個 Tag 對應的 App 資訊。
        /// </remarks>
        /// <param name="tagIds"> Tag ID 陣列。</param>
        /// <returns>
        /// 回傳一個包含動態物件的陣列，每個物件包含標籤的 ID、名稱及其對應的應用程式清單。
        /// 若指定的 Tag ID 不存在，將傳回空陣列。
        /// </returns>
        [HttpPost("App/By-Tag")]
        public async Task<dynamic> GetAppsByTags([FromBody] int[] tagIds)
        {
            var query = from tag in _context.GenreTags
                         where tagIds.Contains(tag.TagId)
                         select new { tagId = tag.TagId, tagName = tag.TagName, apps = tag.Apps };

            return await query.ToArrayAsync();
        }

    }
}
