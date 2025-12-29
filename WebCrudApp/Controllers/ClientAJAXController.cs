using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCrudApp.Repository;

[Authorize]
public class ClientAJAXController : Controller
{
    ClientRepository repo = new ClientRepository();

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetClientsPartial()
    {
        return PartialView("_ClientListPartial", repo.GetClients());
    }

    [HttpGet]
    public IActionResult SearchPartial(string code, string definition)
    {
        return PartialView("_ClientListPartial", repo.Search(code, definition));
    }

    [HttpPost]
    public IActionResult Create(string CODE, string DEFINITION_)
    {
        repo.Create(CODE, DEFINITION_);
        return Ok();
    }

    [HttpPost]
    public IActionResult Update(int LOGICALREF, string CODE, string DEFINITION_)
    {
        return repo.Update(LOGICALREF, CODE, DEFINITION_) ? Ok() : NotFound();
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        return repo.Delete(id) ? Ok() : NotFound();
    }
}