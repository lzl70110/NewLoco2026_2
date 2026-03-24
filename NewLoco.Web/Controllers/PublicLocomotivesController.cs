using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Web.Controllers
{
    // C# 12 primary constructor for the controller
    public class PublicLocomotivesController(ILocomotiveService service) : Controller
    {
        // IMPORTANT: use a *different* name than the primary-ctor parameter
        // Otherwise the field shadows the parameter and you self-initialize the field.
        private readonly ILocomotiveService _service = service ?? throw new ArgumentNullException(nameof(service));

        // GET: /PublicLocomotives?filter=active|all|deleted
        [AllowAnonymous] // <-- whitelist
        public async Task<IActionResult> Index(string? filter = "active")
        {
            var dtos = await _service.GetAllAsync(filter);

            var model = dtos.Select(d => new LocomotiveNumberViewModel
            {
                Id = d.Id,
                Number = d.Number,
                LocomotiveType = d.LocomotiveType,
                MeasuringUnit = d.MeasuringUnit,
                IsDeleted = d.IsDeleted
            }).ToList();

            ViewData["CurrentFilter"] = filter;
            return View(model);
        }

        // GET: /PublicLocomotives/Details/5
        // Make it public as well if you want full public read
        [AllowAnonymous] // <-- add if details must be public; remove if not desired
        public async Task<IActionResult> Details(int id, string? filter)
        {
            var dto = await _service.GetDetailsAsync(id);
            if (dto == null) return NotFound();

            var vm = new LocomotiveDetailsViewModel
            {
                Id = dto.Id,
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,
                MeasuringUnit = dto.MeasuringUnit,
                Note = dto.Note ?? string.Empty,
                IsDeleted = dto.IsDeleted,
                CreatedOn = dto.CreatedOn,
                CreatedBy = dto.CreatedBy!,
                ModifiedOn = dto.ModifiedOn,
                ModifiedBy = dto.ModifiedBy
            };

            ViewData["CurrentFilter"] = filter;
            return View(vm);
        }
    }
}