using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebCrudApp.Models;

[Route("[controller]/[action]")]
public class ClientAJAXController : Controller
{
    public IActionResult Index(){  
      
        return View();
    }

    [HttpGet]
    public IActionResult GetClientsPartial()
    {
        var clients = ClientAJAXModel.GetClients();
        return PartialView("_ClientListPartial", clients);
    }

    [HttpGet]
    public IActionResult SearchPartial(string code, string definition)
    {
        var results = ClientAJAXModel.Search(code, definition);
        return PartialView("_ClientListPartial", results);
    }

    [HttpPost]
    public IActionResult Create(string CODE, string DEFINITION_)
    {
        ClientAJAXModel.Create(CODE, DEFINITION_);
        return Ok();
    }

    [HttpPost]
    public IActionResult Update(int LOGICALREF, string CODE, string DEFINITION_)
    {
        bool updated = ClientAJAXModel.Update(LOGICALREF, CODE, DEFINITION_);
        if (updated) return Ok();
        return NotFound("Güncellenecek kayıt bulunamadı");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        bool deleted = ClientAJAXModel.Delete(id);
        if (deleted) return Ok();
        return NotFound("Silinecek kayıt bulunamadı");
    }
}