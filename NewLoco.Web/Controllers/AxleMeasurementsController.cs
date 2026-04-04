using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth;
using NewLoco.Web.ViewModels.Axles;

namespace NewLoco.Web.Controllers
{
    [Authorize]
    public class AxleMeasurementsController(
        IAxleMeasurementService service,
        ILogger<AxleMeasurementsController> logger) : Controller
    {
        private readonly IAxleMeasurementService _service = service;
        private readonly ILogger<AxleMeasurementsController> _logger = logger;

        // --------------------------------------------------------
        // LIST
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.View)]
        public async Task<IActionResult> Index()
        {
            var cards = await _service.GetAllAsync();
            return View(cards);
        }

        // --------------------------------------------------------
        // DETAILS
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.View)]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var card = await _service.GetDetailsAsync(id);
                return View(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load axle measurement card {Id}", id);
                TempData["Error"] = "Unable to load axle measurement card.";
                return RedirectToAction(nameof(Index));
            }
        }

        // --------------------------------------------------------
        // CREATE (GET)
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.Create)]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = await _service.GetCreateModelAsync();
            return View(vm);
        }

        // --------------------------------------------------------
        // CREATE (POST)
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AxleMeasurementCardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _service.GetCreateModelAsync();
                vm.SelectedLocomotiveId = model.SelectedLocomotiveId;
                vm.MeasurementDate = model.MeasurementDate;
                vm.Axles = model.Axles;
                return View(vm);
            }

            var username = User?.Identity?.Name ?? "Unknown";
            var id = await _service.CreateAsync(model, username);
            TempData["Success"] = "Axle measurement card created.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // --------------------------------------------------------
        // EDIT (GET)
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.Create)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var vm = await _service.GetEditModelAsync(id);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load edit form for {Id}", id);
                TempData["Error"] = "Unable to load edit form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // --------------------------------------------------------
        // EDIT (POST)
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.Create)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AxleMeasurementCardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vm = await _service.GetEditModelAsync(model.Id);
                return View(vm);
            }

            try
            {
                var username = User?.Identity?.Name ?? "Unknown";
                await _service.UpdateAsync(model, username);
                TempData["Success"] = "Axle measurement card updated.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update axle measurement card {Id}", model.Id);
                TempData["Error"] = "Failed to update axle measurement card.";
                var vm = await _service.GetEditModelAsync(model.Id);
                return View(vm);
            }
        }

        // --------------------------------------------------------
        // AJAX — RETURN AXLE TABLE FOR SELECTED LOCOMOTIVE
        // --------------------------------------------------------
        [Authorize(Policy = Perm.Repairs.Create)]
        [HttpGet]
        public async Task<IActionResult> GetAxleInputs(int locoId)
        {
            var axlesCount = await _service.GetAxlesCountAsync(locoId);

            var axles = Enumerable.Range(1, axlesCount)
                .Select(n => new AxleMeasurementValueViewModel
                {
                    AxleNumber = n
                })
                .ToList();

            return PartialView("_AxlesTable", axles);
        }
    }
}