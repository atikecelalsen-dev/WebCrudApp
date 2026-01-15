using Library.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Models.Invoice;

namespace Library.Repository
{
    public interface IInvoiceRepositorycs
    {
        List<InvoiceHeaderModel> GetOrders();
        void CreateOrder(InvoiceCreateViewModel model);
    }
}
