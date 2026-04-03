using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewLoco.Web.Auth;
using NewLoco.Web.Models;




namespace NewLoco.Web.Controllers
{
    public class HomeController : Controller
    {
   
        public IActionResult Index()
        {
            var permClaims = User.Claims
    .Where(c => c.Type == Perm.ClaimType)
    .Select(c => c.Value)
    .ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Forbidden()
        {
            return View("~/Views/Shared/403.cshtml");
        }
        public IActionResult UnauthorizedPage()
        {
            return View("~/Views/Shared/401.cshtml");
        }
    }

}
