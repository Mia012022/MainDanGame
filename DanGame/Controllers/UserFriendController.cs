using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFriendController : ControllerBase
    {
        private DanGameContext _context;

        public UserFriendController(DanGameContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet]
        
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
        [HttpGet("{id}")]
        public IActionResult GetFriendCard(int id) 
        {
            var q = from user in _context.Users
                    join profile in _context.UserProfiles on user.UserId equals profile.UserId
                    where user.UserId == id
                    select new
                    {
                        userId = user.UserId,
                        userName = user.UserName,
                        birthDay = profile.DateOfbirth,
                        profilePictureUrl = profile.ProfilePictureUrl
                    };

            return Ok(q.FirstOrDefault());
        }

        //[HttpGet()]
        //public ActionResult GetUserList()
        //{
        //    int id = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
        //    var query = from friendship in _context.Friendships
        //                where friendship.UserId == id || friendship.FriendUserId == id
                        
        //                select new
        //                {
        //                    user.UserId,
        //                    user.UserName,
        //                    profile.ProfilePictureUrl,
        //                    friend.FriendUserId,
        //                    friend.Status,
        //                    FriendName = friendUser.UserName
        //                };

        //    return Ok(query.ToList());
        //}

        
        [HttpPost]
        public async Task<IActionResult> Sendinvite([FromQuery] int id)
        {
            int userId = Convert.ToInt32(Request.HttpContext.Session.GetString("UserId"));
            var invite = new Friendship
            {
                Status = "Pending",
                UserId = userId,
                FriendUserId = id
            };
            _context.Friendships.Add(invite);
            await _context.SaveChangesAsync();
            var q = from friend in _context.Friendships
                    where friend.UserId == id && friend.Status == "Pending"
                    select friend;
            return Ok(await q.ToArrayAsync());

        }

        [HttpPut("{friendId}")]
        public IActionResult AcceptFriend(int friendId)
        {
            var userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var friendRequest = (from friendShip in _context.Friendships
                                 where friendShip.UserId == friendId || friendShip.FriendUserId == userId
                                 where friendShip.Status == "Pending"
                                 select friendShip).FirstOrDefault();
            if (friendRequest == null)
            {
                return NotFound();
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Database.ExecuteSqlRaw("DISABLE TRIGGER trg_ValidateFriendship ON Friendships");

                    friendRequest.Status = "Accepted";

                    _context.SaveChanges();

                    _context.Database.ExecuteSqlRaw("ENABLE TRIGGER trg_ValidateFriendship ON Friendships");

                    transaction.Commit();
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }

            return Ok(friendRequest);
        }

        // DELETE: api/UserFriend/2
        [HttpDelete("{friendId}")]
        public IActionResult DeleteFriend(int friendId)
        {
            var userId =  Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var friendRequest = (from friendShip in _context.Friendships
                                 where (friendShip.UserId == friendId && friendShip.FriendUserId == userId) ||
                                       (friendShip.UserId == userId && friendShip.FriendUserId == friendId)
                                 select friendShip).FirstOrDefault();
            if (friendRequest == null)
            {
                return NotFound();
            }

            _context.Friendships.Remove(friendRequest);
            _context.SaveChanges();
            return Ok(friendRequest);
        }


        //[HttpGet("{id}")]
        //async public Task<List<int>> GetFriends([FromQuery] int id)
        //{
        //    var query = from friend in _context.FriendLists
        //                where friend.UserId == id || friend.FriendUserId == id
        //                select friend.UserId == id ? friend.FriendUserId : friend.UserId;

        //    return query.ToList();

        //}

        //[HttpGet]
        //public ActionResult GetUserNameList(string keyword)
        //{
        //    var query = from o in _context.UserProfiles
        //                where o.FirstName.Contains(keyword) || o.LastName.Contains(keyword)
        //                select new { UserID = o.UserId, UserName = $"{o.FirstName}{o.LastName}" };

        //    return Ok(query.ToArray());
        //}
        //    [HttpGet]
        //    public ActionResult GetEmailList()
        //    {
        //        var query = (from o in _context.Users
        //                     select o).Take(10);
        //        List<User> result = query.ToList();

        //        return Ok(result);
        //    }
        //}
        //[HttpGet("{id}")]
        //public async Task<ActionResult<UserFriendInfo>> GetUser(long? id)
        //{
        //    var user = await _context.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    return user
        //}
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Friendlist>>> GetDanGameList()
        //{
        //    return await _context.Friendlist.ToListAsync();
        //}

        //[HttpGet("{id}")]
        //public ActionResult GetUserList(long id)
        //{
        //    var query = from user in _context.Users
        //                join profile in _context.UserProfiles on user.UserId equals profile.UserId
        //                join friend in _context.FriendLists on user.UserId equals friend.UserId
        //                where user.UserId == id
        //                select new
        //                {
        //                    user.UserId,
        //                    user.UserName,
        //                    profile.ProfilePictureUrl,
        //                    friend.FriendUserId,
        //                    friend.Status
        //                };

        //    return Ok(query.ToList());
        //}
    }
}
