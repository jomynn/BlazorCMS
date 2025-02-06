using Microsoft.AspNetCore.Identity;

namespace BlazorCMS.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
