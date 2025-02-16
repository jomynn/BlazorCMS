using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlazorCMS.Data.Models;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Page> Pages { get; set; }
    }
}
