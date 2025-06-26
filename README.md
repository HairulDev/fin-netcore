# Project Documentation

This repository contains documentation and references for both the **backend** (.NET Core 8) and **backend** (Rust Axum for upload/download file) and **frontend** (Vue.js or React.js)

---

## Related Projects

- **.NET Core 8 (Backend)**: [fin-netcore](https://github.com/HairulDev/fin-netcore)
- **Vue.js (Frontend)**: [fin-vuejs](https://github.com/HairulDev/fin-vuejs)
- **Rust (Axum) Version**: [fin-rustaxum](https://github.com/HairulDev/fin-rustaxum)
- **React (TypeScript) Version**: [fin-reactts](https://github.com/HairulDev/fin-reactts)

---

## Prerequisites

Ensure the following tools are installed:

- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

---

## Installation & Run

- **Change ConnectionStrings in appsettings.json**

```bash

# Instal EF CLI
dotnet tool install --global dotnet-ef

# Applying migrations
dotnet ef database update

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
