using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class UpdateEContractResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonPropertyName("lastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        [JsonPropertyName("no")]
        public string? No { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("hasVerified")]
        public bool HasVerified { get; set; }

        [JsonPropertyName("fileType")]
        public int FileType { get; set; }

        [JsonPropertyName("status")]
        public ValueDescription Status { get; set; } = default!;

        [JsonPropertyName("contractStatus")]
        public ValueDescription ContractStatus { get; set; } = default!;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("createdByUserId")]
        public long CreatedByUserId { get; set; }

        [JsonPropertyName("processRecipientCount")]
        public int ProcessRecipientCount { get; set; }

        [JsonPropertyName("processInOrder")]
        public bool ProcessInOrder { get; set; }

        [JsonPropertyName("isWaitToSignDraw")]
        public bool IsWaitToSignDraw { get; set; }

        [JsonPropertyName("isWaitToSignDigital")]
        public bool IsWaitToSignDigital { get; set; }

        [JsonPropertyName("isWaitToApprove")]
        public bool IsWaitToApprove { get; set; }

        [JsonPropertyName("isCancelable")]
        public bool IsCancelable { get; set; }

        [JsonPropertyName("isEditable")]
        public bool IsEditable { get; set; }

        [JsonPropertyName("isShareable")]
        public bool IsShareable { get; set; }

        [JsonPropertyName("isAccessable")]
        public bool IsAccessable { get; set; }

        [JsonPropertyName("isExpired")]
        public bool IsExpired { get; set; }

        [JsonPropertyName("canDownload")]
        public bool CanDownload { get; set; }

        [JsonPropertyName("type")]
        public DocumentTypeDto Type { get; set; } = default!;

        [JsonPropertyName("processes")]
        public List<JsonElement> Processes { get; set; } = new();

        [JsonPropertyName("histories")]
        public List<HistoryDto> Histories { get; set; } = new();

        [JsonPropertyName("attachments")]
        public List<JsonElement> Attachments { get; set; } = new();

        [JsonPropertyName("relatedDocuments")]
        public List<JsonElement> RelatedDocuments { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<JsonElement> Messages { get; set; } = new();

        [JsonPropertyName("file")]
        public FileDto File { get; set; } = default!;

        [JsonPropertyName("downloadUrl")]
        public string? DownloadUrl { get; set; }
        public byte[]? FileBytes { get; set; }
        public string? PositionA { get; set; }
        public string? PositionB { get; set; }
        public int? PageSign { get; set; }
    }

    public class ValueDescription
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class HistoryDto
    {
        [JsonPropertyName("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonPropertyName("requestType")]
        public ValueDescription RequestType { get; set; } = default!;

        [JsonPropertyName("ipAddress")]
        public string? IpAddress { get; set; }

        [JsonPropertyName("activity")]
        public ValueDescription Activity { get; set; } = default!;
    }

    public class FileDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}
