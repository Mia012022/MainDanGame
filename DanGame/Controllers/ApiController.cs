using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
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

        // 其他API动作方法...
    }
}
