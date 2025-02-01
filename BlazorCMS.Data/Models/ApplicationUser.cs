using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCMS.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;
    }
}
