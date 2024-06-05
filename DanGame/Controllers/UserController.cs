using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;
using System.Linq;
using System.Threading.Tasks;


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
            return View();
        }
    }
}
