using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;
using SZRST.Web.Serivces;

namespace SZRST.Web.Controllers
{
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationReportController : ControllerBase
    {
        private readonly IReservationReportService _reportService;



        public ReservationReportController(IReservationReportService reportService)
        {
            _reportService = reportService;
        }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] ReservationReportRequest request)
        {
            var reportId = await _reportService.GenerateReport(
                request.DateFrom,
                request.DateTo,
                request.TenantId
            );

            return Ok(new { reportId });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _reportService.GetReport(id);

            if (report == null)
                return NotFound();

            return File(report.PdfData, "application/pdf", report.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _reportService.GetReports();

            return Ok(reports);
        }

        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetReportsByTenantId(int tenantId)
        {
            var reports = await _reportService.GetReportsByTenantId(tenantId);

            return Ok(reports);
        }
    }

    public class ReservationReportRequest
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int TenantId { get; set; }
    }

    public class MonthlyReservationReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalReservations { get; set; }
        public float Profit { get; set; }

        public string FacilityName { get; set; }
    }
}
