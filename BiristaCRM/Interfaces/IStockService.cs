using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IStockService
    {
        Task<List<StockMaster>> GetStocksAsync(StockFilterViewModel? filter = null);
        Task<StockMaster?> GetByIdAsync(Guid id);
        Task CreateStockAsync(CreateStockViewModel model);
        Task UpdateStockAsync(Guid id, CreateStockViewModel model);
        Task SoftDeleteStockAsync(Guid id);
        Task<StockStatsViewModel> GetStatsAsync(StockFilterViewModel? filter = null);
        Task<List<string>> GetDistinctSendersAsync();
    }
}
