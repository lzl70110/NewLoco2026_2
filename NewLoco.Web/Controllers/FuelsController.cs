using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Fuels;
using System.Globalization;

namespace NewLoco.Web.Controllers
{
    [Authorize]
    public class FuelsController(
        IFuelService fuelService,
        ILocomotiveService locoService,
        ILogger<FuelsController> logger) : BaseController
    {
        // use primary-ctor parameters to initialize backing fields
        private readonly IFuelService _fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService));
        private readonly ILocomotiveService _locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
        private readonly ILogger<FuelsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // NEW: slim list view (Locomotive, Date, Initial, Final)

        public IActionResult Index()
        {
            var vm = _fuelService.GetForIndexLatest();
            return View(vm);
        }


        // NEW: full details view of a single fuel record
        public IActionResult Details(int id)
        {
            var vm = _fuelService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // kept: full report (wide dataset)
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

        public async Task<IActionResult> Create()
        {
            await PopulateLocomotivesAsync();
            return View(_fuelService.CreateModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFuelViewModel model)
        {
            // ignore client-provided computed fields; server will compute them
            ModelState.Remove(nameof(CreateFuelViewModel.InitialFuel));
            ModelState.Remove(nameof(CreateFuelViewModel.Consumption));

            if (!ModelState.IsValid)
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create fuel failed");
                TempData["Error"] = "Failed to create fuel entry.";
                await PopulateLocomotivesAsync();
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            var vm = _fuelService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelAllViewModel model)
        {
            if (id != model.Id) return BadRequest();

            string[] nonEditableKeys =
            [
                nameof(FuelAllViewModel.LocomotiveNumber),
                nameof(FuelAllViewModel.InitialFuel),
                nameof(FuelAllViewModel.Consumption),
                nameof(FuelAllViewModel.IsDeleted),
                "CreatedOn","CreatedByUserName","EditedBy","EditedOn"
            ];
            foreach (var key in nonEditableKeys) ModelState.Remove(key);

            if (!ModelState.IsValid) return View(model);

            var user = User?.Identity?.Name ?? "system";
            try
            {
                await _fuelService.EditAsync(id, model, user);
                TempData["Success"] = "Fuel entry updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit fuel failed for id {Id}", id);
                TempData["Error"] = "Failed to update fuel entry.";
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
    }
}