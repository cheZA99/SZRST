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

        Task<List<ReservationReportListDto>> GetReports();

        Task<List<ReservationReportListDto>> GetReportsByTenantId(int tenantId);

        Task<ReservationReport> GetReport(int id);

        Task GenerateMonthlyReports();
    }

    public class ReservationReportListDto
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
