using BiristaCRM.Data;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class ProductRepository : IProductService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public ProductRepository(ApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        private IQueryable<ProductMaster> Scoped()
        {
            var query = _db.ProductMasters.Where(p => !p.IsDeleted);
            if (!_current.IsSuperAdmin)
            {
                query = query.Where(p => p.CompanyId == _current.CompanyId);
            }
            return query;
        }

        public async Task<List<ProductMaster>> GetProductsAsync(ProductFilterViewModel? filter = null)
        {
            var query = Scoped();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim().ToLower();
                    query = query.Where(p =>
                        p.ProductName.ToLower().Contains(term) ||
                        (p.ProductCode != null && p.ProductCode.ToLower().Contains(term)));
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == filter.IsActive.Value);
                }
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<ProductMaster?> GetByIdAsync(Guid id)
        {
            return await Scoped().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateProductAsync(CreateProductViewModel model)
        {
            if (_current.IsSuperAdmin || _current.IsSalesPerson)
            {
                throw new UnauthorizedAccessException("Only company admins/managers can manage products.");
            }

            var product = new ProductMaster
            {
                CompanyId = _current.CompanyId,
                ProductName = model.ProductName,
                ProductCode = model.ProductCode,
                Description = model.Description,
                Unit = model.Unit,
                Price = model.Price,
                IsActive = model.IsActive,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.ProductMasters.AddAsync(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Guid id, CreateProductViewModel model)
        {
            var product = await GetByIdAsync(id) ?? throw new InvalidOperationException("Product not found.");

            product.ProductName = model.ProductName;
            product.ProductCode = model.ProductCode;
            product.Description = model.Description;
            product.Unit = model.Unit;
            product.Price = model.Price;
            product.IsActive = model.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = _current.UserId;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteProductAsync(Guid id)
        {
            var product = await GetByIdAsync(id);
            if (product == null)
            {
                return;
            }

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = _current.UserId;
            await _db.SaveChangesAsync();
        }
    }
}
