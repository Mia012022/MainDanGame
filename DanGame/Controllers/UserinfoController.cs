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
            int id = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
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
    }
}
