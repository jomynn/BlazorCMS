using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlazorCMS.Data.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BlazorCMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbProvider = _configuration["DatabaseProvider"];
                string connectionString = _configuration.GetConnectionString(dbProvider);

                switch (dbProvider)
                {
                    case "PostgreSQL":
                        optionsBuilder.UseNpgsql(connectionString);
                        break;
                    case "MySQL":
                        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                        break;
                    case "MSSQL":
                        optionsBuilder.UseSqlServer(connectionString);
                        break;
                    default:
                        optionsBuilder.UseSqlite(connectionString);
                        break;
                }
            }
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Page> Pages { get; set; }
    }
}
