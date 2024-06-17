using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;

public class ImageController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly DanGameContext _context;

    public ImageController(IHttpClientFactory httpClientFactory, DanGameContext context)
    {
        _httpClient = httpClientFactory.CreateClient();
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImg(IFormFile profilePicture)
    {
        if (profilePicture == null || profilePicture.Length == 0)
        {
            return Json(new { success = false, message = "No image uploaded." });
        }

        var clientId = Environment.GetEnvironmentVariable("0c24f9a27948ecd");
        if (clientId == null)
        {
            return Json(new { success = false, message = "Imgur Client ID is not configured." });
        }

        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(profilePicture.OpenReadStream());
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(profilePicture.ContentType);
        content.Add(streamContent, "image");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "44cb35756025d1c6ad694c0f61b63dd2d2239450");

        try
        {
            var response = await _httpClient.PostAsync("https://api.imgur.com/3/image", content);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadAsStringAsync();
            var imgurResponse = JsonSerializer.Deserialize<ImgurResponse>(responseData);

            if (imgurResponse == null || imgurResponse.Data == null)
            {
                return Json(new { success = false, message = "Error parsing Imgur response." });
            }

            var imageUrl = $"https://i.imgur.com/{imgurResponse.Data.Id}.jpg";

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId.Value);
            if (userProfile == null)
            {
                userProfile = new UserProfile
                {
                    UserId = userId.Value,
                    ProfilePictureUrl = imageUrl
                };
                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                userProfile.ProfilePictureUrl = imageUrl;
                _context.UserProfiles.Update(userProfile);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, url = imageUrl });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error uploading image to Imgur: {ex.Message}");
            return Json(new { success = false, message = "Error uploading image to Imgur." });
        }
    }
}

public class ImgurResponse
{
    public ImgurData? Data { get; set; }
}

public class ImgurData
{
    public string? Id { get; set; }
}

