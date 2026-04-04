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
  ![Public locomotives](./screenshots/public-locomotives.png)

## ✅ Authentication (Identity)
- Login and registration pages  
  ![Login](./screenshots/login.png)

## ✅ Home Dashboard
- Homepage for authenticated users  
  ![Home](./screenshots/home.png)

---

# 🚂 Locomotives Module
Located in **Admin Area** — full CRUD functionality.

### Admin Locomotives List  
  ![Admin locomotives](./screenshots/admin-locomotives.png)

### Create / Edit Locomotive  
  ![Locomotive form](./screenshots/locomotive-form.png)

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
![Fuel list](./screenshots/fuel-list.png)

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
![ShiftWorks](./screenshots/shiftworks-index.png)

---

# 🧱 Architecture Overview

