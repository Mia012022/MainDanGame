﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanGame.Models;
using System.Reflection;
using System.Linq;
using Azure;

namespace DanGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private DanGameContext _context;

        public AppController(DanGameContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet]
        public async Task<dynamic> GetApps()
        {
            var query = from app in _context.Apps
                        where app.AppDetail.AppType == "game"
                        select new {
                            appId=app.AppId, 
                            appName=app.AppName, 
                            headerImage = app.AppDetail.HeaderImage,
                            appDesc = app.AppDetail.ShortDescription, 
                            releaseDate = app.AppDetail.ReleaseDate,
                            downloaded = app.AppDetail.Downloaded,
                            price = app.AppDetail.Price,
                            tags = app.Tags
                        };
            return await query.ToListAsync();
        }

        [HttpPost("AppsDetail")]
        public async Task<List<AppDetail>> AppsDetail([FromBody] int[] tagIds)
        {
            var query = _context.AppDetails.Join(
                    _context.GenreTags,
                    appDetail => appDetail.AppId,
                    genreTag => genreTag.TagId,
                    (appDetail, genreTag) => new { AppDetail = appDetail, GenreTag = genreTag }).Where(joinResult => tagIds.Contains(joinResult.GenreTag.TagId))
                    .Select(joinResult => joinResult.AppDetail);

            return await query.ToListAsync();
        }

        [HttpPost("AppDLCsDetail")]
        public async Task<dynamic> GetAppDLCsDetail([FromBody] int appid)
        {
            var query = from app in _context.Apps
                        where app.AppId == appid
                        from dlcApp in app.Dlcapps
                        join appDetail in _context.AppDetails on dlcApp.AppId equals appDetail.AppId
                        select appDetail;

            return await query.ToArrayAsync();
        }

        [HttpPost("GetAppByTags")]
        public dynamic GetAppByTags([FromBody] int[] tagIds)
        {
            //var query = from app in _context.Apps
            //            from tag in app.Tags
            //            where tagIds.Contains(tag.TagId)
            //            select new { TagId = tag.TagId, apps = app };
            Func<int, IEnumerable<App>> getAppByTag = (int tagId) => {
                var query = from app in _context.Apps
                            from tag in app.Tags
                            where tag.TagId == tagId
                            select app;
                return query.ToArray();
            };

            var query = from tagId in tagIds
                         join tag in _context.GenreTags on tagId equals tag.TagId
                         select new { tagId = tag.TagId, tagName = tag.TagName, apps = getAppByTag(tagId) };

            return query.ToArray();
        }

        [HttpGet("Tags")]
        public async Task<List<GenreTag>> GetTags()
        {
            var query = from tag in _context.GenreTags
                        select tag;
            return await query.ToListAsync();
        }

    }
}
