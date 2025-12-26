using Microsoft.AspNetCore.Mvc;
using WebCrudApp.Models;

namespace WebCrudApp.Controllers
{
    public class ItemController : Controller
    {
        [HttpPost]
        public IActionResult Index(ItemPageViewModel model, string action)
        {
           

            switch (action)
            {
                case "create":
                    ItemViewModel.InsertItem(model.Item);
                    break;

                case "search":
                    model.ItemList = ItemListModel.Search(
                        model.Item.Code,
                        model.Item.Name
                    );
                    return View(model); // 🔴 Redirect YOK

                case "update":
                    ItemViewModel.UpdateItem(model.Item);
                    break;

                case "delete":
                    ItemViewModel.DeleteItem(model.Item.LogicalRef);
                    break;

                case "list":
                default:
                    break;
            }

            // Listele (normal yükleme)
            model.ItemList = ItemListModel.GetItemList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new ItemPageViewModel
            {
                Item = new ItemViewModel(),
                ItemList = ItemListModel.GetItemList()
            };

            return View(model);
        }

       
    }
}