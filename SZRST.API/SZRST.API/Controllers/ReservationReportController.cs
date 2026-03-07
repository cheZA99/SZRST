using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SZRST.Web.Controllers
{
    [Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationReportController : ControllerBase
    {
        private readonly SZRSTContext _context;

        public ReservationReportController(SZRSTContext context)
        {
            _context = context;
        }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] ReservationReportRequest request)
        {
            /*var reportData = await _context.Reservation
                .Include(r => r.Appointment)
                .Where(r =>
                    r.ReservationDateTime >= request.DateFrom &&
                    r.ReservationDateTime <= request.DateTo)
                .GroupBy(r => new
                {
                    r.ReservationDateTime.Year,
                    r.ReservationDateTime.Month
                })
                .Select(g => new MonthlyReservationReport
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalReservations = g.Count(),
                    Profit = g.Sum(x => x.Appointment.AppointmentType.Price)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();*/

            var reportData = await _context.Appointment
                .Include(a => a.AppointmentType)
                .Include(a => a.Facility)
                .Include(a => a.Tenant)
                .Where(a =>
                    a.AppointmentDateTime >= request.DateFrom &&
                    a.AppointmentDateTime <= request.DateTo &&
                    a.TenantId == request.TenantId)
                .GroupBy(a => new
                {
                    a.AppointmentDateTime.Year,
                    a.AppointmentDateTime.Month,
                    FacilityName = a.Facility.Name
                })
                .Select(g => new MonthlyReservationReport
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    FacilityName = g.Key.FacilityName,
                    TotalReservations = g.Count(),
                    Profit = g.Sum(x => x.AppointmentType.Price)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.FacilityName)
                .ToListAsync();

            var totalReservations = reportData.Sum(x => x.TotalReservations);
            var totalProfit = reportData.Sum(x => x.Profit);

            var pdfBytes = GeneratePdf(reportData, request.DateFrom, request.DateTo, totalReservations, totalProfit);

            var report = new ReservationReport
            {
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                FileName = $"reservation_report_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PdfData = pdfBytes,
                CreatedAt = DateTime.UtcNow,
                TenantId = request.TenantId
            };

            _context.ReservationReport.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { report.Id });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _context.ReservationReport.FindAsync(id);

            if (report == null)
                return NotFound();

            return File(report.PdfData, "application/pdf", report.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _context.ReservationReport
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.DateFrom,
                    x.DateTo,
                    x.CreatedAt,
                    x.FileName,
                    x.TenantId
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetReportsByTenantId(int tenantId)
        {
            var reports = await _context.ReservationReport
                .Where(rr => rr.TenantId == tenantId && !rr.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.DateFrom,
                    x.DateTo,
                    x.CreatedAt,
                    x.FileName,
                    x.TenantId
                })
                .ToListAsync();

            return Ok(reports);
        }

        private byte[] GeneratePdf(List<MonthlyReservationReport> data, DateTime from, DateTime to, int totalReservations,
                float totalProfit)
        {
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text($"Reservation Report ({from:dd.MM.yyyy} - {to:dd.MM.yyyy})")
                        .FontSize(20)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Month");
                                header.Cell().Text("Facility");
                                header.Cell().Text("Reservations");
                                header.Cell().Text("Profit");
                            });

                            foreach (var row in data)
                            {
                                table.Cell().Text($"{row.Month}/{row.Year}");
                                table.Cell().Text(row.FacilityName);
                                table.Cell().Text(row.TotalReservations.ToString());
                                table.Cell().Text($"{row.Profit} KM");
                            }
                        });

                        col.Item().PaddingTop(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Total reservations: {totalReservations}")
                                .Bold();

                            row.RelativeItem().AlignRight()
                                .Text($"Total profit: {totalProfit} KM")
                                .Bold();
                        });
                    });

                });
            });

            return pdf.GeneratePdf();
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
