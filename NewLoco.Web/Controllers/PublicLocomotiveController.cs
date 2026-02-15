using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Locomotives;
// Ако VM-ите са с enum полета:
using NewLoco.GCommon.Enums;

namespace NewLoco.Web.Controllers
    {
    public class PublicLocomotiveController : Controller
        {
        private readonly ILocomotiveService service;

        public PublicLocomotiveController(ILocomotiveService service)
            {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            }

        // GET: /PublicLocomotive?filter=active|all|deleted
        public async Task<IActionResult> Index(string? filter = "active")
            {
            // Call the new async service API
            var dtos = await service.GetAllAsync(filter);

            // Map DTO -> public VM (adjust to your exact public VM type if different)
            var model = dtos.Select(d => new LocomotiveNumberViewModel
                {
                Id = d.Id,
                Number = d.Number,
                LocomotiveType = d.LocomotiveType,  // enum -> enum
                MeasuringUnit = d.MeasuringUnit,    // enum -> enum
                IsDeleted = d.IsDeleted
                }).ToList();

            ViewData["CurrentFilter"] = filter;
            return View(model);
            }

        // GET: /PublicLocomotive/Details/5
        public async Task<IActionResult> Details(int id, string? filter)
            {
            var dto = await service.GetDetailsAsync(id);
            if (dto == null) return NotFound();

            // Map DTO -> details VM
            var vm = new LocomotiveDetailsViewModel
                {
                Id = dto.Id,
                Number = dto.Number,
                LocomotiveType = dto.LocomotiveType,  // enum -> enum
                MeasuringUnit = dto.MeasuringUnit,    // enum -> enum
                Note = dto.Note ?? string.Empty,
                IsDeleted = dto.IsDeleted,
                CreatedOn = dto.CreatedOn,
                CreatedBy = dto.CreatedBy,
                ModifiedOn = dto.ModifiedOn,
                ModifiedBy = dto.ModifiedBy
                };

            ViewData["CurrentFilter"] = filter;
            return View(vm);
            }
        }
    }