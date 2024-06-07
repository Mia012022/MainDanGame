using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace DanGame.Controllers
{
    public class UserController : Controller
    {
        private readonly DanGameContext _context;

        public UserController(DanGameContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UserIndex()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId);

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
            var viewModel = new UserProfileViewModel
            {
                User = user,
                UserProfile = userProfile
            };

            return View(viewModel);
        }

        // GET: User/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        public async Task<IActionResult> Login(string useremail, string password)
        {
            // 假設我們有一個方法來驗證使用者帳號和密碼
            var user = await ValidateUser(useremail, password);

            if (user != null)
            {
                // 驗證成功，建立session並儲存於cookie
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("Username", user.UserName);

                // 重新導向至HomeController的index頁面
                return RedirectToAction("Index", "Home");
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
                .SingleOrDefaultAsync(u => u.Email == useremail && u.PasswordHash == password);
        }

        //----------------------------------------------------------------------------------------------

        // GET: User/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            // 清除 session
            HttpContext.Session.Clear();

            // 重新導向至首頁
            return RedirectToAction("Index", "Home");
        }

        // GET: /User/ChangeEmail
        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId);

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
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

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
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId)
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
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId)
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
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId);

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
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // 驗證密碼是否正確
            if (user.PasswordHash != password)
            {
                // 密碼錯誤，返回錯誤訊息
                ViewBag.ErrorMessage = "舊密碼錯誤，請重試。";
                return View(new UserProfileViewModel
                {
                    User = user,
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId)
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
                    UserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId.ToString() == userId)
                });
            }

            // 更新密碼
            user.PasswordHash = newpassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // 登出使用者並重定向到登入頁面
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
    }
}
