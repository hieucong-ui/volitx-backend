using Voltix.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Auth
{
    public class VnptDocumentDto
    {
        public string Id { get; set; } = null!;
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime? DocumentDate { get; set; }
        public decimal? ContractValue { get; set; }
        public string? CustomerCode { get; set; }
        public string? CustomerInformation { get; set; }
        public string? No { get; set; }
        public string? Subject { get; set; }
        public string? DownloadUrl { get; set; }
        public VnptStatusDto? Status { get; set; }
        public VnptProcessItem? WaitingProcess { get; set; }
        public List<VnptProcess>? Processes { get; set; }
        public string? PositionA { get; set; }
        public string? PositionB { get; set; }
        public int PageSign { get; set; }
        public byte[]? PdfBytes { get; set; }
        public string? FileName { get; set; }
    }
}
