using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Services
{
    public class TenantScope
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public TenantScope(ApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        public async Task<List<Guid>> GetVisibleSalesPersonIdsAsync()
        {
            if (_current.IsSuperAdmin)
            {
                return await _db.UserMasters
                    .Where(u => !u.IsDeleted && u.Role == UserRole.SalesPerson)
                    .Select(u => u.Id)
                    .ToListAsync();
            }

            if (_current.IsCompanyAdmin)
            {
                return await _db.UserMasters
                    .Where(u => !u.IsDeleted && u.CompanyId == _current.CompanyId &&
                                (u.Role == UserRole.SalesPerson || u.Role == UserRole.SalesManager || u.Role == UserRole.Admin))
                    .Select(u => u.Id)
                    .ToListAsync();
            }

            if (_current.IsSalesManager)
            {
                var team = await _db.UserMasters
                    .Where(u => !u.IsDeleted && u.CompanyId == _current.CompanyId &&
                                (u.Id == _current.UserId || u.ManagerId == _current.UserId))
                    .Select(u => u.Id)
                    .ToListAsync();
                return team;
            }

            return new List<Guid> { _current.UserId };
        }

        public IQueryable<T> ApplyCompanyFilter<T>(IQueryable<T> query, System.Linq.Expressions.Expression<Func<T, Guid>> companyIdSelector)
            where T : class
        {
            if (_current.IsSuperAdmin)
            {
                return query;
            }

            var companyId = _current.CompanyId;
            var parameter = companyIdSelector.Parameters[0];
            var body = System.Linq.Expressions.Expression.Equal(
                companyIdSelector.Body,
                System.Linq.Expressions.Expression.Constant(companyId));
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, parameter);
            return query.Where(lambda);
        }
    }
}
