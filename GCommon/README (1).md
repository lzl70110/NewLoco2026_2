
# Loko — Railway Maintenance Tracker

## 🌍 Available Languages
- 🇬🇧 English (this file)
- 🇧🇬 Bulgarian — see README_BG.md

---

# 📘 Project Overview
Loko is an ASP.NET Core MVC web application designed for managing locomotives, fuel records, and daily shift work entries that track start and end meter readings (Km/Mh). The project is developed for the **SoftUni ASP.NET Fundamentals Exam** and demonstrates a clean MVC architecture, Identity authentication, CRUD operations, EF Core data access, and solid structuring suitable for future extension.

The system currently supports **English only**. Localization (bg-BG) is planned as a future improvement.

---

# 🎯 Main Features
- **Locomotives module** — full CRUD (Admin area available, no role restrictions yet)
- **Fuel module** — create, edit, delete, list fuel records + fuel report view
- **Shift Work module** — track start/end Km/Mh and auto‑calculate daily usage
- **Public locomotive list** available for all anonymous visitors
- **Full application access** for authenticated users
- **Identity login & register** (without role-based authorization)
- Additional tools: **Calculator**, **Calendar**

---

# 🏗️ Technologies Used
- **ASP.NET Core MVC** (.NET 8)
- **Entity Framework Core** + SQL Server
- **ASP.NET Core Identity** (authentication only)
- **Bootstrap 5** for responsive UI
- **Razor Views**, Layouts, Partials
- **Dependency Injection** architecture

---

# 🔐 Authentication & Authorization
The application uses ASP.NET Core Identity.

### Anonymous users:
- Can view only the **public locomotive list**

### Authenticated users:
- Can **read and write in the entire application** (for now)
- No roles are implemented yet → all logged users have equal permissions

---

# 🚀 Installation & Setup
1. Clone the repository:
```bash
git clone https://github.com/lzl70110/NewLoco2026_2
```
2. Navigate to the project directory:
```bash
cd NewLoco2026_2
```
3. Ensure SQL Server is running.
4. Apply EF Core migrations:
```bash
dotnet ef database update
```
5. Run the application:
```bash
dotnet run --project NewLoco.Web
```

---

# 🗂️ Project Structure (High-Level)
```
GCommon/                     → Shared enums & validation constants
NewLoco.Data/                → DbContext, Configurations
NewLoco.Data.Models/         → Entity models
NewLoco.Service.Core/        → Services + Interfaces
NewLoco.Web/                 → MVC layer, Controllers, Views, Identity
NewLoco.Web.ViewModels/      → Strongly-typed ViewModels
```

---

# 📈 Future Improvements
- Localization support (bg-BG)
- Role-based authorization
- Additional statistics & reporting

---

# 📝 SoftUni Exam Compliance
This project follows the official ASP.NET Fundamentals requirements:
- MVC with Controllers, Models, Views
- EF Core + SQL Server + Migrations
- CRUD for main entities
- Identity authentication
- Clean code structure and SOLID principles
- Public GitHub repository with multiple commits
- Basic documentation included

---

# ⚠️ Disclaimer
This project is **original work**, created specifically for the SoftUni exam. No code, HTML, CSS, or logic has been copied from SoftUni lectures, demos, workshops, or other students.

---

# 📄 License
Educational use only.
