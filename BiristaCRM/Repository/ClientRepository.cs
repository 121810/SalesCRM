using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.Services;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class ClientRepository : IClientService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _current;
        private readonly TenantScope _tenant;

        public ClientRepository(ApplicationDbContext dbContext, ICurrentUserService current, TenantScope tenant)
        {
            _dbContext = dbContext;
            _current = current;
            _tenant = tenant;
        }

        private async Task<IQueryable<ClientMaster>> ScopedClientsAsync()
        {
            var query = _dbContext.ClientMasters.Where(x => !x.IsDeleted);

            if (_current.IsSuperAdmin)
            {
                return query;
            }

            query = query.Where(x => x.CompanyId == _current.CompanyId);

            if (_current.IsSalesPerson || _current.IsSalesManager)
            {
                var visibleIds = await _tenant.GetVisibleSalesPersonIdsAsync();
                query = query.Where(x => visibleIds.Contains(x.SalesPersonId));
            }

            return query;
        }

        public async Task<List<ClientMaster>> GetAllClients(ClientFilterViewModel? filter = null)
        {
            var query = await ScopedClientsAsync();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim().ToLower();
                    query = query.Where(x =>
                        x.BusinessName.ToLower().Contains(term) ||
                        x.OwnerName.ToLower().Contains(term) ||
                        x.MobileNumber.Contains(term) ||
                        (x.Email != null && x.Email.ToLower().Contains(term)));
                }

                if (!string.IsNullOrWhiteSpace(filter.Area))
                {
                    query = query.Where(x => x.Area == filter.Area);
                }

                if (filter.Status.HasValue)
                {
                    query = query.Where(x => x.Status == filter.Status.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == filter.IsActive.Value);
                }
            }

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<List<string>> GetDistinctAreasAsync()
        {
            var query = await ScopedClientsAsync();
            return await query
                .Where(x => !string.IsNullOrWhiteSpace(x.Area))
                .Select(x => x.Area)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<ClientMaster?> GetSingleClientDetails(Guid id)
        {
            var query = await ScopedClientsAsync();
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ClientDetailsViewModel?> GetClientDetailsWithVisits(Guid id)
        {
            var client = await GetSingleClientDetails(id);
            if (client == null)
            {
                return null;
            }

            var visits = await (
                from v in _dbContext.VisitMasters
                join c in _dbContext.ClientMasters on v.ClientId equals c.Id
                join p in _dbContext.ProductMasters on v.ProductId equals p.Id
                where !v.IsDeleted && v.ClientId == id
                orderby v.VisitDate descending
                select new VisitListItemViewModel
                {
                    Id = v.Id,
                    ClientName = c.BusinessName,
                    ProductName = p.ProductName,
                    VisitDate = v.VisitDate,
                    Status = v.Status,
                    CustomerResponse = v.CustomerResponse,
                    IsSampleGiven = v.IsSampleGiven,
                    NextFollowUpDate = v.NextFollowUpDate,
                    Remark = v.Remark
                }).Take(5).ToListAsync();

            return new ClientDetailsViewModel
            {
                Client = client,
                RecentVisits = visits
            };
        }

        public async Task CreateClient(CreateClientViewModel model)
        {
            if (_current.IsSuperAdmin)
            {
                throw new InvalidOperationException("Super Admin cannot create customers. Switch to a company admin account.");
            }

            var client = new ClientMaster
            {
                CompanyId = _current.CompanyId,
                SalesPersonId = _current.IsSalesPerson || _current.IsSalesManager
                    ? _current.UserId
                    : _current.UserId,
                BusinessName = model.BusinessName,
                OwnerName = model.OwnerName,
                Email = model.Email,
                GSTNo = model.GSTNo,
                MobileNumber = model.MobileNumber,
                Area = model.Area,
                Address = model.Address,
                Status = model.Status,
                IsActive = model.IsActive,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            // Company Admin creating a client assigns it to themselves unless they are a salesperson context
            if (_current.IsCompanyAdmin)
            {
                client.SalesPersonId = _current.UserId;
            }

            await _dbContext.ClientMasters.AddAsync(client);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateClient(Guid id, CreateClientViewModel model)
        {
            var client = await GetSingleClientDetails(id);
            if (client == null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            client.BusinessName = model.BusinessName;
            client.OwnerName = model.OwnerName;
            client.Email = model.Email;
            client.GSTNo = model.GSTNo;
            client.MobileNumber = model.MobileNumber;
            client.Area = model.Area;
            client.Address = model.Address;
            client.Status = model.Status;
            client.IsActive = model.IsActive;
            client.UpdatedAt = DateTime.UtcNow;
            client.UpdatedBy = _current.UserId;

            await _dbContext.SaveChangesAsync();
        }

        public async Task SoftDeleteClient(Guid id)
        {
            var client = await GetSingleClientDetails(id);
            if (client == null)
            {
                return;
            }

            client.IsDeleted = true;
            client.UpdatedAt = DateTime.UtcNow;
            client.UpdatedBy = _current.UserId;
            await _dbContext.SaveChangesAsync();
        }
    }
}
