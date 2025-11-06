using BlazorCMS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorCMS.Data.Repositories;

public class VideoRepository : IVideoRepository
{
    private readonly ApplicationDbContext _context;

    public VideoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Video?> GetByIdAsync(int id)
    {
        return await _context.Videos.FindAsync(id);
    }

    public async Task<List<Video>> GetAllAsync()
    {
        return await _context.Videos
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Video>> GetPublishedAsync()
    {
        return await _context.Videos
            .Where(v => v.IsPublished && v.ProcessingStatus == "Completed")
            .OrderByDescending(v => v.PublishedDate ?? v.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Video>> GetByAuthorAsync(string authorId)
    {
        return await _context.Videos
            .Where(v => v.AuthorId == authorId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<Video> CreateAsync(Video video)
    {
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();
        return video;
    }

    public async Task<Video> UpdateAsync(Video video)
    {
        _context.Videos.Update(video);
        await _context.SaveChangesAsync();
        return video;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var video = await GetByIdAsync(id);
        if (video == null)
            return false;

        _context.Videos.Remove(video);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Video>> SearchAsync(string searchTerm)
    {
        return await _context.Videos
            .Where(v => v.Title.Contains(searchTerm) ||
                       (v.Description != null && v.Description.Contains(searchTerm)) ||
                       (v.Tags != null && v.Tags.Contains(searchTerm)))
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Video>> GetByTagAsync(string tag)
    {
        return await _context.Videos
            .Where(v => v.Tags != null && v.Tags.Contains(tag))
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var video = await GetByIdAsync(id);
        if (video != null)
        {
            video.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }
}
