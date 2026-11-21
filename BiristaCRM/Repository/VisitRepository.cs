using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.Services;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class VisitRepository : IVisitService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly TenantScope _tenant;

        public VisitRepository(ApplicationDbContext db, ICurrentUserService current, TenantScope tenant)
        {
            _db = db;
            _current = current;
            _tenant = tenant;
        }

        private async Task<IQueryable<VisitMaster>> ScopedVisitsAsync()
        {
            var query = _db.VisitMasters.Where(v => !v.IsDeleted);

            if (_current.IsSuperAdmin)
            {
                return query;
            }

            query = query.Where(v => v.CompanyId == _current.CompanyId);

            if (_current.IsSalesPerson || _current.IsSalesManager)
            {
                var ids = await _tenant.GetVisibleSalesPersonIdsAsync();
                query = query.Where(v => ids.Contains(v.SalesPersonId));
            }

            return query;
        }

        public async Task<List<VisitListItemViewModel>> GetVisitsAsync(VisitFilterViewModel? filter = null)
        {
            var scoped = await ScopedVisitsAsync();

            var query =
                from v in scoped
                join c in _db.ClientMasters on v.ClientId equals c.Id
                join p in _db.ProductMasters on v.ProductId equals p.Id
                join u in _db.UserMasters on v.SalesPersonId equals u.Id
                select new { v, c, p, u };

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim().ToLower();
                    query = query.Where(x =>
                        x.c.BusinessName.ToLower().Contains(term) ||
                        x.p.ProductName.ToLower().Contains(term) ||
                        x.u.FullName.ToLower().Contains(term));
                }

                if (filter.Status.HasValue)
                {
                    query = query.Where(x => x.v.Status == filter.Status.Value);
                }

                if (filter.FromDate.HasValue)
                {
                    var from = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                    query = query.Where(x => x.v.VisitDate >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    query = query.Where(x => x.v.VisitDate < to);
                }
            }

            return await query
                .OrderByDescending(x => x.v.VisitDate)
                .Select(x => new VisitListItemViewModel
                {
                    Id = x.v.Id,
                    ClientName = x.c.BusinessName,
                    ProductName = x.p.ProductName,
                    SalesPersonName = x.u.FullName,
                    VisitDate = x.v.VisitDate,
                    Status = x.v.Status,
                    CustomerResponse = x.v.CustomerResponse,
                    IsSampleGiven = x.v.IsSampleGiven,
                    NextFollowUpDate = x.v.NextFollowUpDate,
                    Remark = x.v.Remark
                }).ToListAsync();
        }

        public async Task<VisitMaster?> GetByIdAsync(Guid id)
        {
            var scoped = await ScopedVisitsAsync();
            return await scoped.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task CreateVisitAsync(CreateVisitViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            EnsureClientAndProduct(model.ClientId, model.ProductId);

            var visit = new VisitMaster
            {
                CompanyId = _current.CompanyId,
                ClientId = model.ClientId,
                ProductId = model.ProductId,
                SalesPersonId = _current.UserId,
                VisitDate = DateTime.SpecifyKind(model.VisitDate, DateTimeKind.Utc),
                Status = model.Status,
                IsSampleGiven = model.IsSampleGiven,
                SampleQuantity = model.SampleQuantity,
                CurrentBuyingPrice = model.CurrentBuyingPrice,
                OfferPrice = model.OfferPrice,
                MonthlyRequirement = model.MonthlyRequirement,
                NextFollowUpDate = model.NextFollowUpDate.HasValue
                    ? DateTime.SpecifyKind(model.NextFollowUpDate.Value, DateTimeKind.Utc)
                    : null,
                CustomerResponse = model.CustomerResponse,
                Remark = model.Remark,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.VisitMasters.AddAsync(visit);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateVisitAsync(Guid id, CreateVisitViewModel model)
        {
            var visit = await GetByIdAsync(id) ?? throw new InvalidOperationException("Visit not found.");
            EnsureClientAndProduct(model.ClientId, model.ProductId);

            visit.ClientId = model.ClientId;
            visit.ProductId = model.ProductId;
            visit.VisitDate = DateTime.SpecifyKind(model.VisitDate, DateTimeKind.Utc);
            visit.Status = model.Status;
            visit.IsSampleGiven = model.IsSampleGiven;
            visit.SampleQuantity = model.SampleQuantity;
            visit.CurrentBuyingPrice = model.CurrentBuyingPrice;
            visit.OfferPrice = model.OfferPrice;
            visit.MonthlyRequirement = model.MonthlyRequirement;
            visit.NextFollowUpDate = model.NextFollowUpDate.HasValue
                ? DateTime.SpecifyKind(model.NextFollowUpDate.Value, DateTimeKind.Utc)
                : null;
            visit.CustomerResponse = model.CustomerResponse;
            visit.Remark = model.Remark;
            visit.UpdatedAt = DateTime.UtcNow;
            visit.UpdatedBy = _current.UserId;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteVisitAsync(Guid id)
        {
            var visit = await GetByIdAsync(id);
            if (visit == null)
            {
                return;
            }

            visit.IsDeleted = true;
            visit.UpdatedAt = DateTime.UtcNow;
            visit.UpdatedBy = _current.UserId;
            await _db.SaveChangesAsync();
        }

        public async Task<(int Today, int Completed, int FollowUp, int New)> GetStatsAsync()
        {
            var scoped = await ScopedVisitsAsync();
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var list = await scoped.ToListAsync();
            return (
                list.Count(v => v.VisitDate >= today && v.VisitDate < tomorrow),
                list.Count(v => v.Status is VisitStatus.OrderBooked or VisitStatus.Interested or VisitStatus.NotInterested),
                list.Count(v => v.Status == VisitStatus.FollowUp),
                list.Count(v => v.Status == VisitStatus.New)
            );
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
