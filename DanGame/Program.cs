using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI.Extensions;
using DanGame.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// �]�w��Ʈw�s��
var connectionString = builder.Configuration.GetConnectionString("linkDanGameDb");
builder.Services.AddDbContext<DanGameContext>(options =>
    options.UseSqlServer(connectionString));

// �]�w����M���ϪA��
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllers();

// �[�J IHttpContextAccessor �A��
builder.Services.AddHttpContextAccessor();

// �[�JOpen AI �A��
builder.Services.AddOpenAIService();

// Configure HttpClient
builder.Services.AddHttpClient();

// �[�Jsession�A��
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // �]�wsession�L���ɶ�
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
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=10");
    }
});

app.UseRouting();

app.UseCors("AllowAll");

app.MapHub<DanGame.Hubs.ChatHub>("/chatHub");

app.UseAuthorization();

// �[�Jsession�����n��
app.UseSession();

//app.MapControllers(); 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
