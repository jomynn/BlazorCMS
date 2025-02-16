using BlazorCMS.Data.Models;

namespace BlazorCMS.Data.Repositories
{
    public interface IBlogRepository
    {
        Task<IEnumerable<BlogPost>> GetAllAsync();
        Task<BlogPost> GetByIdAsync(int id);
        Task AddAsync(BlogPost blogPost);
        Task UpdateAsync(BlogPost blogPost);
        Task DeleteAsync(int id);
    }
}
