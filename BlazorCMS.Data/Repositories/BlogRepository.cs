using BlazorCMS.Data.Models;
using BlazorCMS.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCMS.Data.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync() => await _context.BlogPosts.ToListAsync();
        public async Task<BlogPost> GetByIdAsync(int id) => await _context.BlogPosts.FindAsync(id);
        public async Task AddAsync(BlogPost blogPost)
        {
            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(BlogPost blogPost)
        {
            _context.BlogPosts.Update(blogPost);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                _context.BlogPosts.Remove(blogPost);
                await _context.SaveChangesAsync();
            }
        }
    }

}
