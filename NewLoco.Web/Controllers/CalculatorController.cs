namespace NewLoco.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
public class CalculatorController : Controller
    {
    [AllowAnonymous]
    public IActionResult Index() => View();
    }