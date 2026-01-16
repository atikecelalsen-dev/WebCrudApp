using Library.Models.Order;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Models.Invoice;
using Library.Models;

namespace Library.Models.Invoice
{
    public class InvoiceCreateViewModel
    {
        public InvoiceCreateViewModel()
        {
            Header = new InvoiceHeaderModel();
            Lines = new List<InvoiceLineModel>();
            InvoiceTypes = new List<SelectListItem>();
            Clients = new List<SelectListItem>();
            Items = new List<InvoiceItemViewModel>();
            Units = new List<SelectListItem>();
            PurchasePrice = new List<SelectListItem>();
            SalePrice = new List<SelectListItem>();

        }

        public InvoiceHeaderModel Header { get; set; }
        public List<InvoiceLineModel> Lines { get; set; }
        public List<SelectListItem> InvoiceTypes { get; set; }
        public List<SelectListItem> Clients { get; set; }
        public List<InvoiceItemViewModel> Items { get; set; }
        public List<SelectListItem> Units { get; set; }
        public List<SelectListItem> PurchasePrice { get; set; }
        public List<SelectListItem> SalePrice { get; set; }



    }
}
