using Microsoft.AspNetCore.Mvc;
using Library.Repository;

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
            return Json(new { success = false, message = "Sipariş silinemedi." });
        }
    }
}
