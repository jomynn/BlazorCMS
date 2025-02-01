using BlazorCMS.Data.Repositories;
using BlazorCMS.Shared.DTOs;
using BlazorCMS.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorCMS.API.Services
{
    public class PageService
    {
        private readonly PageRepository _repository;

        public PageService(PageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PageDTO>> GetAllPagesAsync()
        {
            var pages = await _repository.GetAllAsync();
            return pages.Select(page => new PageDTO
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Content = page.Content,
                IsPublished = page.IsPublished
            }).ToList();
        }

        public async Task<PageDTO> GetPageByIdAsync(int id)
        {
            var page = await _repository.GetByIdAsync(id);
            if (page == null) return null;

            return new PageDTO
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Content = page.Content,
                IsPublished = page.IsPublished
            };
        }

        public async Task<bool> CreatePageAsync(PageDTO pageDto)
        {
            var page = new Page
            {
                Title = pageDto.Title,
                Slug = pageDto.Slug,
                Content = pageDto.Content,
                IsPublished = pageDto.IsPublished
            };

            await _repository.AddAsync(page);
            return true;
        }
    }
}
