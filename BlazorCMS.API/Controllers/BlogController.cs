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
        private readonly BlogRepository _repository;
        private readonly LoggingService _logger;

        public BlogController(BlogRepository repository, LoggingService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        //// Get all blogs
        [HttpGet]
        public async Task<IEnumerable<BlogPost>> GetAllBlogs_0() => await _repository.GetAllAsync();

        //// Get all blogs
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<BlogPostDTO>>> GetAllBlogs()
        //{
        //    var blogs = await _repository.GetAllAsync();

        //    if (blogs == null || blogs.Count == 0)
        //    {
        //        _logger.LogInfo("No blogs found.");
        //        return NotFound(new { message = "No blog posts available." });
        //    }

        //    // Convert BlogPost to BlogPostDTO
        //    var blogDtos = blogs.ConvertAll(blog => new BlogPostDTO
        //    {
        //        Id = blog.Id,
        //        Title = blog.Title,
        //        Content = blog.Content,
        //        Author = blog.Author,
        //        PublishedDate = blog.PublishedDate,
        //        IsPublished = blog.IsPublished
        //    });

        //    return Ok(blogDtos);
        //}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(int id)
        {
            var blog = await _repository.GetByIdAsync(id);
            if (blog == null) return NotFound();
            return Ok(blog);
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

            var blogDto = new BlogPostDTO
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                Author = blog.Author,
                PublishedDate = blog.PublishedDate,
                IsPublished = blog.IsPublished
            };

            return Ok(blogDto);
        }


        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] BlogPost blogDto)
        {


            var blogPost = new BlogPost
            {
                Title = blogDto.Title,
                Content = blogDto.Content,
                Author = blogDto.Author,
                PublishedDate = blogDto.PublishedDate,
                IsPublished = blogDto.IsPublished
            };

            await _repository.AddAsync(blogPost);
            return CreatedAtAction(nameof(GetBlogById), new { id = blogPost.Id }, blogDto);
        }
    }
}
