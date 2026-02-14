using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Web.Areas.Admin.Controllers
    {
    [Area("Admin")]
    [Authorize] // CHANGE: Require authentication for the entire controller (Admin area).
    public class LocomotivesController : Controller
        {
        private readonly ILocomotiveService service;

        public LocomotivesController(ILocomotiveService service)
            {
            service = service ?? throw new ArgumentNullException(nameof(service)); // CHANGE: basic guard
            this.service = service;
            }

        // INDEX (Admin-only)
        public async Task<IActionResult> Index(string filter = "active")
            {
            // CHANGE: Admin full list with filter; anonymous users cannot access due to [Authorize].
            var model = await service.GetAll(filter);
            ViewData["CurrentFilter"] = filter;
            return View(model);
            }

        // DETAILS (Admin-only)
        public async Task<IActionResult> Details(int id, string filter)
            {
            var vm = await service.GetDetails(id);
            if (vm == null) return NotFound();

            ViewData["CurrentFilter"] = filter;
            return View(vm);
            }

        // CREATE GET (Admin-only)
        [HttpGet]
        public IActionResult Create(string filter)
            {
            ViewData["CurrentFilter"] = filter;
            return View(new LocomotiveFormModel());
            }

        // CREATE POST (Admin-only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocomotiveFormModel model, string filter)
            {
            if (!ModelState.IsValid)
                {
                ViewData["CurrentFilter"] = filter;
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            await service.CreateAsync(model, user);

            return RedirectToAction(nameof(Index), new { filter });
            }

        // EDIT GET (Admin-only)
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string filter)
            {
            var vm = await service.GetForEdit(id);
            if (vm == null) return NotFound();

            ViewData["CurrentFilter"] = filter;
            ViewData["EntityId"] = id;

            return View(vm);
            }

        // EDIT POST (Admin-only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LocomotiveFormModel model, string filter)
            {
            if (!ModelState.IsValid)
                {
                ViewData["CurrentFilter"] = filter;
                ViewData["EntityId"] = id;
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            await service.EditAsync(id, model, user);

            return RedirectToAction(nameof(Index), new { filter });
            }

        // DELETE (Admin-only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string filter)
            {
            var user = User?.Identity?.Name ?? "system";
            await service.DeleteAsync(id, user);

            return RedirectToAction(nameof(Index), new { filter });
            }

        // UNDELETE (Admin-only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undelete(int id, string filter)
            {
            var user = User?.Identity?.Name ?? "system";
            await service.UndeleteAsync(id, user);

            return RedirectToAction(nameof(Index), new { filter });
            }
        }
    }