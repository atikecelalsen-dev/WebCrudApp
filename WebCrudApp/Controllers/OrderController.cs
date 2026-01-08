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

    //Sayfa oluştuğunda doldurulması gerekenler
    //[HttpGet]
    //public IActionResult Create()
    //{
    //    var model = new OrderCreateViewModel
    //    {
    //        Header = new OrderHeaderModel
    //        {
    //            DATE_ = DateTime.Now
    //        },
    //        Lines = new List<OrderLineModel>(),
    //        Clients = _repo.GetClients(),
    //        Items = _repo.GetItems(),
    //        OrderTypes = new List<SelectListItem>
    //        {
    //            new SelectListItem { Text = "Satış Siparişi", Value = "1" },
    //            new SelectListItem { Text = "Satın Alma Siparişi", Value = "2" }
    //        }
    //    };

    //    ViewBag.ProductUnits = _repo.GetUnits();

    //    return View(model);
    //}

    private void FillDropdowns(OrderCreateViewModel model)
    {
        model.Clients = _repo.GetClients();
        model.Items = _repo.GetItems();
        model.OrderTypes = new List<SelectListItem>
    {
        new SelectListItem { Text = "Satış Siparişi", Value = "1" },
        new SelectListItem { Text = "Satın Alma Siparişi", Value = "2" }
    };

        ViewBag.ProductUnits = _repo.GetUnits();
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
            Lines = new List<OrderLineModel>()
        };

        FillDropdowns(model);
        return View(model);
    }


    // Yeni sipariş oluşturma
    [HttpPost]
    public IActionResult Create(OrderCreateViewModel model)
    {
        if (model?.Header == null || model.Lines == null || !model.Lines.Any())
            return BadRequest("Geçersiz model");

        _repo.CreateOrder(model);
        TempData["msg"] = "Kayıt başarıyla yapıldı";
        return RedirectToAction("Create");
    }

    // Mevcut sipariş
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var model = _repo.GetOrderForEdit(id); // Header + Lines dolu gelir
        FillDropdowns(model);                  // dropdownlar tekrar doldurulur
        return View("Create", model);           // 🔥 AYNI VIEW
    }

    //[HttpPost]
    //public IActionResult Edit(OrderCreateViewModel model)
    //{
    //    // UPDATE
    //}


}