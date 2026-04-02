using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = Perm.Locomotive.View)]
    public class LocomotivesController : Controller
    {
        private readonly ILocomotiveService service;

        public LocomotivesController(ILocomotiveService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // GET: /Admin/Locomotives?filter=active|all|deleted
        [HttpGet]
        public async Task<IActionResult> Index(string? filter = "active")
        {
            var dtos = await service.GetAllAsync(filter);

            var model = dtos.Select(d => new LocomotiveNumberViewModel
            {
                Id = d.Id,
                Number = d.Number,
                LocomotiveType = d.LocomotiveType,
                MeasuringUnit = d.MeasuringUnit,
                AxlesCount = d.AxlesCount,     // ✔ FIXED
                Note = d.Note ?? "",
                IsDeleted = d.IsDeleted
            }).ToList();

            ViewData["CurrentFilter"] = filter;
            return View(model);
        }

        // GET: /Admin/Locomotives/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id, string? filter)
        {
            var dto = await service.GetDetailsAsync(id);
            if (dto == null) return NotFound();

            ViewData["CurrentFilter"] = filter;
            return View(ToVm(dto));    // ✔ FIXED mapping below
        }

        // GET: /Admin/Locomotives/Create
        [HttpGet]
        [Authorize(Policy = Perm.Locomotive.Create)]
        public IActionResult Create(string? filter)
        {
            ViewData["CurrentFilter"] = filter;
            return View(new LocomotiveFormModel());
        }

        // POST: /Admin/Locomotives/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Locomotive.Create)]
        public async Task<IActionResult> Create(LocomotiveFormModel model, string? filter)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CurrentFilter"] = filter;
                return View(model);
            }

            var user = User?.Identity?.Name ?? "system";
            await service.CreateAsync(ToDto(model), user);   // ✔ FIXED VM → DTO mapping

            return RedirectToAction(nameof(Index), new { filter });
        }

        // GET: /Admin/Locomotives/Edit/5
        [HttpGet]
        [Authorize(Policy = Perm.Locomotive.Edit)]
        public async Task<IActionResult> Edit(int id, string? filter)
        {
            var dto = await service.GetForEditAsync(id);
            if (dto == null) return NotFound();

            ViewData["CurrentFilter"] = filter;
            ViewData["EntityId"] = id;

            return View(ToVm(dto));   // ✔ FIXED mapping below
        }

        // POST: /Admin/Locomotives/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Locomotive.Edit)]
        public async Task<IActionResult> Edit(int id, LocomotiveFormModel model, string? filter)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CurrentFilter"] = filter;
                ViewData["EntityId"] = id;
                return View(model);
            }

            var user = User?.Identity?.Name ?? "system";
            await service.EditAsync(id, ToDto(model), user);   // ✔ FIXED VM → DTO

            return RedirectToAction(nameof(Index), new { filter });
        }

        // POST: /Admin/Locomotives/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Locomotive.Delete)]
        public async Task<IActionResult> Delete(int id, string? filter)
        {
            var user = User?.Identity?.Name ?? "system";
            await service.DeleteAsync(id, user);

            return RedirectToAction(nameof(Index), new { filter });
        }

        // POST: /Admin/Locomotives/Undelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Locomotive.Delete)]
        public async Task<IActionResult> Undelete(int id, string? filter)
        {
            var user = User?.Identity?.Name ?? "system";
            await service.UndeleteAsync(id, user);

            return RedirectToAction(nameof(Index), new { filter });
        }

        // -------------------------------
        // Mapping Helpers  (ALL FIXED)
        // -------------------------------

        // DETAILS: DTO -> VM
        private static LocomotiveDetailsViewModel ToVm(LocoDetailsDto dto)
            => new()
            {
                Id = dto.Id,
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,
                MeasuringUnit = dto.MeasuringUnit,
                AxlesCount = dto.AxlesCount,     // ✔ FIXED
                Note = dto.Note ?? string.Empty,
                IsDeleted = dto.IsDeleted,
                CreatedOn = dto.CreatedOn,
                CreatedBy = dto.CreatedBy!,
                ModifiedOn = dto.ModifiedOn,
                ModifiedBy = dto.ModifiedBy
            };

        // EDIT FORM: DTO -> VM
        private static LocomotiveFormModel ToVm(LocomotiveFormDto dto)
            => new()
            {
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,
                MeasuringUnit = dto.MeasuringUnit,
                AxlesCount = dto.AxlesCount,      
                Note = dto.Note
            };

        // CREATE/EDIT: VM -> DTO
        private static LocomotiveFormDto ToDto(LocomotiveFormModel vm)
            => new(
                vm.Number,
                vm.LocomotiveType,
                vm.MeasuringUnit,
                vm.AxlesCount,                   
                vm.Note
            );
    }
}
 