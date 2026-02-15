using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Web.Controllers
    {
    [Authorize]
    public class FuelsController : BaseController
        {
        // Single set of dependencies (no duplicates)
        private readonly IFuelService fuelService;
        private readonly ILocomotiveService locoService;
        private readonly ILogger<FuelsController> logger;

        public FuelsController(
            IFuelService fuelService,
            ILocomotiveService locoService,
            ILogger<FuelsController> logger)
            {
            // Assign injected services
            this.fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService));
            this.locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        // GET: /Fuels/FuelReport
        public IActionResult FuelReport()
            {
            // Read-only report list
            var vm = fuelService.GetAll();
            return View(vm);
            }

        // Helper: populate locomotives dropdown (DTO -> SelectListItem)
        private async Task PopulateLocomotivesAsync()
            {
            // Use DTO-based service to keep Service.Core clean
            var options = await locoService.GetOptionsAsync();
            ViewBag.Locomotives = options
                .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Number })
                .ToList();
            }

        // GET: /Fuels/Create
        public async Task<IActionResult> Create()
            {
            // Provide defaults for new record + dropdown data
            await PopulateLocomotivesAsync();
            return View(fuelService.CreateModel());
            }

        // POST: /Fuels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFuelViewModel model)
            {
            // Basic server-side validation
            if (!ModelState.IsValid)
                {
                // Repopulate dropdown on validation failure
                await PopulateLocomotivesAsync();
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            try
                {
                await fuelService.CreateAsync(model, user);
                TempData["Success"] = "Fuel entry created.";
                return RedirectToAction(nameof(FuelReport));
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Create fuel failed");
                TempData["Error"] = "Failed to create fuel entry.";
                await PopulateLocomotivesAsync();
                return View(model);
                }
            }

        // GET: /Fuels/Edit/5
        public IActionResult Edit(int id)
            {
            // Load record for editing (service excludes soft-deleted)
            var vm = fuelService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
            }

        // POST: /Fuels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelAllViewModel model)
            {
            // Defense-in-depth: route id must match model id
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid) return View(model);

            var user = User?.Identity?.Name ?? "system";
            try
                {
                await fuelService.EditAsync(id, model, user);
                TempData["Success"] = "Fuel entry updated.";
                return RedirectToAction(nameof(FuelReport));
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Edit fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to update fuel entry.";
                return View(model);
                }
            }

        // POST: /Fuels/Delete/5  (soft-delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
            {
            var user = User?.Identity?.Name ?? "system";
            try
                {
                await fuelService.DeleteAsync(id, user);
                TempData["Success"] = "Fuel entry deleted.";
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Delete fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to delete fuel entry.";
                }
            return RedirectToAction(nameof(FuelReport));
            }

        // (Optional) POST: /Fuels/DeleteConfirmed/5 — keep only if your view posts to this action
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
            {
            var user = User?.Identity?.Name ?? "system";
            try
                {
                await fuelService.DeleteAsync(id, user);
                TempData["Success"] = "Fuel entry deleted.";
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "DeleteConfirmed fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to delete fuel entry.";
                }
            return RedirectToAction(nameof(FuelReport));
            }

        // POST: /Fuels/UndoDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoDelete(int id)
            {
            var user = User?.Identity?.Name ?? "system";
            try
                {
                await fuelService.UndoDeleteAsync(id, user);
                TempData["Success"] = "Fuel entry restored.";
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Undo delete fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to restore fuel entry.";
                }
            return RedirectToAction(nameof(FuelReport));
            }
        }
    }