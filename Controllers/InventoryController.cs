
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

    public IActionResult Export()
    {
        var items = _service.GetItems();
        var builder = new System.Text.StringBuilder();
        
        // Add headers
        builder.AppendLine("Id,Name,SKU,Category,Price,Quantity");
        
        // Add data
        foreach(var item in items)
        {
            // Escape any commas or quotes in the text fields
            var name = item.Name?.Replace("\"", "\"\"") ?? "";
            var sku = item.SKU?.Replace("\"", "\"\"") ?? "";
            var category = item.Category?.Replace("\"", "\"\"") ?? "";
            
            builder.AppendLine($"{item.Id},\"{name}\",\"{sku}\",\"{category}\",{item.Price},{item.Quantity}");
        }
        
        return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "inventory_export.csv");
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
