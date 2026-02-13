using Microsoft.AspNetCore.Mvc;
using NewLoco.Data;
using NewLoco.Web.ViewModels.Locomotives;

namespace NewLoco.Web.Controllers
    {
    public class LocomotivesController : Controller
        {
        private readonly LocoDbContext _context;

        public LocomotivesController(LocoDbContext context)
            {
            _context = context;
            }

        // GET: /Locomotives
        public IActionResult Index()
            {
            var model = _context.Locomotives
                        .Select(l => new LocomotiveNumberViewModel
                            {
                            Id = l.Id,
                            Number = l.Number
                            })
                        .ToList();

            return View(model);
            }
        }
    }
