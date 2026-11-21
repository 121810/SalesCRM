using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyIndexViewModel> GetCompanyPageAsync(string? search = null, bool? isActive = null);
        Task CreateCompanyWithAdminAsync(CreateCompanyViewModel model);
        Task UpdateCompanyAsync(UpdateCompanyViewModel model);
        Task SoftDeleteCompanyAsync(Guid id);
    }
}
