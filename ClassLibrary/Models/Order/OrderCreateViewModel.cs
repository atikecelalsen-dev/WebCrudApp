using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Library.Models.Order
{
    public class OrderCreateViewModel
    {

        public OrderCreateViewModel()
        {
            Header = new OrderHeaderModel();
            Lines = new List<OrderLineModel>();
            OrderTypes = new List<SelectListItem>();
            Clients = new List<SelectListItem>();
            Items = new List<OrderItemViewModel>();
            Units = new List<SelectListItem>();
            PurchasePrice = new List<SelectListItem>();
            SalePrice = new List<SelectListItem>();

        }

        public OrderHeaderModel Header { get; set; }
        public List<OrderLineModel> Lines { get; set; }
        public List<SelectListItem> OrderTypes { get; set; }
        public List<SelectListItem> Clients { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
        public List<SelectListItem> Units { get; set; }
        public List<SelectListItem> PurchasePrice { get; set; }
        public List<SelectListItem> SalePrice { get; set; }






    }
}
