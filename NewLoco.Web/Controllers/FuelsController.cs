using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Fuels;
using System.Globalization;
using System.Text;

namespace NewLoco.Web.Controllers
    {
    [Authorize]
    public class FuelsController : BaseController
        {
        private readonly IFuelService fuelService;
        private readonly ILocomotiveService locoService;
        private readonly ILogger<FuelsController> logger;

        public FuelsController(
            IFuelService fuelService,
            ILocomotiveService locoService,
            ILogger<FuelsController> logger)
            {
            this.fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService));
            this.locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

        public IActionResult FuelReport()
            {
            var vm = fuelService.GetAll();
            return View(vm);
            }

        private async Task PopulateLocomotivesAsync()
            {
            var options = await locoService.GetOptionsAsync();
            ViewBag.Locomotives = options
                .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Number })
                .ToList();
            }

        public async Task<IActionResult> Create()
            {
            await PopulateLocomotivesAsync();
            return View(fuelService.CreateModel());
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFuelViewModel model)
            {
            // игнорираме InitialFuel от клиента – сървърът ще го изчисли
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

        public IActionResult Edit(int id)
            {
            var vm = fuelService.GetForEdit(id);
            if (vm == null) return NotFound();
            return View(vm);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FuelAllViewModel model)
            {
            if (id != model.Id) return BadRequest();

            string[] nonEditableKeys =
            {
                nameof(FuelAllViewModel.LocomotiveNumber),
                nameof(FuelAllViewModel.InitialFuel),
                nameof(FuelAllViewModel.Consumption),
                nameof(FuelAllViewModel.IsDeleted),
                "CreatedOn","CreatedByUserName","EditedBy","EditedOn"
            };
            foreach (var key in nonEditableKeys) ModelState.Remove(key);

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

        [HttpGet]
        public async Task<IActionResult> PrevFinal(int locoId, string date)
            {
            if (string.IsNullOrWhiteSpace(date)) return Json(new { value = 0m });

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                if (!DateTime.TryParse(date, CultureInfo.GetCultureInfo("bg-BG"), DateTimeStyles.None, out parsed))
                    return Json(new { value = 0m });
                }

            var value = await fuelService.GetPrevFinalAsync(locoId, parsed);
            return Json(new { value });
            }

        //        private static ContentResult DiagnosticGate(string details, string continueUrl)
        //            {
        //            var html = $@"
        //<!DOCTYPE html>
        //<html><head><meta charset=""utf-8"" /><title>POST Diagnostics</title></head>
        //<body><pre>{System.Net.WebUtility.HtmlEncode(details)}</pre><a href=""{continueUrl}"">Continue</a></body></html>";
        //            return new ContentResult { Content = html, ContentType = "text/html; charset=utf-8", StatusCode = 200 };
        //            }

        //        private static string BuildPostDiagnostics(string title, object? model, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState, HttpRequest request)
        //            {
        //            var sb = new StringBuilder();
        //            sb.AppendLine(title);
        //            sb.AppendLine(new string('-', 64));
        //            if (request.HasFormContentType)
        //                foreach (var kv in request.Form) sb.AppendLine($"{kv.Key}={string.Join(",", kv.Value)}");
        //            sb.AppendLine($"IsValid={modelState.IsValid}");
        //            return sb.ToString();
        //            }
        //        }
        }
    }