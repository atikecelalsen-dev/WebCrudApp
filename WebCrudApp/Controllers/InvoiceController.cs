using Library.Models.Invoice;
using Library.Models.Order;
using Library.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebCrudApp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly InvoiceRepository _repo;
       

        public InvoiceController()
        {
            _repo = new InvoiceRepository();
        }
 
        public IActionResult Index()
        {
            return View(_repo.GetInvoices());
        }

        [HttpPost]
        public IActionResult DeleteInvoice(int id)
        {
            bool success = _repo.DeleteInvoice(id);
            if (success)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Fatura silinemedi." });
        }

        private void FillDropdowns(InvoiceCreateViewModel model)
        {
            model.Clients = _repo.GetClients();
            model.Items = _repo.GetItems();
            model.InvoiceTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Satın Alma Faturası", Value = "1" },
                new SelectListItem { Text = "Satış Faturası", Value = "2" }
            };
            ViewBag.ProductUnits = _repo.GetUnits();
        }

        [HttpGet]
        public IActionResult GetPrice(int itemRef, int uomRef, int invoiceType)
        {
            decimal price = _repo.GetItemPrice(itemRef, uomRef, invoiceType);
            return Json(price);
        }


        [HttpGet]
        public IActionResult Create()
        {
            var model = new InvoiceCreateViewModel
            {
                Header = new InvoiceHeaderModel
                {
                    DATE_ = DateTime.Now
                },
                Lines = new List<InvoiceLineModel>
                {
                    new InvoiceLineModel{VAT = 20}
                }
            };

            FillDropdowns(model);
            return View(model);
        }

        // Yeni sipariş oluşturma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InvoiceCreateViewModel model)
        {
            if (model?.Header == null || model.Lines == null || !model.Lines.Any())
                return Json(new
                {
                    success = false,
                    message = "Geçersiz model"
                });

            _repo.CreateInvoice(model);


            return Json(new { success = true });

        }

        // Mevcut sipariş
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _repo.GetInvoiceForEdit(id);
            FillDropdowns(model);                  
            return View("Create", model);           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(InvoiceCreateViewModel vm)
        {
            try
            {
                _repo.UpdateInvoiceHeader(vm.Header);
                _repo.UpdateInvoiceLines(vm.Header.LOGICALREF, vm.Lines, vm.Header);
                return Json(new { success = true, message = "Fatura başarıyla güncellendi." });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
