using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.Services;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class CompanyRepository : ICompanyService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public CompanyRepository(ApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        public async Task<CompanyIndexViewModel> GetCompanyPageAsync(string? search = null, bool? isActive = null)
        {
            var vm = new CompanyIndexViewModel
            {
                IsSuperAdmin = _current.IsSuperAdmin,
                Search = search,
                IsActive = isActive
            };

            if (_current.IsSuperAdmin)
            {
                var query = _db.CompanyMasters.Where(c => !c.IsDeleted);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(c =>
                        c.CompanyName.ToLower().Contains(term) ||
                        c.OwnerName.ToLower().Contains(term) ||
                        c.City.ToLower().Contains(term) ||
                        c.Email.ToLower().Contains(term));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(c => c.IsActive == isActive.Value);
                }

                var companies = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

                var userCounts = await _db.UserMasters
                    .Where(u => !u.IsDeleted)
                    .GroupBy(u => u.CompanyId)
                    .Select(g => new { CompanyId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CompanyId, x => x.Count);

                vm.Companies = companies.Select(c => new CompanyListItemViewModel
                {
                    Id = c.Id,
                    CompanyName = c.CompanyName,
                    OwnerName = c.OwnerName,
                    City = c.City,
                    Email = c.Email,
                    Mobile = c.Mobile,
                    IsActive = c.IsActive,
                    UserCount = userCounts.GetValueOrDefault(c.Id),
                    CreatedAt = c.CreatedAt
                }).ToList();

                return vm;
            }

            var company = await _db.CompanyMasters
                .FirstOrDefaultAsync(c => c.Id == _current.CompanyId && !c.IsDeleted);

            if (company != null)
            {
                vm.CurrentCompany = new UpdateCompanyViewModel
                {
                    Id = company.Id,
                    CompanyName = company.CompanyName,
                    OwnerName = company.OwnerName,
                    Mobile = company.Mobile,
                    Email = company.Email,
                    Address = company.Address,
                    City = company.City,
                    IsActive = company.IsActive,
                    CreatedAt = company.CreatedAt,
                    UserCount = await _db.UserMasters.CountAsync(u => u.CompanyId == company.Id && !u.IsDeleted),
                    ClientCount = await _db.ClientMasters.CountAsync(c => c.CompanyId == company.Id && !c.IsDeleted),
                    ProductCount = await _db.ProductMasters.CountAsync(p => p.CompanyId == company.Id && !p.IsDeleted)
                };
            }

            return vm;
        }

        public async Task CreateCompanyWithAdminAsync(CreateCompanyViewModel model)
        {
            if (!_current.IsSuperAdmin)
            {
                throw new UnauthorizedAccessException("Only Super Admin can create companies.");
            }

            var emailTaken = await _db.UserMasters.AnyAsync(u => u.Email == model.AdminEmail && !u.IsDeleted);
            if (emailTaken)
            {
                throw new InvalidOperationException("Admin email is already registered.");
            }

            var company = new CompanyMaster
            {
                CompanyName = model.CompanyName,
                OwnerName = model.OwnerName,
                Mobile = model.Mobile,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                IsActive = model.IsActive,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.CompanyMasters.AddAsync(company);
            await _db.SaveChangesAsync();

            var admin = new UserMaster
            {
                CompanyId = company.Id,
                FullName = model.AdminFullName,
                Email = model.AdminEmail,
                Password = PasswordHasher.Hash(model.AdminPassword),
                Mobile = model.AdminMobile,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.UserMasters.AddAsync(admin);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateCompanyAsync(UpdateCompanyViewModel model)
        {
            CompanyMaster? company;

            if (_current.IsSuperAdmin)
            {
                company = await _db.CompanyMasters.FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsDeleted);
            }
            else if (_current.IsCompanyAdmin)
            {
                company = await _db.CompanyMasters.FirstOrDefaultAsync(c => c.Id == _current.CompanyId && !c.IsDeleted);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }

            if (company == null)
            {
                throw new InvalidOperationException("Company not found.");
            }

            company.CompanyName = model.CompanyName;
            company.OwnerName = model.OwnerName;
            company.Mobile = model.Mobile;
            company.Email = model.Email;
            company.Address = model.Address;
            company.City = model.City;
            company.IsActive = model.IsActive;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = _current.UserId;

            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteCompanyAsync(Guid id)
        {
            if (!_current.IsSuperAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var company = await _db.CompanyMasters.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (company == null)
            {
                return;
            }

            company.IsDeleted = true;
            company.IsActive = false;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = _current.UserId;
            await _db.SaveChangesAsync();
        }
    }
}
