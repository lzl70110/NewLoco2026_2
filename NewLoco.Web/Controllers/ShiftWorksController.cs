using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GCommon;                                  // Messages
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;             // IOptions<T>
using NewLoco.GCommon.Enums;                    // Shift
using NewLoco.Service.Core.Contracts;           // services + FuelPoliciesOptions
using NewLoco.Web.Auth;                         // Perm constants
using NewLoco.Web.ViewModels.Paging;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Web.Controllers
{
    [Authorize(Policy = Perm.ShiftWork.View)]
    public class ShiftWorksController(
        IShiftWorkService shiftService,
        ILocomotiveService locoService,
        IFuelService fuelService,
        IFuelEstimator fuelEstimator,
        IOptions<FuelPoliciesOptions> policies,
        ILogger<ShiftWorksController> logger
    ) : BaseController
    {
        private readonly IShiftWorkService _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        private readonly ILocomotiveService _locoService = locoService ?? throw new ArgumentNullException(nameof(locoService));
        private readonly IFuelService _fuelService = fuelService ?? throw new ArgumentNullException(nameof(fuelService));
        private readonly IFuelEstimator _fuelEstimator = fuelEstimator ?? throw new ArgumentNullException(nameof(fuelEstimator));
        private readonly FuelPoliciesOptions _policies = (policies ?? throw new ArgumentNullException(nameof(policies))).Value
                                                        ?? throw new ArgumentException(Messages.Fuel.Error_PoliciesNotConfigured);
        private readonly ILogger<ShiftWorksController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private async Task PopulateLocomotivesAsync(ShiftWorksViewModelBase model)
        {
            var options = await _locoService.GetOptionsAsync();
            model.Locomotives = [.. options
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.Number,
                    Selected = model.LocomotiveId == o.Id
                })];
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ShiftWorkFilterInput filter, CancellationToken ct)
        {
            var allowIncludeDeleted = User.IsInRole("SysAdmin");

            const int DefaultPage = 1;
            const int DefaultPageSize = 20;
            const int MaxPageSize = 100;

            var page = filter.Page < 1 ? DefaultPage : filter.Page;
            var pageSize = filter.PageSize <= 0 ? DefaultPageSize : Math.Min(filter.PageSize, MaxPageSize);

            var query = new ShiftWorkQuery
            {
                LocomotiveNumber = filter.LocomotiveNumber,
                From = filter.From,
                To = filter.To,
                IncludeDeleted = allowIncludeDeleted && filter.IncludeDeleted,
                Page = page,
                PageSize = pageSize
            };

            var (items, total) = await _shiftService.GetAllAsync(query, ct);

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
                    Page = page,
                    PageSize = pageSize
                },
                Paging = new PagingInfo
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = total
                }
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Policy = Perm.ShiftWork.Create)]
        public async Task<IActionResult> Create()
        {
            var vm = _shiftService.CreateModel();
            vm.Date = DateTime.Today;

            await PopulateLocomotivesAsync(vm);

            if (vm.LocomotiveId != 0)
            {
                var lastShift = await _shiftService.GetLastShiftAsync(vm.LocomotiveId);
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
        public async Task<IActionResult> CreateStep1(CreateShiftWorkViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLocomotivesAsync(model);
                return View("Create", model);
            }

            var hours = model.FinalValue - model.InitialValue;
            if (hours <= 0)
            {
                ModelState.AddModelError(nameof(model.FinalValue), Messages.ShiftWork.Error_FinalGreaterThanInitial);
                await PopulateLocomotivesAsync(model);
                return View("Create", model);
            }

            var locoType = await _locoService.GetTypeAsync(model.LocomotiveId);
            var est = _fuelEstimator.EstimateDefault(locoType, hours);
            var suggestedLiters = est.SuggestedLiters < 0 ? 0 : est.SuggestedLiters;

            var confirm = new ConfirmFuelViewModel
            {
                LocomotiveId = model.LocomotiveId,
                Date = model.Date == default ? DateTime.Today : model.Date,
                Shift = model.Shift,
                InitialValue = model.InitialValue,
                FinalValue = model.FinalValue,
                Note = model.Note,
                Hours = hours,
                LocomotiveType = locoType,
                FuelLiters = suggestedLiters,
                PolicyMinLph = est.PolicyMinLph,
                FullLoadHint = est.PolicyFullHint
            };

            // ---------- Soft warning (pre-commit) ----------
            var current = await _fuelService.GetCurrentStockAsync(model.LocomotiveId);
            var projectedFinal = current - (confirm.FuelLiters <= 0 ? 0 : confirm.FuelLiters);

            var cls = await GetClassCodeAsync(model.LocomotiveId);
            if (!string.IsNullOrWhiteSpace(cls) &&
                _policies.PerClassSafety != null &&
                _policies.PerClassSafety.TryGetValue(cls, out var safety) &&
                safety != null &&
                projectedFinal < safety.SoftWarningLiters &&
                projectedFinal >= safety.HardFloorLiters)
            {
                TempData[Messages.TempDataKeys.Warning] =
                    string.Format(Messages.Fuel.Warn_FinalBelowSoftFmt, safety.SoftWarningLiters);
            }
            // ------------------------------------------------

            return View("ConfirmFuel", confirm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Create)]
        public async Task<IActionResult> CreateCommit(ConfirmFuelViewModel vm)
        {
            if (!ModelState.IsValid) return View("ConfirmFuel", vm);

            var hint = vm.FullLoadHint <= 0 ? 1m : vm.FullLoadHint;
            var hours = vm.Hours <= 0 ? 0 : vm.Hours;
            var maxAllowed = hint * hours * 1.5m;

            if (vm.FuelLiters > maxAllowed)
            {
                ModelState.AddModelError(nameof(vm.FuelLiters),
                    string.Format(Messages.ShiftWork.Error_FuelTooHighFmt, hours));
                return View("ConfirmFuel", vm);
            }

            // Round to whole liters and validate 10 L depot step
            var liters = (int)Math.Round(vm.FuelLiters, 0, MidpointRounding.AwayFromZero);
            if (liters % 10 != 0)
            {
                ModelState.AddModelError(nameof(vm.FuelLiters),
                    Messages.FuelServiceKeys.Msg_FuelAmountMustBeMultipleOf10);
                return View("ConfirmFuel", vm);
            }

            var user = User?.Identity?.Name ?? "system";

            try
            {
                var createVm = new CreateShiftWorkViewModel
                {
                    LocomotiveId = vm.LocomotiveId,
                    Date = vm.Date,
                    Shift = vm.Shift,
                    InitialValue = vm.InitialValue,
                    FinalValue = vm.FinalValue,
                    Note = vm.Note
                };

                await _shiftService.CreateAsync(createVm, user);

                // consume on exact date/shift (uses the new service API)
                if (liters > 0)
                    await _fuelService.ConsumeOnAsync(vm.LocomotiveId, vm.Date, vm.Shift, liters, user);

                TempData[Messages.TempDataKeys.Success] = Messages.ShiftWork.Info_ShiftFuelRecorded;
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("ConfirmFuel", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCommit failed");
                TempData[Messages.TempDataKeys.Error] = Messages.ShiftWork.Error_ShiftSaveFailed;
                return View("ConfirmFuel", vm);
            }
        }

        [HttpGet]
        [Authorize(Policy = Perm.ShiftWork.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = _shiftService.GetForEdit(id);
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
                await _shiftService.EditAsync(id, model, user);
                TempData[Messages.TempDataKeys.Success] = Messages.ShiftWork.Info_ShiftUpdated;
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
                _logger.LogError(ex, "Edit ShiftWork failed for id {Id}", id);
                TempData[Messages.TempDataKeys.Error] = Messages.ShiftWork.Error_ShiftUpdateFailed;
                await PopulateLocomotivesAsync(model);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = User?.Identity?.Name ?? "system";

            try
            {
                await _shiftService.DeleteAsync(id, user);
                TempData[Messages.TempDataKeys.Success] = Messages.ShiftWork.Info_ShiftDeleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete ShiftWork failed for id {Id}", id);
                TempData[Messages.TempDataKeys.Error] = Messages.ShiftWork.Error_ShiftDeleteFailed;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Delete)]
        public async Task<IActionResult> UndoDelete(int id)
        {
            var user = User?.Identity?.Name ?? "system";

            try
            {
                await _shiftService.UndoDeleteAsync(id, user);
                TempData[Messages.TempDataKeys.Success] = Messages.ShiftWork.Info_ShiftRestored;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undo delete ShiftWork failed for id {Id}", id);
                TempData[Messages.TempDataKeys.Error] = Messages.ShiftWork.Error_ShiftRestoreFailed;
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------- helpers ----------

        // Resolve class code ("52", "55", "06") from locomotive number "52-xxx"
        private async Task<string> GetClassCodeAsync(int locomotiveId)
        {
            var opts = await _locoService.GetOptionsAsync();
            var number = opts.FirstOrDefault(o => o.Id == locomotiveId)?.Number?.Trim();
            if (string.IsNullOrWhiteSpace(number)) return string.Empty;
            var part = number.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .FirstOrDefault();
            return part ?? string.Empty;
        }
    }
}