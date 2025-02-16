using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BlazorCMS.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Set base path
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Get database provider & connection string
            string dbProvider = configuration["DatabaseProvider"];
            string connectionString = configuration.GetConnectionString(dbProvider);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("❌ Database connection string is missing!");
            }

            // Configure the database provider
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

            return new ApplicationDbContext(optionsBuilder.Options, configuration);
        }
    }
}
