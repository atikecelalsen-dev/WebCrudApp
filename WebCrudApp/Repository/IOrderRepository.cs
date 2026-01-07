using WebCrudApp.Models;

public interface IOrderRepository
{
    List<OrderHeaderModel> GetOrders();
    void CreateOrder(OrderCreateViewModel model);
}
