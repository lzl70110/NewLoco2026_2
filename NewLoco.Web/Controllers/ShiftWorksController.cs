using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using GCommon.Enums;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth;
using NewLoco.Web.ViewModels.Paging;
using NewLoco.Web.ViewModels.ShiftWorks;

namespace NewLoco.Web.Controllers
{
    [Authorize(Policy = Perm.ShiftWork.View)]
    public class ShiftWorksController : Controller
    {
        private readonly IShiftWorkService _shiftService;
        private readonly ILocomotiveService _locoService;
        private readonly ILogger<ShiftWorksController> _logger;

        public ShiftWorksController(
            IShiftWorkService shiftService,
            ILocomotiveService locoService,
            ILogger<ShiftWorksController> logger)
        {
            _shiftService = shiftService;
            _locoService = locoService;
            _logger = logger;
        }

        private async Task PopulateLocomotivesAsync(ShiftWorksViewModelBase model)
        {
            var options = await _locoService.GetOptionsAsync();
            model.Locomotives = options.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Number,
                Selected = model.LocomotiveId == o.Id
            }).ToList();
        }

        // ====================
        // INDEX
        // ====================
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ShiftWorkFilterInput filter, CancellationToken ct)
        {
            var allowDeleted = User.IsInRole("SysAdmin");

            int page = filter.Page < 1 ? 1 : filter.Page;
            int pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

            // parse ShowMode string from query
            var showMode = allowDeleted ? filter.ShowMode : ShiftWorkShowMode.Active;
            
            // Build query for service
            var query = new ShiftWorkQuery
            {
                LocomotiveNumber = filter.LocomotiveNumber,
                From = filter.From,
                To = filter.To,
                IncludeDeleted = allowDeleted && (showMode == ShiftWorkShowMode.Deleted || showMode == ShiftWorkShowMode.All),
                Page = page,
                PageSize = pageSize
            };

            var (items, total) = await _shiftService.GetAllAsync(query, ct);

            // Filter items for Deleted / Active / All
            var filteredItems = showMode switch
            {
                ShiftWorkShowMode.Active => items.Where(x => !x.IsDeleted).ToList(),
                ShiftWorkShowMode.Deleted => items.Where(x => x.IsDeleted).ToList(),
                ShiftWorkShowMode.All => items.ToList(),
                _ => items.ToList()
            };

            var rows = filteredItems.Select(x => new ShiftWorkListItemViewModel
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
                Filter = filter with { Page = page, PageSize = pageSize },
                Paging = new PagingInfo
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = total
                }
            };

            return View(vm);
        }

        // ====================
        // DELETE (SOFT)
        // ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = User?.Identity?.Name ?? "system";
            await _shiftService.DeleteAsync(id, user);

            TempData["Success"] = "Shift deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ====================
        // UNDO DELETE
        // ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Perm.ShiftWork.Delete)]
        public async Task<IActionResult> UndoDelete(int id)
        {
            var user = User?.Identity?.Name ?? "system";
            await _shiftService.UndoDeleteAsync(id, user);

            TempData["Success"] = "Shift restored successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}