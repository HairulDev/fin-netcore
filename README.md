# Project Documentation

This repository contains documentation and references for both the **backend** (.NET Core 8) and **backend** (Rust Axum for upload/download file) 
and **backend** (Python FastAPI & Pytorch) and **frontend** ([Main: Vue.js] or [Optional: React.js or Angular.js])

---

## Demo

Watch the demo on YouTube: [https://youtu.be/o0NqKYBCWwE?si=TkaWGOZroBRefx9s]

## Related Projects

Main:
- **.NET Core 8 (Backend)**: [fin-netcore](https://github.com/HairulDev/fin-netcore)
- **Python FastAPI Version**: [fin-fastapi](https://github.com/HairulDev/fin-fastapi)
- **Vue.js (Frontend)**: [fin-vuejs](https://github.com/HairulDev/fin-vuejs)
- **Rust (Axum) Version**: [fin-rustaxum](https://github.com/HairulDev/fin-rustaxum)

Optional:
- **React (TypeScript) Version**: [fin-reactts](https://github.com/HairulDev/fin-reactts)
- **Angular (TypeScript) Version**: [fin-angular](https://github.com/HairulDev/fin-angular)

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

# Genereate random key for SigningKey value in appsettings.json
openssl rand -base64 64

```

- Create a `.env` file in the project root directory and add the necessary environment variables:

```env

DB_HOST=
DB_DATABASE=
DB_USERNAME=
DB_PASSWORD=
FMPKey=your_api_key (go to https://financialmodelingprep.com)

```


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
dotnet watch run
