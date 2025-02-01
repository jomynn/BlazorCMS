using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCMS.Shared.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;
    }
}
