using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebCrudApp.Models;



//public class ItemController : Controller
//{
//    public IActionResult Index()
//    {
//        var model = new ItemPageViewModel
//        {
//            Item = new ItemViewModel(),
//            UnitSets = UnitSetViewModel.GetUnitSetDropdown(),
//            ItemList = ItemRepository.GetItems()
//        };

//        return View(model);
//    }

//    [HttpPost]
//    public IActionResult Create(string CODE, string NAME, int UNITSETREF)
//    {
//        Console.WriteLine("UNITSETREF = " + UNITSETREF);
//        ItemRepository.Create(CODE, NAME, UNITSETREF);
//        return Ok();
//    }

//    [HttpGet]
//    public IActionResult GetItemsPartial()
//    {
//        var items = ItemViewModel.GetItems();
//        return PartialView("ItemListPartial", items);
//    }

//    [HttpGet]
//    public IActionResult SearchPartial(string code, string name)
//    {
//        var results = ItemViewModel.Search(code, name);
//        return PartialView("ItemListPartial", results);
//    }



//    [HttpPost]
//    public IActionResult Update(int LOGICALREF, string CODE, string NAME)
//    {
//        bool updated = ItemViewModel.Update(LOGICALREF, CODE, NAME);
//        if (updated) return Ok();
//        return NotFound("Güncellenecek kayıt bulunamadı");
//    }

//    [HttpPost]
//    public IActionResult Delete(int id)
//    {
//        bool deleted = ItemViewModel.Delete(id);
//        if (deleted) return Ok();
//        return NotFound("Silinecek kayıt bulunamadı");
//    }
//}

[Route("[controller]/[action]")]
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
    public IActionResult Update(int LOGICALREF, string CODE, string NAME)
    {
        bool updated = ItemRepository.Update(LOGICALREF, CODE, NAME);

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