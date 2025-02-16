using BlazorCMS.Data.Models;
using BlazorCMS.Data.Repositories;

namespace BlazorCMS.API.Services
{
    public class PageService
    {
        private readonly IRepository<Page> _pageRepository;

        public PageService(IRepository<Page> pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<IEnumerable<Page>> GetAllPagesAsync()
        {
            return await _pageRepository.GetAllAsync();
        }

        public async Task<Page?> GetPageByIdAsync(int id)
        {
            return await _pageRepository.GetByIdAsync(id);
        }

        public async Task AddPageAsync(Page page)
        {
            await _pageRepository.AddAsync(page);
        }

        public async Task UpdatePageAsync(Page page)
        {
            await _pageRepository.UpdateAsync(page);
        }

        public async Task DeletePageAsync(int id)
        {
            await _pageRepository.DeleteAsync(id);
        }
    }
}
