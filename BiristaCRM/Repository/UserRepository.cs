using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.Models;
using BiristaCRM.Services;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class UserRepository : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public UserRepository(ApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        public async Task<List<UserMaster>> GetUsersAsync(UserFilterViewModel? filter = null)
        {
            var query = _db.UserMasters.Where(u => !u.IsDeleted);

            if (_current.IsSuperAdmin)
            {
                query = query.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin);
            }
            else if (_current.IsCompanyAdmin)
            {
                query = query.Where(u => u.CompanyId == _current.CompanyId && u.Role != UserRole.SuperAdmin);
            }
            else if (_current.IsSalesManager)
            {
                query = query.Where(u => u.CompanyId == _current.CompanyId &&
                                        (u.Id == _current.UserId || u.ManagerId == _current.UserId));
            }
            else
            {
                query = query.Where(u => u.Id == _current.UserId);
            }

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim().ToLower();
                    query = query.Where(u =>
                        u.FullName.ToLower().Contains(term) ||
                        u.Email.ToLower().Contains(term) ||
                        u.Mobile.Contains(term));
                }

                if (filter.Role.HasValue)
                {
                    query = query.Where(u => u.Role == filter.Role.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == filter.IsActive.Value);
                }
            }

            return await query.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();
        }

        public async Task<List<UserMaster>> GetManagersAsync()
        {
            if (_current.IsSuperAdmin)
            {
                return new List<UserMaster>();
            }

            return await _db.UserMasters
                .Where(u => !u.IsDeleted && u.CompanyId == _current.CompanyId &&
                            (u.Role == UserRole.Admin || u.Role == UserRole.SalesManager) &&
                            u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task CreateUserAsync(CreateUserViewModel model)
        {
            if (!_current.CanManageUsers)
            {
                throw new UnauthorizedAccessException();
            }

            var emailTaken = await _db.UserMasters.AnyAsync(u => u.Email == model.Email && !u.IsDeleted);
            if (emailTaken)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            Guid companyId;
            UserRole role = model.Role;

            if (_current.IsSuperAdmin)
            {
                if (!model.CompanyId.HasValue || model.CompanyId == Guid.Empty)
                {
                    throw new InvalidOperationException("Company is required.");
                }

                companyId = model.CompanyId.Value;
                role = UserRole.Admin;
            }
            else
            {
                companyId = _current.CompanyId;

                if (_current.IsSalesManager)
                {
                    role = UserRole.SalesPerson;
                }
                else if (role is UserRole.SuperAdmin or UserRole.Admin)
                {
                    throw new InvalidOperationException("Invalid role selection.");
                }
            }

            var user = new UserMaster
            {
                CompanyId = companyId,
                ManagerId = role == UserRole.SalesPerson
                    ? (model.ManagerId ?? (_current.IsSalesManager ? _current.UserId : null))
                    : null,
                FullName = model.FullName,
                Email = model.Email,
                Password = PasswordHasher.Hash(model.Password),
                Mobile = model.Mobile,
                Role = role,
                IsActive = model.IsActive,
                CreatedBy = _current.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.UserMasters.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(Guid id, CreateUserViewModel model)
        {
            var users = await GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id)
                       ?? throw new InvalidOperationException("User not found.");

            if (user.Role == UserRole.SuperAdmin && !_current.IsSuperAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var emailTaken = await _db.UserMasters.AnyAsync(u => u.Email == model.Email && u.Id != id && !u.IsDeleted);
            if (emailTaken)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.IsActive = model.IsActive;
            user.ManagerId = model.ManagerId;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _current.UserId;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.Password = PasswordHasher.Hash(model.Password);
            }

            if (_current.IsCompanyAdmin && model.Role is UserRole.SalesManager or UserRole.SalesPerson)
            {
                user.Role = model.Role;
            }

            await _db.SaveChangesAsync();
        }

        public async Task SetActiveAsync(Guid id, bool isActive)
        {
            var users = await GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.Id == _current.UserId)
            {
                return;
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _current.UserId;
            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteUserAsync(Guid id)
        {
            var users = await GetUsersAsync();
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.Id == _current.UserId || user.Role == UserRole.SuperAdmin)
            {
                return;
            }

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _current.UserId;
            await _db.SaveChangesAsync();
        }
    }
}
