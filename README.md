# Documentation for Using This Project

This is the **backend implementation** using **.NET Core 8**

## Related Projects

This backend is designed to work with multiple frontend and backend implementations:

- **Rust (Axum) version**: [fin-rustaxum](https://github.com/HairulDev/fin-rustaxum)
- **Vue.js version**: [fin-vuejs](https://github.com/HairulDev/fin-vuejs)
- **React (TypeScript) version**: [fin-reactts](https://github.com/HairulDev/fin-reactts)

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PostgreSQL
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

---

### Installation & Run

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
