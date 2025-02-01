using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCMS.Data.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        public bool IsPublished { get; set; } = false;
    }
}
