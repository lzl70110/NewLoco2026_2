using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth; // Perm constants
using NewLoco.Web.ViewModels.Paging;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Web.Controllers
{
    // Class-level guard: require read permission for listing and read-only endpoints
    [Authorize(Policy = Perm.ShiftWork.View)]
    public class ShiftWorksController(
        IShiftWorkService shiftService,
        ILocomotiveService locoService,
        ILogger<ShiftWorksController> logger) : BaseController
    {
        private readonly IShiftWorkService shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        private readonly ILocomotiveService locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
        private readonly ILogger<ShiftWorksController> logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Populate dropdown with locomotives
        private async Task PopulateLocomotivesAsync(ShiftWorksViewModelBase model)
        {
            var options = await locoService.GetOptionsAsync();
            model.Locomotives = [.. options
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Number,
                    Selected = model.LocomotiveId == o.Id
                })];
        }

        // ---------- INDEX (search + paging) ----------
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ShiftWorkFilterInput filter, CancellationToken ct)
        {
            // Only SysAdmin may include deleted via URL
            var allowIncludeDeleted = User.IsInRole("SysAdmin");

            var query = new ShiftWorkQuery
            {
                LocomotiveNumber = filter.LocomotiveNumber,
                From = filter.From,
                To = filter.To,
                IncludeDeleted = allowIncludeDeleted && filter.IncludeDeleted,
                Page = filter.Page < 1 ? 1 : filter.Page,
                PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize
            };

            var (items, total) = await shiftService.GetAllAsync(query, ct);

            var rows = items.Select(x => new ShiftWorkListItemViewModel
            {
                Id = x.Id,
                Date = x.Date,
                Shift = x.Shift,
                LocomotiveNumber = x.LocomotiveNumber,
                Operator = x.Operator,
                InitialValue = x.InitialValue,
                FinalValue = x.FinalValue,
                Amount = x.Amount,
                Note = x.Note,
                IsDeleted = x.IsDeleted
            }).ToList();

            var vm = new ShiftWorkIndexViewModel
            {
                Items = rows,
                Filter = new ShiftWorkFilterInput
                {
                    LocomotiveNumber = filter.LocomotiveNumber,
                    From = filter.From,
                    To = filter.To,
                    IncludeDeleted = query.IncludeDeleted,
                    Page = query.Page,
                    PageSize = query.PageSize
                },
                Paging = new PagingInfo
                {
                    PageNumber = query.Page,
                    PageSize = query.PageSize,
                    TotalItems = total
                }
            };

            return View(vm);
        }

        // ---------- CREATE ----------
        [HttpGet]
        [Authorize(Policy = Perm.ShiftWork.Create)]
        public async Task<IActionResult> Create()
        {
            var vm = shiftService.CreateModel();
            vm.Date = DateTime.Today;

            await PopulateLocomotivesAsync(vm);

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
        [Authorize(Policy = Perm.ShiftWork.Create)]
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
        [HttpGet]
        [Authorize(Policy = Perm.ShiftWork.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = shiftService.GetForEdit(id);
            if (vm == null) return NotFound();

            await PopulateLocomotivesAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Edit)]
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
        [Authorize(Policy = Perm.ShiftWork.Delete)]
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
        [Authorize(Policy = Perm.ShiftWork.Delete)]
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