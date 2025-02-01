Folder Structure

BlazorCMS.Host/
│── 📂 Pages/                    # Entry points for Blazor Server & WebAssembly
│   │── _Host.cshtml             # Hosts the Blazor WebAssembly app
│   │── Admin.razor              # Entry for Blazor Server Admin
│── 📂 Services/                 # Shared Services
│   │── HttpClientService.cs     # Provides a centralized HTTP client
│── 📂 wwwroot/                  # Static files
│   │── index.html               # Default Blazor WebAssembly index file
│── 📄 Program.cs                # Entry point of BlazorCMS.Host
│── 📄 BlazorCMS.Host.csproj     # Project file
