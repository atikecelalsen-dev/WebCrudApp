using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Library.Models.Order;
using Library.Repository;

public class OrderController : Controller
{
    private readonly Library.Repository.OrderRepository _repo;

    public OrderController()
    {
        _repo = new Library.Repository.OrderRepository(); 
    }

    public IActionResult Index()
    {
        return View(_repo.GetOrders());
    }

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
    public IActionResult GetPrice(int itemRef, int uomRef, int orderType)
    {
        decimal price = _repo.GetItemPrice(itemRef, uomRef, orderType);
        return Json(price);
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
    [ValidateAntiForgeryToken]
    public IActionResult Create(OrderCreateViewModel model)
    {
        if (model?.Header == null || model.Lines == null || !model.Lines.Any())
            return Json(new
            {
                success = false,
                message = "Geçersiz model"
            });
    
        _repo.CreateOrder(model);
        

        return Json(new { success = true});
        
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
            return Json(new { success = true});
        }
        return Json(new { success = false, message = "Sipariş silinemedi." });
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(OrderCreateViewModel vm)
    {
        try
        {
            _repo.UpdateOrderHeader(vm.Header);
            _repo.UpdateOrderLines(vm.Header.LOGICALREF, vm.Lines, vm.Header);
            return Json(new { success = true, message = "Sipariş başarıyla güncellendi."});
        
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }


}