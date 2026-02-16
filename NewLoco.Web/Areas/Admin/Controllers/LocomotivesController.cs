using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Web.Areas.Admin.Controllers
    {
    [Area("Admin")]
    [Authorize] // Require authentication for Admin area
    public class LocomotivesController : Controller
        {
        private readonly ILocomotiveService service;

        public LocomotivesController(ILocomotiveService service)
            {
            
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            }

        // GET: /Admin/Locomotives?filter=active|all|deleted
        public async Task<IActionResult> Index(string? filter = "active")
            {
            var dtos = await service.GetAllAsync(filter);
            
            var model = dtos.Select(static d => new LocomotiveNumberViewModel
                {
                Id = d.Id,
                Number = d.Number,
                LocomotiveType = d.LocomotiveType,
                MeasuringUnit = d.MeasuringUnit,
                Note = d.Note?? "",
                IsDeleted = d.IsDeleted
                }).ToList();

            ViewData["CurrentFilter"] = filter;
            if (!ModelState.IsValid)
                return NotFound();
            return View(model);
            }

        // GET: /Admin/Locomotives/Details/5
        public async Task<IActionResult> Details(int id, string? filter)
            {
            var dto = await service.GetDetailsAsync(id);
            if (dto == null) return NotFound();

            ViewData["CurrentFilter"] = filter;
            return View(ToVm(dto)); // DTO -> VM
            }

        // GET: /Admin/Locomotives/Create
        [HttpGet]
        public IActionResult Create(string? filter)
            {
            ViewData["CurrentFilter"] = filter;
            return View(new LocomotiveFormModel());
            }

        // POST: /Admin/Locomotives/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocomotiveFormModel model, string? filter)
            {
            if (!ModelState.IsValid)
                {
                ViewData["CurrentFilter"] = filter;
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            await service.CreateAsync(ToDto(model), user); // VM -> DTO

            return RedirectToAction(nameof(Index), new { filter });
            }

        // GET: /Admin/Locomotives/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? filter)
            {
            var dto = await service.GetForEditAsync(id);
            if (dto == null) return NotFound();

            ViewData["CurrentFilter"] = filter;
            ViewData["EntityId"] = id;

            return View(ToVm(dto)); // DTO -> VM
            }

        // POST: /Admin/Locomotives/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LocomotiveFormModel model, string? filter)
            {
            if (!ModelState.IsValid)
                {
                ViewData["CurrentFilter"] = filter;
                ViewData["EntityId"] = id;
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            await service.EditAsync(id, ToDto(model), user); // VM -> DTO

            return RedirectToAction(nameof(Index), new { filter });
            }

        // POST: /Admin/Locomotives/Delete/5  (soft-delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? filter)
            {
            var user = User?.Identity?.Name ?? "system";
            await service.DeleteAsync(id, user);

            return RedirectToAction(nameof(Index), new { filter });
            }

        // POST: /Admin/Locomotives/Undelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undelete(int id, string? filter)
            {
            var user = User?.Identity?.Name ?? "system";
            await service.UndeleteAsync(id, user);

            return RedirectToAction(nameof(Index), new { filter });
            }

        // -----------------
        // Mapping helpers:
        // -----------------

        // DTO -> VM (list row)
        private static LocomotiveNumberViewModel ToVm(LocoNumberDto dto)
            => new LocomotiveNumberViewModel
                {
                Id = dto.Id,
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,   // enum -> enum (see note below if your VM uses string)
                MeasuringUnit = dto.MeasuringUnit,    // enum -> enum
                IsDeleted = dto.IsDeleted
                };

        // DTO -> VM (details)
        private static LocomotiveDetailsViewModel ToVm(LocoDetailsDto dto)
            => new LocomotiveDetailsViewModel
                {
                Id = dto.Id,
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,   // enum -> enum
                MeasuringUnit = dto.MeasuringUnit,    // enum -> enum
                Note = dto.Note ?? string.Empty,
                IsDeleted = dto.IsDeleted,
                CreatedOn = dto.CreatedOn,
                CreatedBy = dto.CreatedBy!,
                ModifiedOn = dto.ModifiedOn,
                ModifiedBy = dto.ModifiedBy
                };

        // DTO -> VM (edit form)
        private static LocomotiveFormModel ToVm(LocomotiveFormDto dto)
            => new LocomotiveFormModel
                {
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,   // enum -> enum
                MeasuringUnit = dto.MeasuringUnit,    // enum -> enum
                Note = dto.Note
                };

        // VM -> DTO (create/edit)
        private static LocomotiveFormDto ToDto(LocomotiveFormModel vm)
            => new LocomotiveFormDto(
                vm.Number,
                vm.LocomotiveType,   // enum -> enum
                vm.MeasuringUnit,    // enum -> enum
                vm.Note
            );
        }
    }