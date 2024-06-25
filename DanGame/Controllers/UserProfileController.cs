using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DanGame.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace DanGame.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IConfiguration _configuration;

        public UserProfileController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar, int userId)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            Console.WriteLine(userId);

            using (var httpClient = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new StreamContent(avatar.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(avatar.ContentType);

                    content.Add(fileContent, "image", avatar.FileName);
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", "0c24f9a27948ecd");

                    var response = await httpClient.PostAsync("https://api.imgur.com/3/image", content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    if (responseContent == null)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve image URL from Imgur response.");
                    }
                    dynamic? obj = JsonConvert.DeserializeObject(responseContent);
                    string imageUrl = obj.data.link;
                    //return Ok(fuck);
                    if (imageUrl == null)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve image URL from Imgur response.");
                    }

                    // store imageUrl into sql
                    //string connectionString = _configuration.GetConnectionString("linkDanGameDb")
                    //                  ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                    //int UserId = 13;
                    //using (var connection = new SqlConnection(connectionString))
                    //{
                    //    var query = "UPDATE UserProfile SET ProfilePictureUrl = @ProfilePictureUrl WHERE UserId = @UserId";
                    //    await connection.ExecuteAsync(query, new { ProfilePictureUrl = imageUrl, UserId = UserId });
                    //}

                    return Json(new { url = imageUrl });
                    
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAvatarUrl(UserProfileViewModel model)
        {
            if (model.UserProfile == null || string.IsNullOrEmpty(model.UserProfile.ProfilePictureUrl))
            {
                return BadRequest("Invalid data.");
            }

            string connectionString = _configuration.GetConnectionString("linkDanGameDb")
                                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "UPDATE UserProfile SET ProfilePictureUrl = @ProfilePictureUrl WHERE UserId = @UserId";
                await connection.ExecuteAsync(query, new { ProfilePictureUrl = model.UserProfile.ProfilePictureUrl, UserId = HttpContext.Session.GetInt32("UserId") });
            }
            Console.WriteLine(model.UserProfile.ProfilePictureUrl);
            Console.WriteLine(model.User?.UserId);
            return RedirectToAction("UserIndex","User", new { id = model.User?.UserId });
        }
    }
}
