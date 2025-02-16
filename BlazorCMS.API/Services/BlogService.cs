using BlazorCMS.Data.Models;
using BlazorCMS.Data.Repositories;
using BlazorCMS.Shared.DTOs;

namespace BlazorCMS.API.Services
{
    public class BlogService
    {
        private readonly IBlogRepository _repository;

        public BlogService(IBlogRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BlogPostDTO>> GetAllBlogsAsync()
        {
            var blogs = await _repository.GetAllAsync();
            return blogs.Select(blog => new BlogPostDTO
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Author = blog.Author,
                PublishedDate = blog.PublishedDate,
                IsPublished = blog.IsPublished
            }).ToList();
        }

        public async Task<BlogPostDTO> GetBlogByIdAsync(int id)
        {
            var blog = await _repository.GetByIdAsync(id);
            if (blog == null) return null;

            return new BlogPostDTO
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Author = blog.Author,
                PublishedDate = blog.PublishedDate,
                IsPublished = blog.IsPublished
            };
        }

        public async Task<bool> CreateBlogAsync(BlogPostDTO blogDto)
        {
            var blog = new BlogPost
            {
                Title = blogDto.Title,
                Content = blogDto.Content,
                Author = blogDto.Author,
                PublishedDate = blogDto.PublishedDate,
                IsPublished = blogDto.IsPublished
            };

            await _repository.AddAsync(blog);
            return true;
        }
    }
}
