using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace api.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IDistributedCache _cache;
        public CommentRepository(ApplicationDBContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Comment> CreateAsync(Comment commentModel)
        {
            await _context.Comments.AddAsync(commentModel);
            await _context.SaveChangesAsync();
            return commentModel;
        }

        public async Task<Comment?> DeleteAsync(int id)
        {
            var commentModel = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if (commentModel == null)
            {
                return null;
            }

            _context.Comments.Remove(commentModel);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"comment-{id}");
            return commentModel;
        }

        public async IAsyncEnumerable<Comment> GetAllAsync(CommentQueryObject queryObject)
        {
            var comments = _context.Comments.Include(a => a.AppUser).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(queryObject.Symbol))
            {
                comments = comments.Where(s => s.Stock != null && s.Stock.Symbol == queryObject.Symbol);
            };
            if (queryObject.IsDecsending == true)
            {
                comments = comments.OrderByDescending(c => c.CreatedOn);
            }
            
            await foreach (var comment in comments.AsAsyncEnumerable())
            {
                yield return comment;
            }
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            var cacheKey = $"comment-{id}";
            string? cachedComment = await _cache.GetStringAsync(cacheKey);
            Comment? comment;

            if (string.IsNullOrEmpty(cachedComment))
            {
                comment = await _context.Comments.Include(a => a.AppUser).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                if (comment != null)
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                    };
                    await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(comment, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), options);
                }
            }
            else
            {
                comment = JsonConvert.DeserializeObject<Comment>(cachedComment);
            }

            return comment;
        }

        public async Task<Comment?> UpdateAsync(int id, Comment commentModel)
        {
            var existingComment = await _context.Comments.FindAsync(id);

            if (existingComment == null)
            {
                return null;
            }

            existingComment.Title = commentModel.Title;
            existingComment.Content = commentModel.Content;
            existingComment.FilePath = commentModel.FilePath;

            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"comment-{id}");
            return existingComment;
        }
    }
}