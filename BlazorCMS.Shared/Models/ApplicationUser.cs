using Microsoft.AspNet.Identity.EntityFramework;

namespace BlazorCMS.Shared.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;
    }
}
