using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SZRST.Domain.Entities;
using SZRST.Web.Controllers;

namespace SZRST.Web.Serivces
{
    public interface IAppointmentReportService
    {
        Task<int> GenerateReport(DateTime dateFrom, DateTime dateTo, int tenantId);

        Task<List<AppointmentReportListDto>> GetReports();

        Task<List<AppointmentReportListDto>> GetReportsByTenantId(int tenantId);

        Task<ReservationReport> GetReport(int id);

        Task GenerateMonthlyReports();
    }

    public class AppointmentReportListDto
    {
        public int Id { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FileName { get; set; }
        public int TenantId { get; set; }
        public int FileSizeBytes { get; set; }
    }
}
