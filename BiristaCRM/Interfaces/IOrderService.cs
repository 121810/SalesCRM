using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderListItemViewModel>> GetOrdersAsync(OrderFilterViewModel? filter = null);
        Task<OrderMaster?> GetByIdAsync(Guid id);
        Task CreateOrderAsync(CreateOrderViewModel model);
        Task UpdateOrderAsync(Guid id, CreateOrderViewModel model);
        Task SoftDeleteOrderAsync(Guid id);
        Task<(int Count, decimal Revenue, int AwaitingPayment)> GetStatsAsync();
    }
}
