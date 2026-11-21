using BiristaCRM.ViewModels;

namespace BiristaCRM.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardAsync();
        Task<ReportViewModel> GetReportAsync(ReportFilterViewModel? filter = null);
    }
}
