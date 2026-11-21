using BiristaCRM.Interfaces;
using BiristaCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiristaCRM.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public ReportController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ReportFilterViewModel filter)
        {
            filter ??= new ReportFilterViewModel();
            var model = await _dashboardService.GetReportAsync(filter);
            return View(model);
        }
    }
}
