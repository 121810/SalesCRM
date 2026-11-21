using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.Services;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class OrderRepository : IOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly TenantScope _tenant;

        public OrderRepository(ApplicationDbContext db, ICurrentUserService current, TenantScope tenant)
        {
            _db = db;
            _current = current;
            _tenant = tenant;
        }

        private async Task<IQueryable<OrderMaster>> ScopedOrdersAsync()
        {
            var query = _db.OrderMasters.Where(o => !o.IsDeleted);

            if (_current.IsSuperAdmin)
            {
                return query;
            }

            query = query.Where(o => o.CompanyId == _current.CompanyId);

            if (_current.IsSalesPerson || _current.IsSalesManager)
            {
                var ids = await _tenant.GetVisibleSalesPersonIdsAsync();
                query = query.Where(o => ids.Contains(o.SalesPersonId));
            }

            return query;
        }

        public async Task<List<OrderListItemViewModel>> GetOrdersAsync(OrderFilterViewModel? filter = null)
        {
            var scoped = await ScopedOrdersAsync();

            var query =
                from o in scoped
                join c in _db.ClientMasters on o.ClientId equals c.Id
                join p in _db.ProductMasters on o.ProductId equals p.Id
                join u in _db.UserMasters on o.SalesPersonId equals u.Id
                select new { o, c, p, u };

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim().ToLower();
                    query = query.Where(x =>
                        x.o.OrderNo.ToLower().Contains(term) ||
                        x.c.BusinessName.ToLower().Contains(term) ||
                        x.p.ProductName.ToLower().Contains(term));
                }

                if (filter.PaymentStatus.HasValue)
                {
                    query = query.Where(x => x.o.PaymentStatus == filter.PaymentStatus.Value);
                }

                if (filter.PaymentMode.HasValue)
                {
                    query = query.Where(x => x.o.PaymentMode == filter.PaymentMode.Value);
                }

                if (filter.FromDate.HasValue)
                {
                    var from = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(x => x.o.OrderDate >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    query = query.Where(x => x.o.OrderDate < to);
                }
            }

            return await query
                .OrderByDescending(x => x.o.OrderDate)
                .Select(x => new OrderListItemViewModel
                {
                    Id = x.o.Id,
                    OrderNo = x.o.OrderNo,
                    ClientName = x.c.BusinessName,
                    ProductName = x.p.ProductName,
                    SalesPersonName = x.u.FullName,
                    OrderDate = x.o.OrderDate,
                    Quantity = x.o.Quantity,
                    Amount = x.o.Amount,
                    PaymentMode = x.o.PaymentMode,
                    PaymentStatus = x.o.PaymentStatus
                }).ToListAsync();
        }

        public async Task<OrderMaster?> GetByIdAsync(Guid id)
        {
            var scoped = await ScopedOrdersAsync();
            return await scoped.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task CreateOrderAsync(CreateOrderViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            EnsureClientAndProduct(model.ClientId, model.ProductId);

            var orderNo = await GenerateOrderNoAsync();

            var order = new OrderMaster
            {
                CompanyId = _current.CompanyId,
                ClientId = model.ClientId,
                ProductId = model.ProductId,
                SalesPersonId = _current.UserId,
                OrderNo = orderNo,
                OrderDate = DateTime.SpecifyKind(model.OrderDate, DateTimeKind.Utc),
                Quantity = model.Quantity,
                Rate = model.Rate,
                Amount = model.Quantity * model.Rate,
                PaymentMode = model.PaymentMode,
                PaymentStatus = model.PaymentStatus,
                PaymentDate = model.PaymentDate.HasValue
                    ? DateTime.SpecifyKind(model.PaymentDate.Value, DateTimeKind.Utc)
                    : null,
                Remark = model.Remark,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.OrderMasters.AddAsync(order);

            var client = await _db.ClientMasters.FirstAsync(c => c.Id == model.ClientId);
            if (client.Status == ClientStatus.Prospect)
            {
                client.Status = ClientStatus.Customer;
                client.UpdatedAt = DateTime.UtcNow;
                client.UpdatedBy = _current.UserId;
            }

            await _db.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(Guid id, CreateOrderViewModel model)
        {
            var order = await GetByIdAsync(id) ?? throw new InvalidOperationException("Order not found.");
            EnsureClientAndProduct(model.ClientId, model.ProductId);

            order.ClientId = model.ClientId;
            order.ProductId = model.ProductId;
            order.OrderDate = DateTime.SpecifyKind(model.OrderDate, DateTimeKind.Utc);
            order.Quantity = model.Quantity;
            order.Rate = model.Rate;
            order.Amount = model.Quantity * model.Rate;
            order.PaymentMode = model.PaymentMode;
            order.PaymentStatus = model.PaymentStatus;
            order.PaymentDate = model.PaymentDate.HasValue
                ? DateTime.SpecifyKind(model.PaymentDate.Value, DateTimeKind.Utc)
                : null;
            order.Remark = model.Remark;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _current.UserId;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteOrderAsync(Guid id)
        {
            var order = await GetByIdAsync(id);
            if (order == null)
            {
                return;
            }

            order.IsDeleted = true;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _current.UserId;
            await _db.SaveChangesAsync();
        }

        public async Task<(int Count, decimal Revenue, int AwaitingPayment)> GetStatsAsync()
        {
            var scoped = await ScopedOrdersAsync();
            var list = await scoped.ToListAsync();
            return (
                list.Count,
                list.Sum(o => o.Amount),
                list.Count(o => o.PaymentStatus != PaymentStatus.Paid && o.PaymentStatus != PaymentStatus.Cancelled)
            );
        }

        private async Task<string> GenerateOrderNoAsync()
        {
            var count = await _db.OrderMasters.CountAsync(o => o.CompanyId == _current.CompanyId);
            return $"ORD-{(_current.CompanyId.ToString("N")[..4].ToUpper())}-{count + 1:D4}";
        }

        private void EnsureClientAndProduct(Guid clientId, Guid productId)
        {
            var clientOk = _db.ClientMasters.Any(c => c.Id == clientId && !c.IsDeleted && c.CompanyId == _current.CompanyId);
            var productOk = _db.ProductMasters.Any(p => p.Id == productId && !p.IsDeleted && p.CompanyId == _current.CompanyId);
            if (!clientOk || !productOk)
            {
                throw new InvalidOperationException("Invalid customer or product for this company.");
            }
        }
    }
}
