using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebCrudApp.Models;


//[Route("[controller]/[action]")]
public class ItemController : Controller
{
    ItemRepository repo = new ItemRepository();

    public IActionResult Index()
    {
        return View(new ItemPageViewModel
        {
            Item = new ItemViewModel(),
            UnitSets = UnitSetViewModel.GetUnitSetDropdown(),
            ItemList = repo.GetItems()
        });
    }

    [HttpPost]
    public IActionResult Create(string CODE, string NAME, int UNITSETREF)
    {
        repo.Create(CODE, NAME, UNITSETREF);
        return Ok();
    }

    public IActionResult GetItemsPartial()
        => PartialView("ItemListPartial", repo.GetItems());

    public IActionResult SearchPartial(string code, string name)
        => PartialView("ItemListPartial", repo.Search(code, name));

    [HttpPost]
    public IActionResult Update(int LOGICALREF, string CODE, string NAME, int UNITSETREF)
    {
        bool updated = ItemRepository.Update(LOGICALREF, CODE, NAME, UNITSETREF);

        if (updated)
            return Ok();
        else
            return BadRequest("Update başarısız");
    }




    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (id == 0)
            return BadRequest("ID 0 geldi");

        bool deleted = repo.Delete(id);
        if (deleted) return Ok();
        return NotFound("Silinecek kayıt bulunamadı");
    }



}