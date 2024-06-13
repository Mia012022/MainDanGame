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

        // 其他API动作方法...
    }
}
