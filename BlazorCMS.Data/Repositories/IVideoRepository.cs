using BlazorCMS.Data.Models;

namespace BlazorCMS.Data.Repositories;

public interface IVideoRepository
{
    Task<Video?> GetByIdAsync(int id);
    Task<List<Video>> GetAllAsync();
    Task<List<Video>> GetPublishedAsync();
    Task<List<Video>> GetByAuthorAsync(string authorId);
    Task<Video> CreateAsync(Video video);
    Task<Video> UpdateAsync(Video video);
    Task<bool> DeleteAsync(int id);
    Task<List<Video>> SearchAsync(string searchTerm);
    Task<List<Video>> GetByTagAsync(string tag);
    Task IncrementViewCountAsync(int id);
}
