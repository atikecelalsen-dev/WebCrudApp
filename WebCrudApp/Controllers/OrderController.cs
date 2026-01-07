using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCrudApp.Models;

public class OrderController : Controller
{
    private readonly OrderRepository _repo;

    public OrderController()
    {
        _repo = new OrderRepository();  // somut sınıf
    }

    public IActionResult Index()
    {
        return View(_repo.GetOrders());
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new OrderCreateViewModel
        {
            Header = new OrderHeaderModel
            {
                DATE_ = DateTime.Now
            },
            Lines = new List<OrderLineModel>(),
            Clients = _repo.GetClients(),
            Items = _repo.GetItems(),
            OrderTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Satış Siparişi", Value = "1" },
                new SelectListItem { Text = "Satın Alma Siparişi", Value = "2" }
            }
        };

        ViewBag.ProductUnits = _repo.GetUnits();

        return View(model);
    }



    [HttpPost]
    public IActionResult Create(OrderCreateViewModel model)
    {
        if (model?.Header == null || model.Lines == null || !model.Lines.Any())
            return BadRequest("Geçersiz model");

        _repo.CreateOrder(model);
        TempData["msg"] = "Kayıt başarıyla yapıldı";
        return RedirectToAction("Create");
    }
}