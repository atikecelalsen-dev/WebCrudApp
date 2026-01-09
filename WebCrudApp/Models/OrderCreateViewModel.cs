using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Reflection;
using WebCrudApp.Models;

namespace WebCrudApp.Models
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
        }

        public OrderHeaderModel Header { get; set; }
        public List<OrderLineModel> Lines { get; set; }
        public List<SelectListItem> OrderTypes { get; set; }
        public List<SelectListItem> Clients { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
        public List<SelectListItem> Units { get; set; } 







    }
}
