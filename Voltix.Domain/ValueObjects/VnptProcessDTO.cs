using Newtonsoft.Json;
using Voltix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class VnptProcessDTO : IDisposable
    {
        public string? ProcessId { get; set; }
        public string? Reason { get; set; }
        public bool? Reject { get; set; }
        public string? Otp { get; set; }
        public int SignatureDisplayMode { get; set; }
        public string? SignatureImage { get; set; }
        public int? SigningPage { get; set; }
        public string? SigningPosition { get; set; }
        public string? SignatureText { get; set; }
        public int? FontSize { get; set; }
        public bool? ShowReason { get; set; }
        public bool? ConfirmTermsConditions { get; set; }

        private bool _disposed = false;

        public VnptProcessDTO()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dộn dẹp tài nguyên quản lý
                ProcessId = null;
                Reason = null;
                Otp = null;
                SignatureImage = null;
                SigningPosition = null;
                SignatureText = null;

                Console.WriteLine("Resources released successfully.");
            }

            _disposed = true;
        }

        ~VnptProcessDTO()
        {
            Dispose(false);
        }
    }

    public class ProcessRespone
    {
        [JsonProperty("id")] public Guid Id { get; set; }

        [JsonProperty("createdDate")] public DateTime CreatedDate { get; set; }

        [JsonProperty("lastModifiedDate")] public DateTime LastModifiedDate { get; set; }

        [JsonProperty("completedDate")] public DateTime? CompletedDate { get; set; }

        [JsonProperty("no")] public string? No { get; set; }

        [JsonProperty("subject")] public string? Subject { get; set; }

        [JsonProperty("status")] public StatusInfo? Status { get; set; }

        [JsonProperty("description")] public string? Description { get; set; }

        [JsonProperty("waitingProcess")] public WaitingProcessDto? WaitingProcess { get; set; }

        [JsonProperty("processInOrder")] public bool ProcessInOrder { get; set; }

        [JsonProperty("type")] public DocumentType? Type { get; set; }

        [JsonProperty("file")] public FileInfo? File { get; set; }

        [JsonProperty("downloadUrl")] public string? DownloadUrl { get; set; }

        [JsonProperty("receiveOtpMethod")] public int? ReceiveOtpMethod { get; set; }

        [JsonProperty("receiveOtpPhone")] public string? ReceiveOtpPhone { get; set; }
        [JsonProperty("receiveOtpEmail")] public string? ReceiveOtpEmail { get; set; }

        [JsonProperty("requireOtpConfirmation")]
        public bool? RequireOtpConfirmation { get; set; }

        public class StatusInfo
        {
            [JsonProperty("value")] public int Value { get; set; }

            [JsonProperty("description")] public string? Description { get; set; }
        }

        public class WaitingProcessDto
        {
            [JsonProperty("id")] public Guid Id { get; set; }

            [JsonProperty("createdDate")] public DateTime CreatedDate { get; set; }

            [JsonProperty("isOrder")] public bool IsOrder { get; set; }

            [JsonProperty("orderNo")] public int OrderNo { get; set; }

            [JsonProperty("pageSign")] public int PageSign { get; set; }

            [JsonProperty("position")] public string? Position { get; set; }

            [JsonProperty("accessPermission")] public StatusInfo? AccessPermission { get; set; }

            [JsonProperty("status")] public StatusInfo? Status { get; set; }
        }

        public class DocumentType
        {
            [JsonProperty("id")] public int Id { get; set; }

            [JsonProperty("code")] public string? Code { get; set; }

            [JsonProperty("name")] public string? Name { get; set; }
        }

        public class FileInfo
        {
            [JsonProperty("name")] public string? Name { get; set; }

            [JsonProperty("size")] public long Size { get; set; }
        }
    }

}
