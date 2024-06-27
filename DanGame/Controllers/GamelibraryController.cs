using Microsoft.AspNetCore.Mvc;
using DanGame.Models;
using System.Linq;

namespace DanGame.Controllers
{
    public class GamelibraryController : Controller
    {
        private DanGameContext _context;

        public GamelibraryController(DanGameContext dbContext)
        {
            _context = dbContext;
        }

		[PageAuthorizeUser]
        public IActionResult Index()
        {
			var id = Request.HttpContext.Session.GetInt32("UserId");
			//         var orders = _context.Orders
			//                      .Where(order => order.UserId == id)
			//                      .Select(order => order.OrderItems.Select(oi => new { oi.AppId, oi.App.AppName }))
			//			 .ToList();

			//return View(orders.SelectMany(a => a));
			var orderItems = _context.Orders
					 .Where(order => order.UserId == id)
					 .SelectMany(order => order.OrderItems.Select(oi => new { oi.AppId, oi.App.AppName }))
					 .ToList();
			//排除重複的App
			var uniqueOrderItems = new List<object>();
			var appIdSet = new HashSet<int>();

			foreach (var item in orderItems)
			{
				if (appIdSet.Add(item.AppId))
				{
					uniqueOrderItems.Add(item);
				}
			}

			return View(uniqueOrderItems);
		}
    }
}
