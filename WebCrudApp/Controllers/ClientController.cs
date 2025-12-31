using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCrudApp.Models;

[Authorize]
public class ClientController : Controller
{
    public IActionResult Index(string CODE, string DEFINITION_)
    {
        var list = ClientViewModel.GetList(CODE, DEFINITION_);
        return View(list);
    }

    [HttpPost]
    public IActionResult Create(ClientViewModel client)
    {
        ClientViewModel.Add(client);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Update(ClientViewModel client)
    {
        ClientViewModel.Update(client);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int logicalRef)
    {
        ClientViewModel.Delete(logicalRef);
        return RedirectToAction("Index");
    }
}