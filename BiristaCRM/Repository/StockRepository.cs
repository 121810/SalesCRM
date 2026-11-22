using BiristaCRM.Data;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class StockRepository : IStockService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public StockRepository(ApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        private IQueryable<StockMaster> Scoped()
        {
            var query = _db.StockMasters.Where(s => !s.IsDeleted);
            if (!_current.IsSuperAdmin)
            {
                query = query.Where(s => s.CompanyId == _current.CompanyId);
            }
            return query;
        }

        public async Task<List<StockMaster>> GetStocksAsync(StockFilterViewModel? filter = null)
        {
            var query = Scoped();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim().ToLower();
                    query = query.Where(s =>
                        s.ProductName.ToLower().Contains(term) ||
                        (s.ProductSubType != null && s.ProductSubType.ToLower().Contains(term)) ||
                        s.ProductSender.ToLower().Contains(term) ||
                        (s.VehicleType != null && s.VehicleType.ToLower().Contains(term)) ||
                        (s.VehicleNo != null && s.VehicleNo.ToLower().Contains(term)));
                }

                if (!string.IsNullOrWhiteSpace(filter.ProductSender))
                {
                    query = query.Where(s => s.ProductSender == filter.ProductSender);
                }

                if (!string.IsNullOrWhiteSpace(filter.VehicleType))
                {
                    query = query.Where(s => s.VehicleType == filter.VehicleType);
                }

                if (filter.FromDate.HasValue)
                {
                    var from = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(s => s.Date >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    query = query.Where(s => s.Date < to);
                }
            }

            var stocks = await query.OrderByDescending(s => s.Date).ToListAsync();

            // Populate unit directly from ProductMaster for linked catalog products
            var productIds = stocks.Where(s => s.ProductId.HasValue)
                .Select(s => s.ProductId!.Value)
                .Distinct()
                .ToList();

            if (productIds.Any())
            {
                var prodUnits = await _db.ProductMasters
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Unit);

                foreach (var s in stocks.Where(s => s.ProductId.HasValue))
                {
                    if (prodUnits.TryGetValue(s.ProductId!.Value, out var u) && !string.IsNullOrWhiteSpace(u))
                    {
                        s.Unit = u;
                    }
                }
            }

            return stocks;
        }

        public async Task<StockMaster?> GetByIdAsync(Guid id)
        {
            var stock = await Scoped().FirstOrDefaultAsync(s => s.Id == id);
            if (stock != null && string.IsNullOrWhiteSpace(stock.Unit) && stock.ProductId.HasValue)
            {
                stock.Unit = await _db.ProductMasters.Where(p => p.Id == stock.ProductId.Value).Select(p => p.Unit).FirstOrDefaultAsync();
            }
            return stock;
        }

        public async Task CreateStockAsync(CreateStockViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                throw new InvalidOperationException("Super Admin cannot record stock directly.");
            }

            var totalPrice = model.Price * model.Qty;
            var grandTotal = totalPrice + model.FactoryToTransportCost + model.TransportToCompanyCost;

            // Unit is taken directly from ProductMaster when Product is linked
            string? unit = null;
            if (model.ProductId.HasValue)
            {
                unit = await _db.ProductMasters.Where(p => p.Id == model.ProductId.Value).Select(p => p.Unit).FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrWhiteSpace(model.Unit))
            {
                unit = model.Unit.Trim();
            }

            var stock = new StockMaster
            {
                CompanyId = _current.CompanyId,
                ProductId = model.ProductId,
                ProductName = model.ProductName.Trim(),
                ProductSubType = model.ProductSubType?.Trim(),
                Price = model.Price,
                Qty = model.Qty,
                Unit = unit,
                TotalPrice = totalPrice,
                FactoryToTransportCost = model.FactoryToTransportCost,
                TransportToCompanyCost = model.TransportToCompanyCost,
                GrandTotalCost = grandTotal,
                VehicleType = model.VehicleType?.Trim(),
                VehicleNo = model.VehicleNo?.Trim(),
                Date = DateTime.SpecifyKind(model.Date.Date, DateTimeKind.Utc),
                ProductSender = model.ProductSender.Trim(),
                Remark = model.Remark?.Trim(),
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.StockMasters.AddAsync(stock);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStockAsync(Guid id, CreateStockViewModel model)
        {
            var stock = await GetByIdAsync(id) ?? throw new InvalidOperationException("Stock record not found.");

            var totalPrice = model.Price * model.Qty;
            var grandTotal = totalPrice + model.FactoryToTransportCost + model.TransportToCompanyCost;

            // Unit is taken directly from ProductMaster when Product is linked
            string? unit = null;
            if (model.ProductId.HasValue)
            {
                unit = await _db.ProductMasters.Where(p => p.Id == model.ProductId.Value).Select(p => p.Unit).FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrWhiteSpace(model.Unit))
            {
                unit = model.Unit.Trim();
            }

            stock.ProductId = model.ProductId;
            stock.ProductName = model.ProductName.Trim();
            stock.ProductSubType = model.ProductSubType?.Trim();
            stock.Price = model.Price;
            stock.Qty = model.Qty;
            stock.Unit = unit;
            stock.TotalPrice = totalPrice;
            stock.FactoryToTransportCost = model.FactoryToTransportCost;
            stock.TransportToCompanyCost = model.TransportToCompanyCost;
            stock.GrandTotalCost = grandTotal;
            stock.VehicleType = model.VehicleType?.Trim();
            stock.VehicleNo = model.VehicleNo?.Trim();
            stock.Date = DateTime.SpecifyKind(model.Date.Date, DateTimeKind.Utc);
            stock.ProductSender = model.ProductSender.Trim();
            stock.Remark = model.Remark?.Trim();
            stock.UpdatedAt = DateTime.UtcNow;
            stock.UpdatedBy = _current.UserId;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteStockAsync(Guid id)
        {
            var stock = await GetByIdAsync(id);
            if (stock != null)
            {
                stock.IsDeleted = true;
                stock.UpdatedAt = DateTime.UtcNow;
                stock.UpdatedBy = _current.UserId;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<StockStatsViewModel> GetStatsAsync(StockFilterViewModel? filter = null)
        {
            var list = await GetStocksAsync(filter);
            return new StockStatsViewModel
            {
                TotalEntries = list.Count,
                TotalQuantity = list.Sum(s => s.Qty),
                TotalValue = list.Sum(s => s.TotalPrice),
                TotalTransportCost = list.Sum(s => s.FactoryToTransportCost + s.TransportToCompanyCost),
                TotalLandedCost = list.Sum(s => s.GrandTotalCost)
            };
        }

        public async Task<List<string>> GetDistinctSendersAsync()
        {
            return await Scoped()
                .Where(s => !string.IsNullOrWhiteSpace(s.ProductSender))
                .Select(s => s.ProductSender)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }
    }
}
