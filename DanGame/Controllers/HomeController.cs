using Azure;
using DanGame.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DanGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DanGameContext _context;

        public HomeController(ILogger<HomeController> logger, DanGameContext dbContext)
        {
            _logger = logger;
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
            return View(new
            {
                apps = query.ToArray(),
                subscriptionPlans = _context.SubscriptionPlans.ToList()
            });
        }

        public IActionResult ranking()
        {

            return View();
        }

        [HttpGet("/Game/{id}")]
        public IActionResult Game(int id)
        {
            int? UserId = Request.HttpContext.Session.GetInt32("UserId");
            bool alreadyOwnThisGame = false;
            bool alreadySubscription = false;
            bool alreadyInShoppingCart = false;
            bool alreadyLogin = false;
            if (UserId != null)
            {
                var userGames = from order in _context.Orders
                                where order.UserId == UserId
                                select order;

                var userShopppingCart = from shoppingCart in _context.ShoppingCarts
                                        where shoppingCart.UserId == UserId
                                        select shoppingCart.AppId;
                
                alreadyOwnThisGame = userGames.Any(order => order.OrderItems.Select(orderItem => orderItem.AppId).Contains(id));
                alreadySubscription = userGames.Any(order => order.Subscriptions.Any(subscription => subscription.SubscriptionStatus ?? false));
                alreadyInShoppingCart = userShopppingCart.Contains(id);
                alreadyLogin = true;
            }

            var query = from appDetail in _context.AppDetails
                        where appDetail.AppId == id
                        select new { 
                            detail = appDetail, 
                            media = appDetail.App.AppMedia, 
                            DLCs = appDetail.App.Dlcapps.Select((d) => d.AppDetail), 
                            alreadyOwnThisGame, 
                            alreadySubscription,
                            alreadyInShoppingCart,
                            alreadyLogin
                        };
            return View(query.FirstOrDefault());
        }

        [HttpGet("/test")]
        public IActionResult test()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("/Home/category/{id}")]
        public IActionResult category(int id)
        {
            var query = from genreTag in _context.GenreTags
                        where genreTag.TagId == id
                        select genreTag.Apps.Select( a => a.AppDetail);
            var tag = _context.GenreTags.Find(id);
            return View(new { tag, apps = query.ToArray().SelectMany(i => i).Where(a => a.AppType == "game") });
        }

        public IActionResult gameindex()
        {
            var userId = Request.HttpContext.Session.GetInt32("UserId");
            var ownGames = from order in _context.Orders
                             where order.UserId == userId
                             select (from items in order.OrderItems
                                    select new { appDetail = items.App.AppDetail, alreadyInShoppingCart = items.App.ShoppingCarts.Any(s => s.UserId == userId) }).ToArray();

            var tagApps = (from tag in _context.GenreTags
                          where tag.TagId == 70
                          select tag.Apps.Select(a => a.AppDetail)).FirstOrDefault();
            return View(new {
                alreadyLogin = userId != null,
                ownGames = ownGames.SelectMany(a => a).ToArray(), 
                tagApps = tagApps.ToArray() 
            });
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
