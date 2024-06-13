using DanGame.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace DanGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private DanGameContext _context;
		private static readonly HttpClient _httpClient = new HttpClient();

		public HomeController( DanGameContext dbContext)
		{

			_context = dbContext;

		}

		public IActionResult Index()
        {
			//// 檢查是否有已登入的session
			//if (HttpContext.Session.GetString("UserId") != null)
			//{
			//    // 如果已登入，導向UserController的UserIndex頁面
			//    return RedirectToAction("UserIndex", "User");
			//}

			//// 否則顯示Index頁面
			///
			var query = from app in _context.Apps
						where app.AppDetail.AppType == "game"
						select new
						{
							appId = app.AppId,
							appName = app.AppName,
							headerImage = app.AppDetail.HeaderImage,
							appDesc = app.AppDetail.ShortDescription,
							releaseDate = app.AppDetail.ReleaseDate,
							downloaded = app.AppDetail.Downloaded,
							price = app.AppDetail.Price,
							tags = app.Tags
						};
			return View(new { apps = query.ToArray(), subscriptionPlans =  _context.SubscriptionPlans.ToList()
		});
        }

        public IActionResult ranking()
        {

            return View();
        }

		public IActionResult Privacy()
		{
			return View();
		}

		public async Task<IActionResult> game()
		{
			string apiUrl = "http://localhost:5000/api/app";
			List<App> apps = null;
			try
			{
				var client = _httpClientFactory.CreateClient();
				var response = await client.GetStringAsync(apiUrl);
				apps = JsonConvert.DeserializeObject<List<App>>(response);
			}
			catch (Exception ex)
			{
				// 记录异常信息
				Console.WriteLine($"Error fetching or deserializing data: {ex.Message}");
				// 处理异常，例如返回错误视图或其他
				return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
			}

			if (apps == null)
			{
				// 处理没有获取到数据的情况
				return View("NoData");
			}

			var tagIds = new List<int> { 1, 28 }; // 动作为 tagId 1，模拟为 tagId 28
			var taggedApps = new Dictionary<string, List<App>>();

			// 获取带有指定标签的应用
			foreach (var tagId in tagIds)
			{
				var appsWithTag = apps.Where(app => app.Tags.Any(tag => tag.TagId == tagId)).ToList();
				string tagName = tagId == 1 ? "動作" : "模擬";
				taggedApps[tagName] = appsWithTag;
			}

			// 获取所有应用
			taggedApps["所有遊戲"] = apps;

			return View(taggedApps);
		}

		public IActionResult category()
        {
            return View();
        }

        public IActionResult RedirectToUserIndex()
        {
            return RedirectToAction("UserIndex", "User");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
