using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewLoco.Data;
using NewLoco.Web.ViewModels.Locomotives;
using NewLoco.GCommon.Enums;
namespace NewLoco.Web.Areas.Admin.Controllers
    {
    [Area("Admin")]
    public class LocomotivesController : Controller
        {
        private readonly LocoDbContext _db;

        public LocomotivesController(LocoDbContext db) => _db = db;

        // GET: Admin/Locomotives?filter=active|deleted|all
        public async Task<IActionResult> Index(string filter = "active")
            {
            var query = _db.Locomotives.AsQueryable();

            query = filter switch
                {
                    "deleted" => query.Where(l => l.IsDeleted),
                    "all" => query,
                    _ => query.Where(l => !l.IsDeleted) // default: active
                    };

            // IMPORTANT: project to the concrete view model (NOT an anonymous type)
            var model = await query
                .OrderBy(l => l.Number)
                .Select(l => new LocomotiveNumberViewModel
                    {
                    Id = l.Id,
                    Number = l.Number,
                    // If your VM uses enums, keep enum types here;
                    // below we map to strings for a simple table output:
                    LocomotiveType = l.LocomotiveType,
                    MeasuringUnit = l.MeasuringUnit,
                    IsDeleted = l.IsDeleted
                    })
                .ToListAsync();

            ViewData["CurrentFilter"] = filter;
            return View(model);
            }

        // GET: Admin/Locomotives/Details/5
        public async Task<IActionResult> Details(int id, string filter)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = new LocomotiveDetailsViewModel
                {
                Id = entity.Id,
                Number = entity.Number,
                LocomotiveType = entity.LocomotiveType,
                MeasuringUnit = entity.MeasuringUnit,
                IsDeleted = entity.IsDeleted,
                Note = entity.Note!,
              CreatedOn= entity.CreatedOn,
                CreatedBy= entity.CreatedBy,
                ModifiedOn= entity.ModifiedOn,
                ModifiedBy= entity.ModifiedBy
                };

            ViewData["CurrentFilter"] = filter;
            return View(vm);
            }

        // GET: Admin/Locomotives/Create
        [HttpGet]
        public IActionResult Create(string filter)
            {
            ViewData["CurrentFilter"] = filter;
            return View(new LocomotiveFormModel());
            }

        // POST: Admin/Locomotives/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocomotiveFormModel model, string filter)
            {
            if (!ModelState.IsValid)
                {
                ViewData["CurrentFilter"] = filter;
                return View(model);
                }

            var user = User?.Identity?.Name ?? "system";
            var entity = new Data.Models.Locomotive
                {
                Number = model.Number,
                LocomotiveType = model.LocomotiveType,
                MeasuringUnit = model.MeasuringUnit,
                Note = model.Note,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user,
                ModifiedOn = null,
                ModifiedBy = null
                };

            _db.Locomotives.Add(entity);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { filter });
            }

        // GET: Admin/Locomotives/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string filter)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return NotFound();

            var vm = new LocomotiveFormModel
                {
                Number = entity.Number,
                LocomotiveType = entity.LocomotiveType,
                MeasuringUnit = entity.MeasuringUnit,
                Note = entity.Note
                };

            ViewData["CurrentFilter"] = filter;
            ViewData["EntityId"] = id;
            return View(vm);
            }

        // POST: Admin/Locomotives/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LocomotiveFormModel model, string filter)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return NotFound();

            if (!ModelState.IsValid)
                {
                ViewData["CurrentFilter"] = filter;
                ViewData["EntityId"] = id;
                return View(model);
                }

            entity.Number = model.Number;
            entity.LocomotiveType = model.LocomotiveType;
            entity.MeasuringUnit = model.MeasuringUnit;
            entity.Note = model.Note;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = User?.Identity?.Name ?? "system";

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { filter });
            }

        // POST: Admin/Locomotives/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string filter)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return NotFound();

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = User?.Identity?.Name ?? "system";

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { filter });
            }

        // POST: Admin/Locomotives/Undelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undelete(int id, string filter)
            {
            var entity = await _db.Locomotives.FindAsync(id);
            if (entity == null) return NotFound();

            entity.IsDeleted = false;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = User?.Identity?.Name ?? "system";

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { filter });
            }
        }
    }