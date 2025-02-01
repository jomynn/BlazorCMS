Folder Structure for BlazorCMS.Client

BlazorCMS.Client/
│── 📂 Pages/                  # Public pages
│   │── Index.razor            # Home page (Latest Blogs)
│   │── Blog.razor             # Blog list page
│   │── BlogDetail.razor       # Single blog page
│   │── Page.razor             # CMS page
│   │── Login.razor            # User login page
│   │── Register.razor         # User registration page
│── 📂 Services/               # API services
│   │── BlogService.cs         # Fetch blog data
│   │── PageService.cs         # Fetch CMS pages
│   │── AuthService.cs         # Handles authentication
│── 📂 Components/             # Reusable UI components
│   │── Navbar.razor           # Website navbar
│   │── Footer.razor           # Website footer
│── 📂 Shared/                 # Shared UI layout
│   │── MainLayout.razor       # Main website layout
│── 📄 App.razor               # Entry point
│── 📄 Program.cs              # Configuration
│── 📄 BlazorCMS.Client.csproj # Project file
