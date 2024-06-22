using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace DanGame.Controllers
{
    public class UserController : Controller
    {
        private readonly DanGameContext _context;

        public UserController(DanGameContext context)
        {
            _context = context;
        }

        private string GetSHA256(string value)
        {
            using var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(byteArray).ToLower();
        }

        public async Task<IActionResult> UserIndex()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value);

            if (userProfile == null)
            {
                // 如果沒有找到對應的 UserProfile，可以選擇返回錯誤頁面或者初始化一個新的 UserProfile
                userProfile = new UserProfile { UserId = user.UserId, ProfilePictureUrl = "~/image/user_profile.jpg" }; // 假設有一個默認圖片
            }

            // 檢查 ProfilePictureUrl 是否為空或為 null
            if (string.IsNullOrEmpty(userProfile.ProfilePictureUrl))
            {
                userProfile.ProfilePictureUrl = "~/image/user_profile.jpg"; // 默認圖片
            }

            ViewBag.UserName = user.UserName;

            var creditCardInfos = await _context.CreditCardInfos.Where(c => c.UserId == userId.Value).ToListAsync();
            var subscription = _context.Subscriptions.FirstOrDefault(s => s.UserId == userId.Value && s.SubscriptionStatus == true);

            var viewModel = new UserProfileViewModel
            {
                User = user,
                UserProfile = userProfile,
                CreditCardInfos = creditCardInfos,
                UserSubscription = subscription
            };

            return View(viewModel);
        }

        // GET: User/Login
        [HttpGet]
        public IActionResult Login([FromQuery] string? redirectTo)
        {
            return View(new { redirectTo });
        }

        // POST: User/Login
        [HttpPost]
        public async Task<IActionResult> Login(string useremail, string password, string? redirectTo)
        {
            // 假設我們有一個方法來驗證使用者帳號和密碼
            var user = await ValidateUser(useremail, password);

            if (user != null)
            {
                // 驗證成功，建立session並儲存於cookie
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.UserName);

                // 重新導向至HomeController的index頁面
                if (redirectTo != null)
                {
                    return Redirect(redirectTo);
                } else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // 驗證失敗，顯示錯誤訊息
            ViewBag.ErrorMessage = "Invalid email or password.";
            return View();
        }

        // 驗證使用者帳號及密碼的方法
        private async Task<User?> ValidateUser(string useremail, string password)
        {
            // 假設有一個方法用來驗證使用者資料
            // 比如：使用Entity Framework從資料庫中查詢
                return  await _context.Users
                .SingleOrDefaultAsync(u => u.Email == useremail && u.PasswordHash == GetSHA256(password));
        }

        // GET: User/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            var user = new User
            {
                UserName = model.UserName ?? string.Empty,
                Email = model.Email ?? string.Empty,
                PasswordHash = model.PasswordHash ?? string.Empty,
                CreatedAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
            //return Ok(model);
        }

        // Post: User/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            // 清除 session
            HttpContext.Session.Clear();
           
            // 重新導向至首頁
            return RedirectToAction("Index", "Home");
        }

        //---------------------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileName = Path.GetFileName(profilePicture.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue)
                {
                    var userProfile = await GetUserProfileFromDatabase(userId.Value);
                    if (userProfile != null)
                    {
                        userProfile.ProfilePictureUrl = "/images/profile/" + fileName;
                        await UpdateUserProfileInDatabase(userProfile);

                        return Json(new { success = true, url = userProfile.ProfilePictureUrl });
                    }
                }
            }

            return Json(new { success = false });
        }

        private async Task<UserProfile?> GetUserProfileFromDatabase(int userId)
        {
            return await _context.UserProfiles.SingleOrDefaultAsync(up => up.UserId == userId);
        }

        private async Task UpdateUserProfileInDatabase(UserProfile userProfile)
        {
            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();
        }
        //----------------------------------------------------------------------------------------------------

        // GET: /User/UserIndex/CheckEmail
        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { available = false });
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);
            return Json(new { available = !emailExists });
        }

        // GET: /User/UserIndex/CheckUserName
        [HttpGet]
        public async Task<IActionResult> CheckUserName(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                return Json(new { available = false });
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserName == UserName);
            return Json(new { available = !userExists });
        }

        // POST: /User/UserIndex/UpdateUserName
        [HttpPost]
        public async Task<IActionResult> UpdateUserName(int userId, string userName)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return Json(new { success = false });
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserName == userName);
            if (userExists)
            {
                return Json(new { success = false });
            }

            user.UserName = userName;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: /User/UserIndex/DeleteCreditCard
        [HttpPost]
        public async Task<IActionResult> DeleteCreditCard(string cardNumber)
        {
            var userId = Request.HttpContext.Session.GetInt32("UserId");
            var card = await (from creditCard in _context.CreditCardInfos
                              where creditCard.UserId == userId && creditCard.CardNumber == cardNumber
                              select creditCard).SingleOrDefaultAsync();

            if (card != null)
            {
                _context.CreditCardInfos.Remove(card);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UserIndex", "User");
        }

        // GET: /User/ChangeEmail
        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value);

            var viewModel = new UserProfileViewModel
            {
                User = user,
                UserProfile = userProfile
            };

            return View(viewModel);
        }

        // POST: /User/ChangeEmail
        [HttpPost]
        public async Task<IActionResult> ChangeEmail(string password, string email)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // 驗證密碼是否正確
            if (user.PasswordHash != password)
            {
                // 密碼錯誤，返回錯誤訊息
                ViewBag.ErrorMessage = "密碼錯誤，請重試。";
                return View(new UserProfileViewModel
                {
                    User = user,
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value)
                });
            }

            // 驗證電子郵件格式
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (!emailRegex.IsMatch(email))
            {
                ViewBag.ErrorMessage = "請輸入有效的電子郵件地址。";
                return View(new UserProfileViewModel
                {
                    User = user,
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value)
                });
            }

            // 更新電子郵件
            user.Email = email;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // 登出使用者並重定向到登入頁面
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        // GET: /User/ChangePwd
        [HttpGet]
        public async Task<IActionResult> ChangePwd()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value);

            var viewModel = new UserProfileViewModel
            {
                User = user,
                UserProfile = userProfile
            };

            return View(viewModel);
        }

        // POST: /User/ChangePwd
        [HttpPost]
        public async Task<IActionResult> ChangePwd(string password, string newpassword, string confirmpassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // 驗證密碼是否正確
            if (user.PasswordHash != GetSHA256(password))
            {
                // 密碼錯誤，返回錯誤訊息
                ViewBag.ErrorMessage = "舊密碼錯誤，請重試。";
                return View(new UserProfileViewModel
                {
                    User = user,
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value)
                });
            }

            // 驗證新密碼和確認新密碼是否一致
            if (newpassword != confirmpassword)
            {
                // 新密碼和確認密碼不一致，返回錯誤訊息
                ViewBag.ErrorMessage = "新密碼和確認密碼不一致，請重試。";
                return View(new UserProfileViewModel
                {
                    User = user,
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value)
                });
            }

            // 更新密碼
            user.PasswordHash = GetSHA256(newpassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // 登出使用者並重定向到登入頁面
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

		// GET: /User/PurchaseHistory
		[HttpGet]
		public async Task<IActionResult> PurchaseHistory()
		{
			var userId = HttpContext.Session.GetInt32("UserId");

			if (!userId.HasValue)
			{
				return RedirectToAction("Login", "User");
			}

			var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

			if (user == null)
			{
				return RedirectToAction("Login", "User");
			}

            var orders = await _context.Orders
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.OrderDate) // 按日期排序
                .ToListAsync();

            var orderDetails = new List<OrderDetail>();

            foreach (var order in orders)
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == order.OrderId)
                    .ToListAsync();

                var appDetails = new List<AppDetail>();
                decimal totalPrice = 0;

                foreach (var item in orderItems)
                {
                    var appDetail = await _context.AppDetails
                        .FirstOrDefaultAsync(ad => ad.AppId == item.AppId);

                    if (appDetail != null)
                    {
                        appDetails.Add(appDetail);
                        totalPrice += item.Price;
                    }
                }

                // 處理訂閱表和訂閱計畫
                var subscriptions = await _context.Subscriptions
                    .Where(s => s.OrderId == order.OrderId)
                    .ToListAsync();

                var subscriptionDetails = new List<SubscriptionDetail>();

                foreach (var subscription in subscriptions)
                {
                    var subscriptionPlan = await _context.SubscriptionPlans
                        .FirstOrDefaultAsync(sp => sp.PlanId == subscription.SubscriptionPlanId);

                    subscriptionDetails.Add(new SubscriptionDetail
                    {
                        Subscription = subscription,
                        SubscriptionPlan = subscriptionPlan
                    });
                    totalPrice += subscriptionPlan?.Price ?? 0; // 加上訂閱計劃的價格，如果 subscriptionPlan 為 null，則加上 0
                }

                orderDetails.Add(new OrderDetail
                {
                    Order = order,
                    AppDetails = appDetails,
                    TotalPrice = totalPrice,
                    Subscriptions = subscriptionDetails
                });
            }

            var viewModel = new UserOrderViewModel
            {
                User = user,
                Orders = orderDetails
            };

            return View(viewModel);
        }
	}
}
