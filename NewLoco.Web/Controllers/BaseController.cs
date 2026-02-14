using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NewLoco.Web.Controllers;

[Authorize]
public abstract class BaseController : Controller
    {
    }
