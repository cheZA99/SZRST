using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZRST.Domain.Entities
{
    public class ReservationReport : BaseEntity<int>, ITenantEntity
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public string FileName { get; set; }
        public byte[] PdfData { get; set; }

        public DateTime CreatedAt { get; set; }

        public int TenantId { get; set; }
    }
}
