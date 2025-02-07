Full Directory Structure

BlazorCMS/
│── BlazorCMS.sln               # Solution file
│
├── 📂 backend/                 # Backend folder
│   ├── 📂 BlazorCMS.API/        # ASP.NET Core Web API
│   │   ├── 📂 Controllers/      # API Controllers
│   │   │   ├── AuthController.cs
│   │   │   ├── BlogController.cs
│   │   │   ├── PageController.cs
│   │   ├── 📂 Services/         # Business logic services
│   │   │   ├── AuthService.cs
│   │   │   ├── BlogService.cs
│   │   │   ├── PageService.cs
│   │   ├── 📂 Configuration/    # API Configuration
│   │   │   ├── DependencyInjection.cs
│   │   ├── 📄 Program.cs        # API startup file
│   │   ├── 📄 appsettings.json  # API Configuration
│   │   ├── 📄 BlazorCMS.API.csproj
│
│   ├── 📂 BlazorCMS.Data/       # Data Access Layer
│   │   ├── 📂 Models/           # Database models
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── BlogPost.cs
│   │   │   ├── Page.cs
│   │   ├── 📂 Repositories/      # Data access repository pattern
│   │   │   ├── IRepository.cs
│   │   │   ├── BlogRepository.cs
│   │   │   ├── PageRepository.cs
│   │   ├── 📄 ApplicationDbContext.cs # Database context
│   │   ├── 📄 BlazorCMS.Data.csproj
│
│   ├── 📂 BlazorCMS.Infrastructure/   # Cross-cutting concerns
│   │   ├── 📂 Authentication/         # JWT Authentication
│   │   │   ├── JwtTokenService.cs
│   │   ├── 📂 Email/                  # Email sending
│   │   │   ├── EmailService.cs
│   │   ├── 📂 Logging/                # Logging service
│   │   │   ├── LoggingService.cs
│   │   ├── 📄 DependencyInjection.cs
│   │   ├── 📄 BlazorCMS.Infrastructure.csproj
│
├── 📂 frontend/                # Frontend folder
│   ├── 📂 BlazorCMS.Admin/      # Admin Panel (Blazor Server)
│   │   ├── 📂 Pages/            # Admin pages
│   │   │   ├── Dashboard.razor
│   │   │   ├── Users.razor
│   │   │   ├── BlogPosts.razor
│   │   │   ├── Pages.razor
│   │   ├── 📂 Services/         # API service communication
│   │   │   ├── AuthService.cs
│   │   │   ├── BlogService.cs
│   │   │   ├── PageService.cs
│   │   ├── 📂 Components/       # Reusable UI components
│   │   │   ├── Sidebar.razor
│   │   │   ├── Navbar.razor
│   │   ├── 📂 Shared/           # Shared layout
│   │   │   ├── MainLayout.razor
│   │   ├── 📄 App.razor         # Entry point
│   │   ├── 📄 Program.cs        # Admin panel startup
│   │   ├── 📄 BlazorCMS.Admin.csproj
│
│   ├── 📂 BlazorCMS.Client/     # Public website (Blazor WebAssembly)
│   │   ├── 📂 Pages/            # Public pages
│   │   │   ├── Index.razor
│   │   │   ├── Blog.razor
│   │   │   ├── BlogDetail.razor
│   │   │   ├── Page.razor
│   │   │   ├── Login.razor
│   │   │   ├── Register.razor
│   │   ├── 📂 Services/         # API service communication
│   │   │   ├── AuthService.cs
│   │   │   ├── BlogService.cs
│   │   │   ├── PageService.cs
│   │   ├── 📂 Components/       # UI components
│   │   │   ├── Navbar.razor
│   │   │   ├── Footer.razor
│   │   ├── 📂 Shared/           # Shared layout
│   │   │   ├── MainLayout.razor
│   │   ├── 📂 wwwroot/          # Static assets
│   │   │   ├── css/
│   │   │   ├── images/
│   │   ├── 📄 App.razor         # Entry point
│   │   ├── 📄 Program.cs        # Client startup
│   │   ├── 📄 BlazorCMS.Client.csproj
│
├── 📂 shared/                  # Shared code
│   ├── 📂 BlazorCMS.Shared/     # Shared models & DTOs
│   │   ├── 📂 DTOs/             # Data transfer objects
│   │   │   ├── RegisterDTO.cs
│   │   │   ├── LoginDTO.cs
│   │   │   ├── BlogPostDTO.cs
│   │   │   ├── PageDTO.cs
│   │   ├── 📂 Models/           # Shared database models
│   │   │   ├── BlogPost.cs
│   │   │   ├── Page.cs
│   │   │   ├── ApplicationUser.cs
│   │   ├── 📂 Utilities/        # Helper utilities
│   │   │   ├── DateTimeHelper.cs
│   │   ├── 📄 DependencyInjection.cs
│   │   ├── 📄 BlazorCMS.Shared.csproj
│
├── 📂 ui/                      # UI components
│   ├── 📂 BlazorCMS.UIComponents/ # Reusable Blazor components
│   │   ├── 📂 Components/
│   │   │   ├── BlogList.razor
│   │   │   ├── BlogDetail.razor
│   │   │   ├── PageView.razor
│   │   │   ├── LoadingSpinner.razor
│   │   │   ├── Pagination.razor
│   │   ├── 📄 BlazorCMS.UIComponents.csproj



🚀 Summary
✅ Backend (BlazorCMS.API) → Handles authentication, blog, CMS APIs
✅ Data Layer (BlazorCMS.Data) → Manages database & repositories
✅ Infrastructure (BlazorCMS.Infrastructure) → Handles security, email, logging
✅ Admin Panel (BlazorCMS.Admin) → Manages blog & CMS content
✅ Public Website (BlazorCMS.Client) → Displays blogs & CMS pages
✅ Shared Models (BlazorCMS.Shared) → DTOs & models
✅ Reusable UI (BlazorCMS.UIComponents) → Custom Blazor components
Would you like a Dockerfile for deployment or unit tests for services next? 🚀



DROP TABLE IF EXISTS BlogPosts;

CREATE TABLE BlogPosts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    AuthorId TEXT NOT NULL,  -- Fixed: Ensuring NOT NULL
    Author TEXT NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    PublishedDate TEXT DEFAULT NULL,
    IsPublished INTEGER NOT NULL DEFAULT 0
);


