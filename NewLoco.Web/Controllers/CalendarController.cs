namespace NewLoco.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
public class CalendarController : Controller
    {
    [AllowAnonymous]
    public IActionResult Index() => View();
    }