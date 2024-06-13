using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DanGame.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 設定資料庫連接
var connectionString = builder.Configuration.GetConnectionString("linkDanGameDb");
builder.Services.AddDbContext<DanGameContext>(options =>
    options.UseSqlServer(connectionString));

// 設定控制器和視圖服務
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();


// Add services to the container.
builder.Services.AddControllers();

// 加入 IHttpContextAccessor 服務
builder.Services.AddHttpContextAccessor();

// Configure HttpClient
builder.Services.AddHttpClient();

// 加入session服務
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定session過期時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 加入session中介軟體
app.UseSession();

//app.MapControllers(); 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
