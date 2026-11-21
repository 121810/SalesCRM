using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IUserService
    {
        Task<List<UserMaster>> GetUsersAsync(UserFilterViewModel? filter = null);
        Task CreateUserAsync(CreateUserViewModel model);
        Task UpdateUserAsync(Guid id, CreateUserViewModel model);
        Task SetActiveAsync(Guid id, bool isActive);
        Task SoftDeleteUserAsync(Guid id);
        Task<List<UserMaster>> GetManagersAsync();
    }
}
