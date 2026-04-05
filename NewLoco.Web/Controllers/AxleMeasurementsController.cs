using GCommon.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.Auth;
using NewLoco.Web.ViewModels.Axles;
using NewLoco.Web.ViewModels.Paging;

namespace NewLoco.Web.Controllers;

[Authorize]
public class AxleMeasurementsController(
    IAxleMeasurementService service,
    ILogger<AxleMeasurementsController> logger) : Controller
{
    private readonly IAxleMeasurementService _service = service;
    private readonly ILogger<AxleMeasurementsController> _logger = logger;

    // --------------------------------------------------------
    // INDEX
    // --------------------------------------------------------
    [Authorize(Policy = Perm.Repairs.View)]
    public async Task<IActionResult> Index(AxleMeasurementFilterViewModel filter, int Page = 1, int PageSize = 20)
    {
        var cards = await _service.GetAllAsync();

        // filter
        if (!string.IsNullOrWhiteSpace(filter.LocomotiveNumber))
        {
            cards = [.. cards.Where(c => c.LocomotiveNumber.Contains(filter.LocomotiveNumber, StringComparison.OrdinalIgnoreCase))];
        }

        if (filter.From.HasValue)
            cards = [.. cards.Where(c => c.MeasurementDate >= filter.From.Value)];
        if (filter.To.HasValue)
            cards = [.. cards.Where(c => c.MeasurementDate <= filter.To.Value)];

        if (filter.ShowMode == AxleMeasurementShowMode.Active)
            cards = [.. cards.Where(c => !c.IsDeleted)];
        else if (filter.ShowMode == AxleMeasurementShowMode.Deleted)
            cards = [.. cards.Where(c => c.IsDeleted)];

        var pagedItems = cards
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .Select(c => new AxleMeasurementListItemViewModel
            {
                Id = c.Id,
                DocumentNumber = c.DocumentNumber,
                LocomotiveNumber = c.LocomotiveNumber,
                MeasurementDate = c.MeasurementDate,
                AxleCount = c.AxleCount,
                IsDeleted = c.IsDeleted
            })
            .ToList();

        var vm = new AxleMeasurementIndexViewModel
        {
            Filter = filter,
            Items = pagedItems,
            Paging = new PagingInfo
            {
                PageNumber = Page,
                PageSize = PageSize,
                TotalItems = cards.Count
            }
        };

        return View(vm);
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
        catch
        {
            TempData["Error"] = "Unable to load axle measurement card.";
            return RedirectToAction(nameof(Index));
        }
    }

    // --------------------------------------------------------
    // CREATE
    // --------------------------------------------------------
    [Authorize(Policy = Perm.Repairs.Create)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = await _service.GetCreateModelAsync();
        return View(vm);
    }

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

        var id = await _service.CreateAsync(model, User?.Identity?.Name ?? "Unknown");
        TempData["Success"] = "Axle measurement card created.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // --------------------------------------------------------
    // EDIT
    // --------------------------------------------------------
    [Authorize(Policy = Perm.Repairs.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var vm = await _service.GetEditModelAsync(id);
            return View(vm);
        }
        catch
        {
            TempData["Error"] = "Unable to load edit form.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Policy = Perm.Repairs.Edit)]
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
            await _service.UpdateAsync(model, User?.Identity?.Name ?? "Unknown");
            TempData["Success"] = "Axle measurement card updated.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }
        catch
        {
            TempData["Error"] = "Failed to update axle measurement card.";
            var vm = await _service.GetEditModelAsync(model.Id);
            return View(vm);
        }
    }

    // --------------------------------------------------------
    // DELETE
    // --------------------------------------------------------
    [Authorize(Policy = Perm.Repairs.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Axle measurement card deleted.";
        }
        catch
        {
            TempData["Error"] = "Failed to delete axle measurement card.";
        }

        return RedirectToAction(nameof(Index));
    }

    // --------------------------------------------------------
    // RESTORE
    // --------------------------------------------------------
    [Authorize(Policy = Perm.Repairs.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UndoDelete(int id)
    {
        try
        {
            await _service.RestoreAsync(id);
            TempData["Success"] = "Axle measurement card restored.";
        }
        catch
        {
            TempData["Error"] = "Failed to restore axle measurement card.";
        }

        return RedirectToAction(nameof(Index));
    }

    // --------------------------------------------------------
    // AJAX: GET AXLE INPUTS
    // --------------------------------------------------------
    [Authorize(Policy = Perm.Repairs.Create)]
    [HttpGet]
    public async Task<IActionResult> GetAxleInputs(int locoId)
    {
        var axlesCount = await _service.GetAxlesCountAsync(locoId);
        var axles = Enumerable.Range(1, axlesCount)
            .Select(n => new AxleMeasurementValueViewModel { AxleNumber = n })
            .ToList();
        return PartialView("_AxlesTable", axles);
    }
}