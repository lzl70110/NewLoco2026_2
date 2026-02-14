using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NewLoco.Web.Controllers;
public class ShiftWorksController : Controller
    {
    // GET: WorksController
    public ActionResult Index()
        {
        return View();
        }

    // GET: WorksController/Details/5
    public ActionResult Details(int id)
        {
        return View();
        }

    // GET: WorksController/Create
    public ActionResult Create()
        {
        return View();
        }

    // POST: WorksController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(IFormCollection collection)
        {
        try
            {
            return RedirectToAction(nameof(Index));
            }
        catch
            {
            return View();
            }
        }

    // GET: WorksController/Edit/5
    public ActionResult Edit(int id)
        {
        return View();
        }

    // POST: WorksController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, IFormCollection collection)
        {
        try
            {
            return RedirectToAction(nameof(Index));
            }
        catch
            {
            return View();
            }
        }

    // GET: WorksController/Delete/5
    public ActionResult Delete(int id)
        {
        return View();
        }

    // POST: WorksController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
        {
        try
            {
            return RedirectToAction(nameof(Index));
            }
        catch
            {
            return View();
            }
        }
    }
