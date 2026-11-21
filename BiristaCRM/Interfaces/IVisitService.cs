using BiristaCRM.Models;
using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IVisitService
    {
        Task<List<VisitListItemViewModel>> GetVisitsAsync(VisitFilterViewModel? filter = null);
        Task<VisitMaster?> GetByIdAsync(Guid id);
        Task CreateVisitAsync(CreateVisitViewModel model);
        Task UpdateVisitAsync(Guid id, CreateVisitViewModel model);
        Task SoftDeleteVisitAsync(Guid id);
        Task<(int Today, int Completed, int FollowUp, int New)> GetStatsAsync();
    }
}
