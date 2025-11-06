# BlazorCMS

A modern, full-stack Content Management System built with Blazor, ASP.NET Core, and Entity Framework Core.

## Overview

BlazorCMS is a professional-grade blog and content management system featuring:

- **Blazor Server** admin panel for content management
- **Blazor WebAssembly** public website for viewing content
- **ASP.NET Core Web API** backend with JWT authentication
- **SQLite** database with Entity Framework Core
- **Role-based access control** (Admin, Editor, User)
- **n8n Automation** support for workflow automation

## Features

### Content Management
- 📝 **Blog Posts** - Create, edit, publish blog posts with rich content
- 📄 **Static Pages** - Manage custom pages with SEO-friendly slugs
- 👤 **User Management** - User registration, authentication, and role management
- 🔒 **Security** - JWT-based authentication with secure password hashing

### Automation (NEW!)
- 🤖 **n8n Integration** - Automate workflows with pre-built examples
- 📧 **Email Notifications** - Automatic welcome emails and subscriber notifications
- 📱 **Social Media** - Auto-post to Twitter, LinkedIn, Slack
- 💾 **Backups** - Scheduled backups to Google Drive, AWS S3, Azure
- ⏰ **Scheduled Publishing** - Publish blog posts at specific times

## Quick Start

### Prerequisites

- .NET 6.0 SDK or later
- SQLite (included)
- Node.js 18+ (optional, for n8n automation)

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/BlazorCMS.git
cd BlazorCMS

# Restore dependencies
dotnet restore

# Run database migrations
cd BlazorCMS.API
dotnet ef database update

# Start the API
dotnet run --project BlazorCMS.API

# In another terminal, start the Admin Panel
dotnet run --project BlazorCMS.Admin

# In another terminal, start the Public Website
dotnet run --project BlazorCMS.Client
```

### Default Login

- **Email**: `admin@blazorcms.com`
- **Password**: Check your database initialization script

### Access Points

- **API**: http://localhost:7250
- **API Documentation**: http://localhost:7250/swagger
- **Admin Panel**: http://localhost:5001
- **Public Website**: http://localhost:5000

## Project Structure

```
BlazorCMS/
├── BlazorCMS.API/              # REST API backend
│   ├── Controllers/            # API endpoints
│   ├── Services/               # Business logic
│   └── Program.cs              # API configuration
│
├── BlazorCMS.Admin/            # Admin panel (Blazor Server)
│   ├── Pages/                  # Admin UI pages
│   └── Services/               # Admin services
│
├── BlazorCMS.Client/           # Public website (Blazor WASM)
│   ├── Pages/                  # Public pages
│   └── Services/               # Client services
│
├── BlazorCMS.Data/             # Data access layer
│   ├── Models/                 # Entity models
│   ├── Repositories/           # Data repositories
│   └── ApplicationDbContext.cs # EF Core context
│
├── BlazorCMS.Infrastructure/   # Infrastructure services
│   ├── Authentication/         # JWT authentication
│   ├── Email/                  # Email service
│   ├── Logging/                # Logging service
│   └── Storage/                # File storage
│
├── BlazorCMS.Shared/           # Shared code
│   └── DTOs/                   # Data transfer objects
│
└── n8n-examples/               # n8n automation workflows
    ├── workflows/              # Pre-built workflow files
    ├── README.md               # Automation documentation
    ├── SETUP_GUIDE.md          # Setup instructions
    └── QUICK_START.md          # Quick start guide
```

## n8n Automation

BlazorCMS includes pre-built n8n workflows for common automation tasks:

### Available Workflows

1. **Blog Publication to Social Media** - Automatically share blog posts on Twitter, LinkedIn, Slack
2. **User Registration Welcome** - Send welcome emails and add to mailing lists
3. **Blog Backup to Cloud** - Daily backups to Google Drive, AWS S3, Azure
4. **Scheduled Publishing** - Publish blog posts at scheduled times
5. **Email Subscriber Notifications** - Notify subscribers of new content

### Quick Start with n8n

```bash
# Start n8n with Docker
docker run -it --rm \
  --name n8n \
  -p 5678:5678 \
  -e BLAZORCMS_API_URL=http://host.docker.internal:7250 \
  -e BLAZORCMS_JWT_TOKEN="your-jwt-token" \
  -v ~/.n8n:/home/node/.n8n \
  n8nio/n8n

