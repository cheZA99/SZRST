using Infrastructure.Persistance;
using System;
using System.Threading.Tasks;
using SZRST.Domain.Entities;
using SZRST.Web.Serivces;

namespace SZRST.Web.Schedule
{
    public class ReportService
    {
        private readonly IAppointmentReportService _appointmentsReportService;

        public ReportService(IAppointmentReportService appointmentsReportService)
        {
            _appointmentsReportService = appointmentsReportService;
        }

        public async Task GenerateMonthlyReports()
        {
            Console.WriteLine("Pokrenuo se scheduler");
            await _appointmentsReportService.GenerateMonthlyReports();
        }
    }
}
