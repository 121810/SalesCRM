using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductMaster>> GetProductsAsync(ProductFilterViewModel? filter = null);
        Task<ProductMaster?> GetByIdAsync(Guid id);
        Task CreateProductAsync(CreateProductViewModel model);
        Task UpdateProductAsync(Guid id, CreateProductViewModel model);
        Task SoftDeleteProductAsync(Guid id);
    }
}
