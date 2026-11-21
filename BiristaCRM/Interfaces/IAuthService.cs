using BiristaCRM.Enums;
using BiristaCRM.Models;

namespace BiristaCRM.Interfaces
{
    public interface IAuthService
    {
        Task<UserMaster?> ValidateLoginAsync(string email, string password);
        Task SignInAsync(UserMaster user, bool rememberMe);
        Task SignOutAsync();
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task UpdateProfileAsync(Guid userId, string fullName, string mobile);
        Task<UserMaster?> GetUserByIdAsync(Guid userId);
    }
}
