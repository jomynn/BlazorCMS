using BlazorCMS.Data.Models;
using BlazorCMS.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCMS.Data.Repositories
{
    public class BlogRepository : IRepository<BlogPost>
    {
        private readonly ApplicationDbContext _context;

        public BlogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync() => await _context.BlogPosts.ToListAsync();
        public async Task<BlogPost> GetByIdAsync(int id) => await _context.BlogPosts.FindAsync(id);
        public async Task AddAsync(BlogPost blog) { _context.BlogPosts.Add(blog); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(BlogPost blog) { _context.BlogPosts.Update(blog); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog != null) _context.BlogPosts.Remove(blog);
            await _context.SaveChangesAsync();
        }
    }
}
