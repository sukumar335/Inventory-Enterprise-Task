
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryEnterpriseProject.Core.Interfaces;
using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    public IActionResult Index()
    {
        var items = _service.GetItems();
        return View(items);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(InventoryItem item)
    {
        _service.CreateItem(item);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var item = _service.GetItem(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    public IActionResult Edit(InventoryItem item)
    {
        _service.UpdateItem(item);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        _service.DeleteItem(id);
        return RedirectToAction("Index");
    }
}
