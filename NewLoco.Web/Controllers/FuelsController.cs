using Microsoft.AspNetCore.Authorization;
using NewLoco.Service.Core.Contracts;
using NewLoco.Service.Core;
using Microsoft.AspNetCore.Mvc;

namespace NewLoco.Web.Controllers
    {
    [Authorize]
    public class FuelsController : BaseController
        {
        private readonly IFuelService service;


        public FuelsController(IFuelService service)
            {
            this.service = service;
            }
        public IActionResult FuelReport()
            {
            return View(service.GetAll());
            }
        public IActionResult Create()
            {
            return View(service.CreateModel());
            }

        }
    }
