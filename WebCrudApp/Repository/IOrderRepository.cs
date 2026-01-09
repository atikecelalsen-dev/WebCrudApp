using WebCrudApp.Models.Order;

public interface IOrderRepository
{
    List<OrderHeaderModel> GetOrders();
    void CreateOrder(OrderCreateViewModel model);
}
