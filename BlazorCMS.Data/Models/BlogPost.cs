using System.ComponentModel.DataAnnotations;

namespace BlazorCMS.Data.Models
{
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string AuthorId { get; set; }  // Must be set before saving

        public string Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedDate { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
