using Microsoft.AspNetCore.Mvc;
using DanGame.Models;

namespace DanGame.Controllers
{
    public class GamelibraryController : Controller
    {
        private DanGameContext _context;

        public GamelibraryController(DanGameContext dbContext)
        {
            _context = dbContext;
        }
        public IActionResult Index()
        {
			var id = Request.HttpContext.Session.GetInt32("UserId");
            var orders = _context.Orders
                         .Where(order => order.UserId == id)
                         .Select(order => order.OrderItems.Select(oi => new { oi.AppId, oi.App.AppName })).ToList();

			return View(orders.SelectMany(a => a));
		}
    }
}
