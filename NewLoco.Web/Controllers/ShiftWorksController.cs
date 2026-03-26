using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GCommon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewLoco.GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth;
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
        private readonly IShiftWorkService _shiftService = shiftService;
        private readonly ILocomotiveService _locoService = locoService;
        private readonly IFuelService _fuelService = fuelService;
        private readonly IFuelEstimator _fuelEstimator = fuelEstimator;
        private readonly FuelPoliciesOptions _policies =
            (policies ?? throw new ArgumentNullException(nameof(policies))).Value
            ?? throw new ArgumentException(Messages.Fuel.Error_PoliciesNotConfigured);
        private readonly ILogger<ShiftWorksController> _logger = logger;

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
            var allowDeleted = User.IsInRole("SysAdmin");

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
                IncludeDeleted = allowDeleted && filter.IncludeDeleted,
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
                var last = await _shiftService.GetLastShiftAsync(vm.LocomotiveId);
                if (last != null)
                {
                    vm.InitialValue = last.FinalValue;
                    vm.InitialValueDate = last.Date;
                }
            }

            return View(vm);
        }

        // =====================================================
        // LOAD LAST VALUES (AJAX ENDPOINT)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetLastValues(int id)
        {
            var last = await _shiftService.GetLastShiftAsync(id);
            if (last == null)
                return Json(new { ok = false });

            return Json(new
            {
                ok = true,
                initial = last.FinalValue,
                date = last.Date.ToString("yyyy-MM-dd")
            });
        }

        // =====================================================
        // CREATE STEP 1
        // =====================================================
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

            var amount = model.FinalValue - model.InitialValue;
            if (amount <= 0)
            {
                ModelState.AddModelError(nameof(model.FinalValue),
                    Messages.ShiftWork.Error_FinalGreaterThanInitial);

                await PopulateLocomotivesAsync(model);
                return View("Create", model);
            }

            var loco = await _locoService.GetDetailsAsync(model.LocomotiveId);
            if (loco == null)
            {
                ModelState.AddModelError(string.Empty, Messages.ShiftWork.Error_LocomotiveNotFound);
                await PopulateLocomotivesAsync(model);
                return View("Create", model);
            }

            decimal suggested = 0;
            decimal fullHint = 0;

            if (loco.MeasuringUnit == MeasuringUnits.Mh)
            {
                var est = _fuelEstimator.EstimateDefault(
                    loco.LocomotiveType,
                    amount,
                    loco.MeasuringUnit
                );

                suggested = est.SuggestedLiters < 0 ? 0 : est.SuggestedLiters;
                fullHint = est.PolicyFullHint;
            }

            var currentFuel = await _fuelService.GetCurrentStockAsync(model.LocomotiveId);
            if (currentFuel < 100)
            {
                TempData[Messages.TempDataKeys.Warning] =
                    string.Format(Messages.ShiftWork.Warn_LowFuelLevelFmt, currentFuel);
            }

            var confirm = new ConfirmFuelViewModel
            {
                LocomotiveId = model.LocomotiveId,
                Date = model.Date == default ? DateTime.Today : model.Date,
                Shift = model.Shift,
                InitialValue = model.InitialValue,
                FinalValue = model.FinalValue,
                Note = model.Note,
                Hours = amount,
                LocomotiveType = loco.LocomotiveType,
                MeasuringUnits = loco.MeasuringUnit,
                LocomotiveNumber = loco.Number,
                FuelLiters = suggested,
                FullLoadHint = fullHint
            };

            return View("ConfirmFuel", confirm);
        }

        // =====================================================
        // CREATE COMMIT
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Create)]
        public async Task<IActionResult> CreateCommit(ConfirmFuelViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("ConfirmFuel", vm);

            if (vm.MeasuringUnits == MeasuringUnits.Mh)
            {
                var hint = vm.FullLoadHint <= 0 ? 1m : vm.FullLoadHint;
                var maxAllowed = hint * vm.Hours * 1.5m;

                if (vm.FuelLiters > maxAllowed)
                {
                    ModelState.AddModelError(nameof(vm.FuelLiters),
                        string.Format(Messages.ShiftWork.Error_FuelTooHighFmt, vm.Hours));

                    return View("ConfirmFuel", vm);
                }
            }

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

                if (liters > 0)
                    await _fuelService.ConsumeOnAsync(vm.LocomotiveId, vm.Date, vm.Shift, liters, user);

                TempData[Messages.TempDataKeys.Success] =
                    Messages.ShiftWork.Info_ShiftFuelRecorded;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCommit failed");
                TempData[Messages.TempDataKeys.Error] =
                    Messages.ShiftWork.Error_ShiftSaveFailed;
                return View("ConfirmFuel", vm);
            }
        }

        // =====================================================
        // EDIT / DELETE / UNDO — unchanged
        // =====================================================

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

                TempData[Messages.TempDataKeys.Success] =
                    Messages.ShiftWork.Info_ShiftUpdated;

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

                TempData[Messages.TempDataKeys.Error] =
                    Messages.ShiftWork.Error_ShiftUpdateFailed;

                await PopulateLocomotivesAsync(model);
                return View(model);
            }
        }

        private async Task<string> GetClassCodeAsync(int locomotiveId)
        {
            var opts = await _locoService.GetOptionsAsync();
            var number = opts.FirstOrDefault(o => o.Id == locomotiveId)?.Number?.Trim();
            if (string.IsNullOrWhiteSpace(number)) return string.Empty;

            var part = number.Split('-', 2,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            return part ?? string.Empty;
        }
    }
}