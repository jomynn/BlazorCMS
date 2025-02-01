using Microsoft.AspNetCore.Mvc;
using BlazorCMS.Shared.DTOs;
using BlazorCMS.Infrastructure.Logging;
using BlazorCMS.Data.Repositories;
using BlazorCMS.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IEnumerable<BlogPost>> GetAllBlogs() => await _repository.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(int id)
        {
            var blog = await _repository.GetByIdAsync(id);
            if (blog == null) return NotFound();
            return Ok(blog);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] BlogPostDTO blogDto)
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
            return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blog);
        }
    }
}
