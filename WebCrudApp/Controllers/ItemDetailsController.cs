using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebCrudApp.Data;
using WebCrudApp.Models;
using WebCrudApp.Repository;

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

            model.UnitDetails = repo.GetItemUnitDetails(logicalRef);  // bu satır eklendi

            return View(model);
        }




    }
}
