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
            Lines = new List<OrderLineModel>
        {
            new OrderLineModel
            {
                VAT = 20
            }
        }
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

    [HttpPost]
    public IActionResult DeleteOrder(int id)
    {
        bool success = _repo.DeleteOrder(id);
        if (success)
        {
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Sipariş silinemedi." });
    }










    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit3(OrderCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Hangi alanlar hatalı?
            var errors = ModelState
                            .Where(x => x.Value.Errors.Count > 0)
                            .Select(x => new { x.Key, x.Value.Errors })
                            .ToList();
            return Json(new { success = false, errors });
        }

        // Kaydetme işlemi
        
    return Json(new { success = true });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(OrderCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Form eksik veya hatalı." });
        }

      
        try
        {
            // 1️⃣ Header güncelle
            _repo.UpdateOrderHeader(vm.Header);

            // 2️⃣ Lines güncelle (header da gönderiliyor, indirim için)
            _repo.UpdateOrderLines(vm.Header.LOGICALREF, vm.Lines, vm.Header);

            // 3️⃣ Başarılıysa JSON veya redirect
            return Json(new { success = true, message = "Sipariş başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


}