# Access n8n at http://localhost:5678
```

📚 **Full Documentation**: See [n8n-examples/README.md](n8n-examples/README.md) for complete setup instructions.

## API Documentation

The API is fully documented with Swagger/OpenAPI. Access the interactive documentation at:

**http://localhost:7250/swagger**

### Main Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

#### Blog Posts
- `GET /api/blog` - Get all blog posts
- `GET /api/blog/{id}` - Get specific blog post
- `POST /api/blog` - Create new blog post (requires auth)
- `PUT /api/blog/{id}` - Update blog post (requires auth)
- `DELETE /api/blog/{id}` - Delete blog post (requires auth)

#### Pages
- `GET /api/pages` - Get all pages
- `GET /api/pages/{id}` - Get specific page
- `POST /api/pages` - Create new page (requires auth)

## Configuration

### Database

Configure your database connection in `BlazorCMS.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blazorcms.db"
  }
}
```

Supports: SQLite, PostgreSQL, MySQL, SQL Server

### JWT Authentication

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key-here-min-16-chars",
    "Issuer": "BlazorCMS",
    "Audience": "BlazorCMS",
    "ExpirationHours": 2
  }
}
```

### Email Service

Configure SMTP settings:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@blazorcms.com",
    "FromName": "BlazorCMS"
  }
}
```

## Development

### Building the Project

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build BlazorCMS.API
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Migrations

```bash
# Add new migration
cd BlazorCMS.API
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName
```

## Deployment

### Prerequisites

- .NET Runtime 6.0+
- Web server (IIS, Nginx, Apache)
- SSL certificate (recommended)

### Production Configuration

1. Update `appsettings.Production.json` with production settings
2. Set `ASPNETCORE_ENVIRONMENT=Production`
3. Configure reverse proxy (Nginx/IIS)
4. Enable HTTPS
5. Set strong JWT secret key
6. Configure production database

### Docker Deployment

```bash
# Build Docker image
docker build -t blazorcms:latest .

# Run container
docker run -d \
  -p 80:80 \
  -p 443:443 \
  -v /path/to/data:/app/data \
  blazorcms:latest
```

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Backend API | ASP.NET Core 6/7/8 |
| Admin UI | Blazor Server |
| Public UI | Blazor WebAssembly |
| Database | SQLite + Entity Framework Core |
| Authentication | JWT Bearer + ASP.NET Identity |
| API Documentation | Swagger/OpenAPI |
| Automation | n8n |
| Email | SMTP |
| Storage | Local File System |

## Security

- ✅ JWT-based authentication
- ✅ Password hashing with ASP.NET Identity
- ✅ Role-based authorization
- ✅ HTTPS support
- ✅ CSRF protection
- ✅ XSS prevention
- ✅ SQL injection protection (EF Core)

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📖 **Documentation**: See the [docs](docs/) folder
- 🐛 **Issues**: Report bugs via [GitHub Issues](https://github.com/yourusername/BlazorCMS/issues)
- 💬 **Discussions**: Join [GitHub Discussions](https://github.com/yourusername/BlazorCMS/discussions)
- 📧 **Email**: support@blazorcms.com

## Roadmap

### Version 2.0
- [ ] Multi-language support
- [ ] Advanced SEO features
- [ ] Media library management
- [ ] Comment system
- [ ] Search functionality
- [ ] Analytics dashboard

### Version 3.0
- [ ] Multi-tenancy support
- [ ] Advanced workflow automation
- [ ] Custom fields and content types
- [ ] API rate limiting
- [ ] CDN integration

## Acknowledgments

- Built with [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- Automated with [n8n](https://n8n.io/)
- Inspired by modern CMS platforms

---

**⭐ Star this repo if you find it useful!**

Made with ❤️ by the BlazorCMS Team
