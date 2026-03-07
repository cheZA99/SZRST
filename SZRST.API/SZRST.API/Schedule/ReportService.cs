using Infrastructure.Persistance;
using System;
using System.Threading.Tasks;
using SZRST.Domain.Entities;
using SZRST.Web.Serivces;

namespace SZRST.Web.Schedule
{
    public class ReportService
    {
        private readonly IReservationReportService _reservationsReportService;

        public ReportService(IReservationReportService reservationsReportService)
        {
            _reservationsReportService = reservationsReportService;
        }

        public async Task GenerateMonthlyReports()
        {
            Console.WriteLine("Pokrenuo se scheduler");
            await _reservationsReportService.GenerateMonthlyReports();
        }
    }
}
