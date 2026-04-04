
# Локо — Railway Maintenance Tracker

## 🌍 Налични езици
- 🇧🇬 Български (този файл)
- 🇬🇧 English — виж `README.md`

---
# 📘 Описание на проекта
**Локо** е ASP.NET Core MVC приложение за управление на локомотиви, разход на гориво, осови измервания и работни смени.  
Проектът демонстрира:

- Чиста многослойна архитектура  
- Entity Framework Core + SQL Server  
- ASP.NET Core Identity и Администраторска зона  
- SOLID принципи и ясна структурираност  
- Филтриране, странициране и валидации  
- Unit тестване с високо покритие  
- CI/CD чрез GitHub Actions  

Разработено за **SoftUni ASP.NET Advanced Exam (Aприл 2026)**.

---
# 🎯 Основни функционалности

## ✅ Публична зона
- Гостите (анонимни потребители) могат да виждат публичните локомотиви  
  ./screenshots/public-locomotives.png

## ✅ Автентикация (Identity)
- Login и Register страници  
  ./screenshots/login.png

## ✅ Начално табло (Home)
- Стартова страница за вписани потребители  
  ./screenshots/home.png

---
# 🚂 Модул „Локомотиви“
Намира се в **Admin Area** — пълен CRUD.

### Списък локомотиви (Админ)  
  ./screenshots/admin-locomotives.png

### Създаване / Редакция на локомотив  
  ./screenshots/locomotive-form.png

Възможности:
- Създаване, редакция, изтриване  
- Силни валидации  
- Филтриране по статус (Active / All / Deleted)  
- Добре структурирани ViewModels  

---
# ⛽ Модул „Гориво“
- Добавяне на нови записни точки за гориво  
- Списък и детайли  
- Автоматично изчисляване на дневен разход  
- Валидации на начални/крайни стойности  

### Списък с горивни записи  
./screenshots/fuel-list.png

---
# 🕓 Модул „Работни смени“ (Shift Work)
Следи дневните показания Km/Mh.

Функции:
- Филтър по номер на локомотив  
- Филтър по дати  
- Избор на странициране  
- Pagination  
- Автоматично изчисляване на Total  

### ShiftWorks Overview  
./screenshots/shiftworks-index.png


# 🔧 Модул „Измерване на колооси“

Модулът позволява създаване, редактиране и преглед на технически измервания на колоосите за всеки локомотив.  
Системата използва динамични UI компоненти, AJAX зареждане и пълна сървърна валидация.

### Създаване на карта за измерване на колооси  
./screenshots/axle_create.png

### Редактиране на карта за измерване  
./screenshots/axle_edit.png

## ✅ Функционалности

- Автоматично зареждане на точния брой колооси според избрания локомотив  
- AJAX заявка (`GetAxleInputs`) – таблицата се презарежда без рефреш на страницата  
- Пълна сървърна валидация на всички полета  
- Автоматично изчисление на SR (AR + SD_L + SD_R) на сървъра  
- Премахване на празните редове преди запис в базата  
- Генериране на документен номер (Година + Серия)  
- CRUD: списък, детайли, създаване, редактиране  
- Достъп според права (Repairs.View / Repairs.Create)

## ✅ Технически бележки

- Използва две EF Core ентита:
  - `AxleMeasurementCard`
  - `AxleMeasurementValue`
- Конфигурация чрез Fluent API (прецизност, подредба, релации)
- Сервиз: `AxleMeasurementService` с ясно разделена логика  
- View модели, оптимизирани за Razor формуляри  
- Unobtrusive validation работи и при динамично зареждан HTML (AJAX)

## ✅ Възможни бъдещи подобрения

- Автоматично изчисление на SR в реално време (клиентски JS)  
- Inline визуална валидация при въвеждане  
- Динамично добавяне/премахване на редове  
- Графики и тренд анализ за исторически измервания
---
# 🧱 Архитектура

```
GCommon/                 → Общи енумерации, константи, съобщения
NewLoco.Data/            → DbContext, миграции, конфигурации
NewLoco.Data.Models/     → Entity модели
NewLoco.Service.Core/    → Сервизи + интерфейси (бизнес логика)
NewLoco.Web/             → Контролери, Areas/Admin, Identity, изгледи
NewLoco.Web.ViewModels/  → View модели
NewLoco.TestS/           → Unit тестове (xUnit + Moq + InMemory EF)
```

---
# 🧩 Основни Entity модели

Проектът включва повече от изискваните 5 модела:

- `Locomotive`
- `Fuel`
- `ShiftWork`
- `AxleMeasurementCard`
- `AxleMeasurementValue`
- `ApplicationUser` / `ApplicationRole`

Всички модели използват:
- Data annotations
- Fluent API конфигурации
- Enum стойности
- Валидации по условие

---
# 🔐 Сигурност и права
Системата използва:
- ASP.NET Core Identity  
- Admin Area за администратори  
- Custom Authorization Policies  
- Permission‑based достъп  
- AntiForgeryToken за POST заявки  
- HTML Encoding срещу XSS  

---
# 🔍 Филтриране и странициране
Реализирани изцяло според изискванията на SoftUni:
- ShiftWorks: пълно филтриране + paging  
- Admin Locomotives: статус филтър  
- Fuel: филтриране и логическа валидация  

---
# 🧪 Unit тестове и покритие
Проектът съдържа unit тестове за:
- `LocomotiveService`  
- `FuelService`  
- `ShiftWorkService`  
- `AxleMeasurementService`  
- `FuelEstimator`  

Използвани технологии:
- xUnit  
- Moq  
- InMemory база  

### ✅ Code Coverage (ReportGenerator):
- **Line coverage:** ~86%  
- **Branch coverage:** ~72%  

./screenshots/coverage.png

Това надвишава изискването на SoftUni (минимум 65%).

---
# 🔄 CI/CD (GitHub Actions)
- Автоматичен build  
- Автоматични тестове  

Файл:  
```
.github/workflows/dotnet.yml
```

---
# 🚀 Инсталация и стартиране

### 1️⃣ Клониране
```bash
git clone https://github.com/lzl70110/NewLoco2026_2
```

### 2️⃣ Миграции
```bash
dotnet ef database update
```

### 3️⃣ Стартиране
```bash
dotnet run --project NewLoco.Web
```

Изисква локално работещ **SQL Server**.

---
# ✅ SoftUni — Проверка на изискванията

| Изискване | Статус |
|-----------|--------|
| 10+ изгледа | ✅ |
| 5+ модела | ✅ |
| 5+ контролера | ✅ |
| Identity | ✅ |
| Admin Area | ✅ |
| SQL Server + EF Core | ✅ |
| Валидации | ✅ |
| Pagination | ✅ |
| Filtering | ✅ |
| Error Pages (401/403/404) | ✅ |
| Unit Tests ≥ 65% | ✅ (86%) |
| GitHub история (30+ комита) | ✅ |
| README документация | ✅ |

---
# ⚠️ Дисклеймър
Този проект е оригинална разработка, създадена специално за **SoftUni ASP.NET Advanced Exam (Aприл 2026)**.  
Не е използван код от лекции, упражнения, други студенти или външни ресурси.

---
# 📄 Лиценз
Apache License 2.0  
Само за учебни цели.
