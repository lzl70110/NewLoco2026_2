using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Web.Controllers
    {
    public class PublicLocomotivesController : Controller
        {
        private readonly ILocomotiveService service;

        public PublicLocomotivesController(ILocomotiveService service)
            => this.service = service ?? throw new ArgumentNullException(nameof(service));

        // GET: /PublicLocomotives?filter=active|all|deleted
        public async Task<IActionResult> Index(string? filter = "active")
            {
            var dtos = await service.GetAllAsync(filter);

            // map DTO -> public VM (enum->enum; ако при теб VM е string, ползвай .ToString())
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
        public async Task<IActionResult> Details(int id, string? filter)
            {
            var dto = await service.GetDetailsAsync(id);
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