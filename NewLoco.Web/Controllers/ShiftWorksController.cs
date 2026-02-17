using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.ShiftWorks;

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

        // ---------- Helper: populate dropdown ----------
        private async Task PopulateLocomotivesAsync(ShiftWorksViewModelBase model)
            {
            var options = await locoService.GetOptionsAsync();
            model.Locomotives = options
                .Select(o => new SelectListItem
                    {
                    Value = o.Id.ToString(),
                    Text = o.Number,
                    Selected = model.LocomotiveId == o.Id
                    })
                .ToList();
            }

        // ---------- INDEX ----------
        public IActionResult Index()
            {
            var vm = shiftService.GetAll();
            return View(vm);
            }

        // ---------- CREATE ----------
        public async Task<IActionResult> Create()
            {
            var vm = shiftService.CreateModel();
            vm.Date = DateTime.Today;

            await PopulateLocomotivesAsync(vm);

            // Ако е избран локомотив, вземи последната смяна
            if (vm.LocomotiveId != 0)
                {
                var lastShift = await shiftService.GetLastShiftAsync(vm.LocomotiveId);
                if (lastShift != null)
                    {
                    vm.InitialValue = lastShift.FinalValue;
                    vm.InitialValueDate = lastShift.Date;
                    }
                }

            return View(vm);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShiftWorkViewModel model)
            {
            if (!ModelState.IsValid)
                {
                await PopulateLocomotivesAsync(model);
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            model.Date = model.Date == default ? DateTime.Today : model.Date;

            try
                {
                await shiftService.CreateAsync(model, user);
                TempData["Success"] = "ShiftWork created.";
                return RedirectToAction(nameof(Index));
                }
            catch (InvalidOperationException ex)
                {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateLocomotivesAsync(model);
                return View(model);
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Create ShiftWork failed");
                TempData["Error"] = "Failed to create shift work.";
                await PopulateLocomotivesAsync(model);
                return View(model);
                }
            }

        // ---------- EDIT ----------
        public async Task<IActionResult> Edit(int id)
            {
            var vm = shiftService.GetForEdit(id);
            if (vm == null) return NotFound();

            await PopulateLocomotivesAsync(vm);
            return View(vm);
            }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditShiftWorkViewModel model)
            {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                {
                await PopulateLocomotivesAsync(model);
                return View(model);
                }

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
                await PopulateLocomotivesAsync(model);
                return View(model);
                }
            catch (Exception ex)
                {
                logger.LogError(ex, "Edit ShiftWork failed for id {Id}", id);
                TempData["Error"] = "Failed to update shift work.";
                await PopulateLocomotivesAsync(model);
                return View(model);
                }
            }

        // ---------- DELETE ----------
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

        // ---------- UNDO DELETE ----------
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
