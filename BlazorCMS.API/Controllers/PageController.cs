using BlazorCMS.Data.Models;
using BlazorCMS.Data.Repositories;
using BlazorCMS.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BlazorCMS.API.Controllers
{
    [Route("api/pages")]
    [ApiController]
    public class PageController : ControllerBase
    {
        private readonly PageRepository _repository;

        public PageController(PageRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<Page>> GetAllPages() => await _repository.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPage(int id)
        {
            var page = await _repository.GetByIdAsync(id);
            if (page == null) return NotFound();
            return Ok(page);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePage([FromBody] PageDTO pageDto)
        {
            var page = new Page
            {
                Title = pageDto.Title,
                Slug = pageDto.Slug,
                Content = pageDto.Content,
                IsPublished = pageDto.IsPublished
            };

            await _repository.AddAsync(page);
            return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
        }
    }
}
