using DanGame.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchFriendController : ControllerBase
    {
        private DanGameContext _context;

        public SearchFriendController(DanGameContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet]
        public ActionResult GetUserNameList(string keyword)
        {
            var query = from o in _context.UserProfiles
                        where o.FirstName.Contains(keyword) || o.LastName.Contains(keyword) || o.ProfilePictureUrl.Contains(keyword)
                        select new { UserID = o.UserId, UserName = $"{o.FirstName}{o.LastName}", Avatar = o.ProfilePictureUrl };

            return Ok(query.ToArray());
        }
    }
}
