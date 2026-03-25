using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;   // <-- for ModelStateDictionary
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth; // Perm constants
using NewLoco.Web.ViewModels.Fuels;

namespace NewLoco.Web.Controllers
{
    // Class-level guard: anyone reaching Fuels must have 'read' permission
    [Authorize(Policy = Perm.Fuel.View)]
    public class FuelsController(
        IFuelService fuelService,
        ILocomotiveService locoService,
        ILogger<FuelsController> logger) : BaseController
    {
        // Init backing fields via primary-ctor parameters
        private readonly IFuelService _fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService));
        private readonly ILocomotiveService _locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
        private readonly ILogger<FuelsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Slim list view (Locomotive, Date, Initial, Final)
        [HttpGet]
        public IActionResult Index()
        {
            var vm = _fuelService.GetForIndexLatest();
            return View(vm);
        }

        // Full details view of a single fuel record
        [HttpGet]
        public IActionResult Details(int id)
        {
            var vm = _fuelService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // Full report (wide dataset)
        [HttpGet]
        public IActionResult FuelReport()
        {
            var vm = _fuelService.GetAll();
            return View(vm);
        }

        private async Task PopulateLocomotivesAsync()
        {
            var options = await _locoService.GetOptionsAsync();
            ViewBag.Locomotives = options
                .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Number })
                .ToList();
        }

        // Create
        [HttpGet]
        [Authorize(Policy = Perm.Fuel.Create)]
        public async Task<IActionResult> Create()
        {
            await PopulateLocomotivesAsync();
            return View(_fuelService.CreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Fuel.Create)]
        public async Task<IActionResult> Create(CreateFuelViewModel model)
        {
            // Ignore client-provided computed fields; server will compute them
            ModelState.Remove(nameof(CreateFuelViewModel.InitialFuel));
            ModelState.Remove(nameof(CreateFuelViewModel.Consumption));

            // If the Create view posts other read-only fields, remove them too (defensive):
            ModelState.Remove("LocomotiveNumber");
            ModelState.Remove("CreatedOn");
            ModelState.Remove("CreatedByUserName");
            ModelState.Remove("EditedBy");
            ModelState.Remove("EditedOn");

            // IMPORTANT: Clear stale errors and re-validate on the server
            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                LogModelStateErrors(ModelState);
                await PopulateLocomotivesAsync();
                return View(model);
            }

            var user = User?.Identity?.Name ?? "system";
            try
            {
                await _fuelService.CreateAsync(model, user);
                TempData["Success"] = "Fuel entry created.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // domain validation (10L step, hard floor, etc.)
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateLocomotivesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create fuel failed");
                TempData["Error"] = "Failed to create fuel entry.";
                await PopulateLocomotivesAsync();
                return View(model);
            }
        }

        // Edit
        [HttpGet]
        [Authorize(Policy = Perm.Fuel.Edit)]
        public IActionResult Edit(int id)
        {
            var vm = _fuelService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Fuel.Edit)]
        public async Task<IActionResult> Edit(int id, FuelAllViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // remove non-editable keys from ModelState
            string[] nonEditableKeys =
            [
                nameof(FuelAllViewModel.LocomotiveNumber),
                nameof(FuelAllViewModel.InitialFuel),
                nameof(FuelAllViewModel.Consumption),
                nameof(FuelAllViewModel.IsDeleted),
                "CreatedOn", "CreatedByUserName", "EditedBy", "EditedOn"
            ];
            foreach (var key in nonEditableKeys) ModelState.Remove(key);

            // Re-validate server-side to drop stale errors (optional but consistent)
            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                LogModelStateErrors(ModelState);
                return View(model);
            }

            var user = User?.Identity?.Name ?? "system";
            try
            {
                await _fuelService.EditAsync(id, model, user);
                TempData["Success"] = "Fuel entry updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to update fuel entry.";
                return View(model);
            }
        }

        // Delete / Undo delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Fuel.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = User?.Identity?.Name ?? "system";
            try
            {
                await _fuelService.DeleteAsync(id, user);
                TempData["Success"] = "Fuel entry deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to delete fuel entry.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Fuel.Delete)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = User?.Identity?.Name ?? "system";
            try
            {
                await _fuelService.DeleteAsync(id, user);
                TempData["Success"] = "Fuel entry deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteConfirmed fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to delete fuel entry.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.Fuel.Delete)]
        public async Task<IActionResult> UndoDelete(int id)
        {
            var user = User?.Identity?.Name ?? "system";
            try
            {
                await _fuelService.UndoDeleteAsync(id, user);
                TempData["Success"] = "Fuel entry restored.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undo delete fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to restore fuel entry.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Utility for Create form (prev final fuel) — keep 'read' guard from class-level
        [HttpGet]
        public async Task<IActionResult> PrevFinal(int locoId, string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return Json(new { value = 0m });

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                if (!DateTime.TryParse(date, CultureInfo.GetCultureInfo("bg-BG"), DateTimeStyles.None, out parsed))
                    return Json(new { value = 0m });
            }

            var value = await _fuelService.GetPrevFinalAsync(locoId, parsed);
            return Json(new { value });
        }

        // --- helper: log invalid ModelState reasons ---
        private void LogModelStateErrors(ModelStateDictionary ms)
        {
            var errors = ms
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value!.Errors.Select(e => e.ErrorMessage))}");
            _logger.LogWarning("Fuel Create/Edit - ModelState invalid. Errors: {Errors}", string.Join(" | ", errors));
        }
    }
}