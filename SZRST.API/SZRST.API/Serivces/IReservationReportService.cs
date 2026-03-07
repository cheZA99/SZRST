using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SZRST.Domain.Entities;
using SZRST.Web.Controllers;

namespace SZRST.Web.Serivces
{
    public interface IReservationReportService
    {
        Task<int> GenerateReport(DateTime dateFrom, DateTime dateTo, int tenantId);

        Task<List<ReservationReport>> GetReports();

        Task<List<ReservationReport>> GetReportsByTenantId(int tenantId);

        Task<ReservationReport> GetReport(int id);

        Task GenerateMonthlyReports();
    }
}
