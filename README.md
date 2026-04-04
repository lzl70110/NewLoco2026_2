
# Loko — Railway Maintenance Tracker

## 🌍 Available Languages
- 🇬🇧 English (this file)  
- 🇧🇬 Bulgarian — see `README_BG.md`

---
# 📘 Project Overview
**Loko** is an ASP.NET Core MVC application designed to manage locomotives, fuel usage, axle measurements, and daily shift work entries.  
The project demonstrates:

- Clean multi-layered architecture  
- Entity Framework Core with SQL Server  
- ASP.NET Core Identity authentication & Admin Area  
- SOLID-compliant services  
- Pagination, filtering, validation  
- Unit Testing with high coverage  
- CI/CD through GitHub Actions  

Developed for the **SoftUni ASP.NET Advanced Exam (April 2026).**

---
# 🎯 Main Features

## ✅ Public Area
- Anonymous users can view public locomotives  
  ./screenshots/public-locomotives.png

## ✅ Authentication (Identity)
- Login and registration pages  
  ./screenshots/login.png

## ✅ Home Dashboard
- Homepage for authenticated users  
  ./screenshots/home.png

---
# 🚂 Locomotives Module
Located in **Admin Area** — full CRUD functionality.

### Admin Locomotives List  
  ./screenshots/admin-locomotives.png

### Create / Edit Locomotive  
  ./screenshots/locomotive-form.png

Features:
- Create, edit, delete locomotives  
- Strong validation  
- Status filtering (Active / All / Deleted)  
- Structured ViewModels  

---
# ⛽ Fuel Module
Includes:
- New fuel input  
- Listing and details  
- Consumption calculation  
- Validation of start/end values  

### Fuel Records List  
./screenshots/fuel-list.png

---
# 🕓 Shift Work Module
Tracks locomotive daily Km/Mh usage.

Features:
- Date range filtering  
- Search by locomotive number  
- Page size selection  
- Pagination  
- Automatic "Total" calculation  

### ShiftWork Overview  
./screenshots/shiftworks-index.png

---
# 🧱 Architecture Overview

```
GCommon/                 → Shared enums, constants, messages
NewLoco.Data/            → DbContext, migrations, configurations
NewLoco.Data.Models/     → Entity models
NewLoco.Service.Core/    → Business services + interfaces
NewLoco.Web/             → Controllers, Areas/Admin, Identity, Views
NewLoco.Web.ViewModels/  → View models for Razor pages
NewLoco.TestS/           → xUnit tests + Moq + InMemory EF
```

Built with strong separation of concerns and SOLID design principles.

---
# 🧩 Main Entity Models

The project includes more than the required minimum of 5 entities:

- `Locomotive`
- `Fuel`
- `ShiftWork`
- `AxleMeasurementCard`
- `AxleMeasurementValue`
- `ApplicationUser` / `ApplicationRole` (Identity)

Entities use:
- Data annotations  
- Fluent API configurations  
- Enum mappings  
- Validation rules  

---
# 🔐 Security & Authorization

The system uses:
- ASP.NET Core Identity  
- Admin Area for privileged operations  
- Custom Authorization Policies  
- Permissions-based access  
- AntiForgeryToken on all POST forms  
- Sanitized and HTML-encoded user inputs  

---
# 🔍 Pagination & Filtering

Required by SoftUni and implemented as:
- ShiftWorks: full filtering + paging  
- Admin Locomotives: status filtering  
- Fuel: filtering and data validation  

---
# 🧪 Unit Tests & Coverage

The project includes unit tests for all core services:

- `LocomotiveService`  
- `FuelService`  
- `ShiftWorkService`  
- `AxleMeasurementService`  
- `FuelEstimator`  

Using:
- xUnit  
- Moq  
- InMemory DbContext  

### ✅ Code Coverage (ReportGenerator):

- **Line coverage:** ~86%  
- **Branch coverage:** ~72%  
- Above SoftUni requirement (≥ 65%)  

./screenshots/coverage.png

---
# 🔄 CI/CD (GitHub Actions)

Automated pipeline includes:
- `dotnet restore`  
- `dotnet build`  
- `dotnet test`  

Located at:
```
.github/workflows/dotnet.yml
```

---
# 🚀 Installation & Setup

### 1️⃣ Clone the repository
```bash
git clone https://github.com/lzl70110/NewLoco2026_2
```

### 2️⃣ Apply database migrations
```bash
dotnet ef database update
```

### 3️⃣ Run the application
```bash
dotnet run --project NewLoco.Web
```

Requires **SQL Server** running locally.

---
# ✅ SoftUni Exam Requirements Checklist

| Requirement | Status |
|------------|--------|
| 10+ Views | ✅ |
| 5+ Entities | ✅ |
| 5+ Controllers | ✅ |
| Identity Authentication | ✅ |
| Admin Area | ✅ |
| SQL Server + EF Core + Migrations | ✅ |
| Validations | ✅ |
| Pagination | ✅ |
| Filtering/Search | ✅ |
| Error Pages (401/403/404) | ✅ |
| Unit Tests ≥ 65% | ✅ (86%) |
| GitHub history (30+ commits) | ✅ |
| Documentation (README) | ✅ |

---
# ⚠️ Disclaimer
This project is original work, created specifically for the **SoftUni ASP.NET Advanced Exam (April 2026)**.  
No code was copied from lectures, demos, other students, or external sources.

---
# 📄 License
Apache License 2.0  
For educational use only.
