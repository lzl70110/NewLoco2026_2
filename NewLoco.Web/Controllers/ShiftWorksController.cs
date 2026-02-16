using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.ShiftWorks;
using System;

namespace NewLoco.Web.Controllers
    {
    [Authorize]
    public class ShiftWorksController : BaseController
        {
        private readonly IShiftWorkService shiftService;
        private readonly ILocomotiveService locoService;
        private readonly ILogger<ShiftWorksController> logger;

        public ShiftWorksController(
            IShiftWorkService shiftService,
            ILocomotiveService locoService,
            ILogger<ShiftWorksController> logger)
            {
            this.shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
            this.locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        private async Task PopulateLocomotivesAsync()
            {
            var options = await locoService.GetOptionsAsync();
            ViewBag.Locomotives = options
                .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Number })
                .ToList();
            }

        public IActionResult Index()
            {
            var vm = shiftService.GetAll();
            return View(vm);
            }

        public async Task<IActionResult> Create()
            {
            await PopulateLocomotivesAsync();
            return View(shiftService.CreateModel());
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShiftWorkViewModel model)
            {
            if (!ModelState.IsValid)
                {
                await PopulateLocomotivesAsync();
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            try
                {
                await shiftService.CreateAsync(model, user);
                TempData["Success"] = "ShiftWork created.";
                return RedirectToAction(nameof(Index));
                }
            catch (InvalidOperationException ex)
                {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateLocomotivesAsync();
                return View(model);
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Create ShiftWork failed");
                TempData["Error"] = "Failed to create shift work.";
                await PopulateLocomotivesAsync();
                return View(model);
                }
            }

        public IActionResult Edit(int id)
            {
            var vm = shiftService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditShiftWorkViewModel model)
            {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid) return View(model);

            var user = User?.Identity?.Name ?? "system";
            try
                {
                await shiftService.EditAsync(id, model, user);
                TempData["Success"] = "ShiftWork updated.";
                return RedirectToAction(nameof(Index));
                }
            catch (InvalidOperationException ex)
                {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Edit ShiftWork failed for id {Id}", id);
                TempData["Error"] = "Failed to update shift work.";
                return View(model);
                }
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
            {
            var user = User?.Identity?.Name ?? "system";
            try
                {
                await shiftService.DeleteAsync(id, user);
                TempData["Success"] = "ShiftWork deleted.";
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Delete ShiftWork failed for id {Id}", id);
                TempData["Error"] = "Failed to delete shift work.";
                }
            return RedirectToAction(nameof(Index));
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoDelete(int id)
            {
            var user = User?.Identity?.Name ?? "system";
            try
                {
                await shiftService.UndoDeleteAsync(id, user);
                TempData["Success"] = "ShiftWork restored.";
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Undo delete ShiftWork failed for id {Id}", id);
                TempData["Error"] = "Failed to restore shift work.";
                }
            return RedirectToAction(nameof(Index));
            }
        }
    }