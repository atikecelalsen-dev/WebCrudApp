using Library.Models.Order;

namespace Library.Repository
{
    public interface IOrderRepository
    {
        List<OrderHeaderModel> GetOrders();
        void CreateOrder(OrderCreateViewModel model);
        OrderCreateViewModel? GetOrderForEdit(int ordFicheRef);
        void UpdateOrderHeader(OrderHeaderModel header);
        void UpdateOrderLines(int headerRef, List<OrderLineModel> lines, OrderHeaderModel header);
        bool DeleteOrder(int logicalRef);
    }
}
