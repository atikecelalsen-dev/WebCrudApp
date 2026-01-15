using Library.Models.Order;

namespace Library.Repository
{
    public interface IOrderRepository
    {
        List<OrderHeaderModel> GetOrders();
        void CreateOrder(OrderCreateViewModel model);
    }
}
