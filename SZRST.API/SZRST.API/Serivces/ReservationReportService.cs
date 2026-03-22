using Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SZRST.Domain.Entities;
using System.Linq;
using QuestPDF.Fluent;
using SZRST.Web.Controllers;

namespace SZRST.Web.Serivces
{
    public class ReservationReportService : IReservationReportService
    {
        private readonly SZRSTContext _context;

        public ReservationReportService(SZRSTContext context)
        {
            _context = context;
        }

        public async Task<int> GenerateReport(DateTime dateFrom, DateTime dateTo, int tenantId)
        {
            var reportData = await _context.Appointment
                .IgnoreQueryFilters()
                .Include(a => a.AppointmentType)
                .Include(a => a.Facility)
                .Where(a =>
                    a.AppointmentDateTime >= dateFrom &&
                    a.AppointmentDateTime <= dateTo &&
                    !a.IsDeleted &&
                    !a.IsFree &&
                    a.TenantId == tenantId)
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

            var pdfBytes = GeneratePdf(reportData, dateFrom, dateTo, totalReservations, totalProfit);

            var report = new ReservationReport
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                FileName = $"reservation_report_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf",
                PdfData = pdfBytes,
                CreatedAt = DateTime.UtcNow,
                TenantId = tenantId
            };

            _context.ReservationReport.Add(report);
            await _context.SaveChangesAsync();

            return report.Id;
        }

        public async Task<List<ReservationReportListDto>> GetReports()
        {
            return await _context.ReservationReport
                .IgnoreQueryFilters()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReservationReportListDto
                {
                    Id = x.Id,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    CreatedAt = x.CreatedAt,
                    FileName = x.FileName,
                    TenantId = x.TenantId,
                    FileSizeBytes = x.PdfData != null ? x.PdfData.Length : 0
                })
                .ToListAsync();
        }

        public async Task<List<ReservationReportListDto>> GetReportsByTenantId(int tenantId)
        {
            return await _context.ReservationReport
                .IgnoreQueryFilters()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReservationReportListDto
                {
                    Id = x.Id,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    CreatedAt = x.CreatedAt,
                    FileName = x.FileName,
                    TenantId = x.TenantId,
                    FileSizeBytes = x.PdfData != null ? x.PdfData.Length : 0
                })
                .ToListAsync();
        }

        public async Task<ReservationReport> GetReport(int id)
        {
            return await _context.ReservationReport
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task GenerateMonthlyReports()
        {
             var now = DateTime.UtcNow;

             var firstDayThisMonth = new DateTime(now.Year, now.Month, 1);
             var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
             var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

            //var tenants = await _context.Tenant.ToListAsync();

            var tenantIds = await _context.Database
                .SqlQuery<int>($"SELECT Id FROM Tenant")
                .ToListAsync();

            foreach (var tenantId in tenantIds)
             {
                 await GenerateReport(firstDayLastMonth, lastDayLastMonth, tenantId);
             }
        }

        public byte[] GeneratePdf(List<MonthlyReservationReport> data, DateTime from, DateTime to, int totalReservations, decimal totalProfit)
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
                                table.Cell().Text($"{row.Profit:0.00} KM");
                            }
                        });

                        col.Item().PaddingTop(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Total reservations: {totalReservations}").Bold();

                            row.RelativeItem().AlignRight()
                                .Text($"Total profit: {totalProfit:0.00} KM")
                                .Bold();
                        });
                    });
                });
            });

            return pdf.GeneratePdf();
        }
    }
}
