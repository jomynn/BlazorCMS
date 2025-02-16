using BlazorCMS.Data.Models;
using BlazorCMS.Data.Repositories;
using BlazorCMS.Infrastructure.Logging;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.API.Controllers
{
    [Route("api/blog")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogRepository _repository;
        private readonly LoggingService _logger;

        public BlogController(IBlogRepository repository, LoggingService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        ////// Get all blogs
        //[HttpGet]
        //public async Task<IEnumerable<BlogPost>> GetAllBlogs_0() => await _repository.GetAllAsync();

       
        [HttpGet]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _repository.GetAllAsync();
            return Ok(blogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            var blog = await _repository.GetByIdAsync(id);
            if (blog == null)
            {
                _logger.LogWarning($"Blog post with ID {id} not found.");
                return NotFound(new { message = "Blog post not found" });
            }
            return Ok(blog);
        }


        [HttpPost]// ✅ Create a new blog
        public async Task<IActionResult> CreateBlog([FromBody] BlogPostDTO blogDto)
        {
            if (blogDto == null)
            {
                return BadRequest(new { message = "Invalid blog data." });
            }

            if (string.IsNullOrEmpty(blogDto.AuthorId))
            {
                return BadRequest(new { message = "AuthorId is required." });
            }

            var blogPost = new BlogPost
            {
                Title = blogDto.Title,
                Content = blogDto.Content,
                AuthorId = blogDto.AuthorId, // Ensure this is assigned
                Author = blogDto.Author,
                PublishedDate = blogDto.PublishedDate ?? DateTime.UtcNow,
                IsPublished = blogDto.IsPublished
            };

            await _repository.AddAsync(blogPost);
            return CreatedAtAction(nameof(GetBlogById), new { id = blogPost.Id }, blogDto);
        }

        [HttpPut("{id}")] // ✅ Update a blog
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogPost blogDto)
        {
            var blog = await _repository.GetByIdAsync(id);
            if (blog == null) return NotFound();

            blog.Title = blogDto.Title;
            blog.Content = blogDto.Content;
            blog.IsPublished = blogDto.IsPublished;

            await _repository.UpdateAsync(blog);
            return NoContent();
        }

        [HttpDelete("{id}")] // ✅ Delete a blog
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _repository.GetByIdAsync(id);
            if (blog == null) return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
