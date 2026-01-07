using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using WebCrudApp.Data;
using WebCrudApp.Models;
using WebCrudApp.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebCrudApp.Controllers
{
    public class ItemDetailsController : Controller
    {
        ItemDetailRepository repo = new ItemDetailRepository();

        public IActionResult Index(int logicalRef)
        {
            ItemViewModel model = repo.GetItemDetails(logicalRef);
            if (model == null)
                return NotFound();

            model.UnitDetails = repo.GetItemUnitDetails(logicalRef);

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateItems([FromBody] List<ItemUnitDetailModel> items)
        {
            repo.UpdateItems(items);
            return Ok();
        }


    }
}
