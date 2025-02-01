using Microsoft.EntityFrameworkCore;
using BlazorCMS.Data.Models;

namespace BlazorCMS.Data.Repositories
{
    public class PageRepository : IRepository<Page>
    {
        private readonly ApplicationDbContext _context;

        public PageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Page>> GetAllAsync() => await _context.Pages.ToListAsync();
        public async Task<Page> GetByIdAsync(int id) => await _context.Pages.FindAsync(id);
        public async Task AddAsync(Page page) { _context.Pages.Add(page); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(Page page) { _context.Pages.Update(page); await _context.SaveChangesAsync(); }
        public async Task DeleteAsync(int id)
        {
            var page = await _context.Pages.FindAsync(id);
            if (page != null) _context.Pages.Remove(page);
            await _context.SaveChangesAsync();
        }
    }
}
