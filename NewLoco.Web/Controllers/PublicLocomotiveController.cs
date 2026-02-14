//   Public read-only controller exposing ONLY locomotive numbers to anonymous users.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Service.Core.Contracts;
using System.Linq;

namespace NewLoco.Web.Controllers
    {
    [AllowAnonymous] //   explicitly allow anonymous access
    public class PublicLocomotivesController : Controller
        {
        private readonly ILocomotiveService service;

        public PublicLocomotivesController(ILocomotiveService service)
            {
            this.service = service ?? throw new ArgumentNullException(nameof(service));  
            }

        // GET: /Locomotives
        [HttpGet]
        [Route("Locomotives")]  
        public async Task<IActionResult> Index()
            {
            //  We reuse service.GetAll("active") but project to ONLY Numbers to avoid exposing details.
            var all = await service.GetAll("active");
            var numbers = all
                .Select(x => x.Number)    
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            return View(numbers); // model: List<string>
            }
        }
    }