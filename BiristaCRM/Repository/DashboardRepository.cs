using BiristaCRM.Data;
using BiristaCRM.Enums;
using BiristaCRM.Interfaces;
using BiristaCRM.Services;
using BiristaCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BiristaCRM.Repository
{
    public class DashboardRepository : IDashboardService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly TenantScope _tenant;

        public DashboardRepository(ApplicationDbContext db, ICurrentUserService current, TenantScope tenant)
        {
            _db = db;
            _current = current;
            _tenant = tenant;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var visibleSalesIds = await _tenant.GetVisibleSalesPersonIdsAsync();
            var companyId = _current.CompanyId;
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var today = now.Date;
            var tomorrow = today.AddDays(1);

            IQueryable<Models.ClientMaster> clients = _db.ClientMasters.Where(c => !c.IsDeleted);
            IQueryable<Models.VisitMaster> visits = _db.VisitMasters.Where(v => !v.IsDeleted);
            IQueryable<Models.OrderMaster> orders = _db.OrderMasters.Where(o => !o.IsDeleted);

            if (_current.IsSuperAdmin)
            {
                // Platform-wide snapshot for Super Admin
            }
            else
            {
                clients = clients.Where(c => c.CompanyId == companyId);
                visits = visits.Where(v => v.CompanyId == companyId);
                orders = orders.Where(o => o.CompanyId == companyId);

                if (_current.IsSalesPerson || _current.IsSalesManager)
                {
                    clients = clients.Where(c => visibleSalesIds.Contains(c.SalesPersonId));
                    visits = visits.Where(v => visibleSalesIds.Contains(v.SalesPersonId));
                    orders = orders.Where(o => visibleSalesIds.Contains(o.SalesPersonId));
                }
            }

            var clientList = await clients.ToListAsync();
            var visitList = await visits.ToListAsync();
            var orderList = await orders.ToListAsync();

            var greeting = now.Hour switch
            {
                < 12 => "Good morning",
                < 17 => "Good afternoon",
                _ => "Good evening"
            };

            var chart = BuildMonthlyRevenue(orderList);

            var recentOrders = await (
                from o in orders.OrderByDescending(x => x.OrderDate).Take(5)
                join c in _db.ClientMasters on o.ClientId equals c.Id
                select new OrderListItemViewModel
                {
                    Id = o.Id,
                    OrderNo = o.OrderNo,
                    ClientName = c.BusinessName,
                    OrderDate = o.OrderDate,
                    Amount = o.Amount,
                    PaymentMode = o.PaymentMode,
                    PaymentStatus = o.PaymentStatus,
                    Quantity = o.Quantity
                }).ToListAsync();

            var followUps = await (
                from v in visits.Where(v => v.NextFollowUpDate != null &&
                                           v.NextFollowUpDate >= today &&
                                           v.NextFollowUpDate < tomorrow)
                join c in _db.ClientMasters on v.ClientId equals c.Id
                orderby v.NextFollowUpDate
                select new FollowUpItemViewModel
                {
                    ClientName = c.BusinessName,
                    Detail = (v.Remark ?? v.Status.ToString()) + " · " + (v.NextFollowUpDate!.Value.ToString("hh:mm tt")),
                    StatusLabel = v.Status.ToString(),
                    BadgeClass = v.Status == VisitStatus.FollowUp ? "sf-badge-warning" :
                                 v.Status == VisitStatus.OrderBooked ? "sf-badge-success" : "sf-badge-info"
                }).Take(5).ToListAsync();

            var activity = new List<ActivityItemViewModel>();
            foreach (var o in orderList.OrderByDescending(x => x.CreatedAt).Take(3))
            {
                var clientName = clientList.FirstOrDefault(c => c.Id == o.ClientId)?.BusinessName ?? "Client";
                activity.Add(new ActivityItemViewModel
                {
                    Text = $"Order #{o.OrderNo} placed by {clientName}",
                    TimeAgo = ToTimeAgo(o.CreatedAt),
                    Color = "primary"
                });
            }

            foreach (var v in visitList.OrderByDescending(x => x.CreatedAt).Take(3))
            {
                var clientName = clientList.FirstOrDefault(c => c.Id == v.ClientId)?.BusinessName ?? "Client";
                activity.Add(new ActivityItemViewModel
                {
                    Text = $"Visit logged for {clientName} ({v.Status})",
                    TimeAgo = ToTimeAgo(v.CreatedAt),
                    Color = "success"
                });
            }

            foreach (var c in clientList.OrderByDescending(x => x.CreatedAt).Take(2))
            {
                activity.Add(new ActivityItemViewModel
                {
                    Text = $"New client {c.BusinessName} added",
                    TimeAgo = ToTimeAgo(c.CreatedAt),
                    Color = "info"
                });
            }

            return new DashboardViewModel
            {
                UserName = $"{greeting}, {_current.FullName}",
                TotalClients = clientList.Count,
                ClientsThisMonth = clientList.Count(c => c.CreatedAt >= monthStart),
                TodaysVisits = visitList.Count(v => v.VisitDate >= today && v.VisitDate < tomorrow),
                VisitsInProgress = visitList.Count(v => v.Status is VisitStatus.New or VisitStatus.FollowUp),
                OpenOrders = orderList.Count(o => o.PaymentStatus is PaymentStatus.Pending or PaymentStatus.Partial),
                OrdersToday = orderList.Count(o => o.OrderDate >= today && o.OrderDate < tomorrow),
                RevenueMtd = orderList.Where(o => o.OrderDate >= monthStart && o.PaymentStatus != PaymentStatus.Cancelled).Sum(o => o.Amount),
                ChartLabels = chart.Labels,
                ChartValues = chart.Values,
                RecentActivity = activity.OrderBy(a => a.TimeAgo).Take(6).ToList(),
                TodaysFollowUps = followUps,
                RecentOrders = recentOrders
            };
        }

        public async Task<ReportViewModel> GetReportAsync(ReportFilterViewModel? filter = null)
        {
            filter ??= new ReportFilterViewModel();
            var visibleSalesIds = await _tenant.GetVisibleSalesPersonIdsAsync();
            var companyId = _current.CompanyId;

            IQueryable<Models.VisitMaster> visits = _db.VisitMasters.Where(v => !v.IsDeleted);
            IQueryable<Models.OrderMaster> orders = _db.OrderMasters.Where(o => !o.IsDeleted);
            IQueryable<Models.ClientMaster> clients = _db.ClientMasters.Where(c => !c.IsDeleted);

            if (!_current.IsSuperAdmin)
            {
                visits = visits.Where(v => v.CompanyId == companyId);
                orders = orders.Where(o => o.CompanyId == companyId);
                clients = clients.Where(c => c.CompanyId == companyId);

                if (_current.IsSalesPerson || _current.IsSalesManager)
                {
                    visits = visits.Where(v => visibleSalesIds.Contains(v.SalesPersonId));
                    orders = orders.Where(o => visibleSalesIds.Contains(o.SalesPersonId));
                    clients = clients.Where(c => visibleSalesIds.Contains(c.SalesPersonId));
                }
            }

            if (filter.FromDate.HasValue)
            {
                var from = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                visits = visits.Where(v => v.VisitDate >= from);
                orders = orders.Where(o => o.OrderDate >= from);
                clients = clients.Where(c => c.CreatedAt >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                visits = visits.Where(v => v.VisitDate < to);
                orders = orders.Where(o => o.OrderDate < to);
                clients = clients.Where(c => c.CreatedAt < to);
            }

            if (filter.SalesPersonId.HasValue && filter.SalesPersonId != Guid.Empty)
            {
                var spId = filter.SalesPersonId.Value;
                visits = visits.Where(v => v.SalesPersonId == spId);
                orders = orders.Where(o => o.SalesPersonId == spId);
                clients = clients.Where(c => c.SalesPersonId == spId);
            }

            var visitList = await visits.ToListAsync();
            var orderList = await orders.ToListAsync();
            var clientList = await clients.ToListAsync();
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalVisits = visitList.Count;
            var converted = visitList.Count(v => v.Status == VisitStatus.OrderBooked || v.CustomerResponse == CustomerResponse.OrderBooked);
            var completedish = visitList.Count(v => v.Status != VisitStatus.New);
            var collected = orderList.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.Amount);

            var salesPeople = await _db.UserMasters
                .Where(u =>
                    !u.IsDeleted && u.IsActive && u.Role == UserRole.SalesPerson &&
                    (_current.IsSuperAdmin || u.CompanyId == companyId) &&
                    (_current.IsCompanyAdmin || _current.IsSuperAdmin || visibleSalesIds.Contains(u.Id)))
                .OrderBy(u => u.FullName)
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var performers = salesPeople
                .Where(s => !filter.SalesPersonId.HasValue || filter.SalesPersonId == Guid.Empty || s.Id == filter.SalesPersonId)
                .Select(s => new PerformerViewModel
                {
                    Name = s.FullName,
                    VisitCount = visitList.Count(v => v.SalesPersonId == s.Id),
                    OrderCount = orderList.Count(o => o.SalesPersonId == s.Id)
                })
                .OrderByDescending(x => x.OrderCount)
                .ThenByDescending(x => x.VisitCount)
                .Take(5)
                .Select((p, i) =>
                {
                    p.Rank = i + 1;
                    return p;
                })
                .ToList();

            var chart = BuildMonthlyRevenue(orderList, filter.FromDate, filter.ToDate);
            var hasDateFilter = filter.FromDate.HasValue || filter.ToDate.HasValue;

            return new ReportViewModel
            {
                Filter = filter,
                ConversionRate = totalVisits == 0 ? 0 : Math.Round((decimal)converted / totalVisits * 100, 1),
                CollectedAmount = collected,
                VisitCompletionRate = totalVisits == 0 ? 0 : Math.Round((decimal)completedish / totalVisits * 100, 1),
                NewClientsThisMonth = hasDateFilter
                    ? clientList.Count
                    : clientList.Count(c => c.CreatedAt >= monthStart),
                TotalVisits = totalVisits,
                TotalOrders = orderList.Count,
                ChartLabels = chart.Labels,
                ChartValues = chart.Values,
                TopPerformers = performers,
                SalesPeople = salesPeople.Select(s => (Id: s.Id, Name: s.FullName)).ToList()
            };
        }

        private static (List<string> Labels, List<decimal> Values) BuildMonthlyRevenue(
            List<Models.OrderMaster> orders,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var labels = new List<string>();
            var values = new List<decimal>();

            DateTime cursor;
            int months;

            if (fromDate.HasValue && toDate.HasValue)
            {
                cursor = new DateTime(fromDate.Value.Year, fromDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endMonth = new DateTime(toDate.Value.Year, toDate.Value.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                months = Math.Max(1, ((endMonth.Year - cursor.Year) * 12) + endMonth.Month - cursor.Month + 1);
                months = Math.Min(months, 12);
            }
            else
            {
                cursor = DateTime.UtcNow.Date.AddMonths(-6);
                cursor = new DateTime(cursor.Year, cursor.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                months = 7;
            }

            for (var i = 0; i < months; i++)
            {
                var start = cursor.AddMonths(i);
                var end = start.AddMonths(1);
                labels.Add(start.ToString("MMM yyyy"));
                values.Add(orders
                    .Where(o => o.OrderDate >= start && o.OrderDate < end && o.PaymentStatus != PaymentStatus.Cancelled)
                    .Sum(o => o.Amount));
            }

            return (labels, values);
        }

        private static string ToTimeAgo(DateTime utc)
        {
            var span = DateTime.UtcNow - utc;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            return $"{(int)span.TotalDays}d ago";
        }
    }
}
