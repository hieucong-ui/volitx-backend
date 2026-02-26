using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class GetEContractResponse<TItem>
    {
        [JsonProperty("items")]
        public List<TItem> Items { get; set; } = new();

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("pageCount")]
        public int PageCount { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }
    }

    public class DocumentListItemDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("lastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("no")]
        public string? No { get; set; }

        [JsonProperty("subject")]
        public string? Subject { get; set; }

        [JsonProperty("hasVerified")]
        public bool HasVerified { get; set; }

        [JsonProperty("fileType")]
        public int FileType { get; set; }

        [JsonProperty("status")]
        public VnptValueDescription Status { get; set; } = new();

        [JsonProperty("contractStatus")]
        public VnptValueDescription ContractStatus { get; set; } = new();

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("department")]
        public DepartmentDto? Department { get; set; }

        [JsonProperty("createdByUserId")]
        public int CreatedByUserId { get; set; }

        [JsonProperty("processRecipientCount")]
        public int ProcessRecipientCount { get; set; }

        [JsonProperty("waitingProcess")]
        public ProcessDto? WaitingProcess { get; set; }

        [JsonProperty("processInOrder")]
        public bool ProcessInOrder { get; set; }

        [JsonProperty("isWaitToSignDraw")]
        public bool IsWaitToSignDraw { get; set; }

        [JsonProperty("isWaitToSignDigital")]
        public bool IsWaitToSignDigital { get; set; }

        [JsonProperty("isWaitToApprove")]
        public bool IsWaitToApprove { get; set; }

        [JsonProperty("isCancelable")]
        public bool IsCancelable { get; set; }

        [JsonProperty("isEditable")]
        public bool IsEditable { get; set; }

        [JsonProperty("isShareable")]
        public bool IsShareable { get; set; }

        [JsonProperty("isAccessable")]
        public bool IsAccessable { get; set; }

        [JsonProperty("isExpired")]
        public bool IsExpired { get; set; }

        [JsonProperty("canDownload")]
        public bool CanDownload { get; set; }

        [JsonProperty("type")]
        public DocumentTypeDto? Type { get; set; }

        [JsonProperty("documentTemplate")]
        public DocumentTemplateDto? DocumentTemplate { get; set; }

        [JsonProperty("batchImport")]
        public BatchImportDto? BatchImport { get; set; }

        [JsonProperty("processes")]
        public List<ProcessDto> Processes { get; set; } = new();

        [JsonProperty("histories")]
        public JToken? Histories { get; set; }

        [JsonProperty("attachments")]
        public JToken? Attachments { get; set; }

        [JsonProperty("relatedDocuments")]
        public JToken? RelatedDocuments { get; set; }

        [JsonProperty("messages")]
        public JToken? Messages { get; set; }

        [JsonProperty("file")]
        public FileLightDto? File { get; set; }

        [JsonProperty("downloadUrl")]
        public string? DownloadUrl { get; set; }
    }

    public class VnptValueDescription
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
    }

    public class DepartmentDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }
    }

    public class DocumentTypeDto
    {   
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("documentCount")]
        public int DocumentCount { get; set; }
    }

    public class DocumentTemplateDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("fileName")]
        public string? FileName { get; set; }

        [JsonProperty("downloadUrl")]
        public string? DownloadUrl { get; set; }

        [JsonProperty("pdfDownloadUrl")]
        public string? PdfDownloadUrl { get; set; }

        [JsonProperty("isShareable")]
        public bool IsShareable { get; set; }

        // Lưu ý: theo mẫu là "isDeletelable" (chính tả như vậy)
        [JsonProperty("isDeletelable")]
        public bool IsDeletelable { get; set; }

        [JsonProperty("isUpdateable")]
        public bool IsUpdateable { get; set; }
    }

    public class BatchImportDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("uploadedByUserId")]
        public int UploadedByUserId { get; set; }

        [JsonProperty("numberOfRecords")]
        public int NumberOfRecords { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public class ProcessDto
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("comId")]
        public int ComId { get; set; }

        [JsonProperty("isOrder")]
        public bool IsOrder { get; set; }

        [JsonProperty("orderNo")]
        public int OrderNo { get; set; }

        [JsonProperty("pageSign")]
        public int PageSign { get; set; }

        [JsonProperty("position")]
        public string? Position { get; set; }

        [JsonProperty("displayType")]
        public VnptValueDescription DisplayType { get; set; } = new();

        [JsonProperty("accessPermission")]
        public VnptValueDescription AccessPermission { get; set; } = new();

        [JsonProperty("status")]
        public VnptValueDescription Status { get; set; } = new();

        [JsonProperty("processedByUserId")]
        public int ProcessedByUserId { get; set; }

        [JsonProperty("documentId")]
        public string DocumentId { get; set; } = default!;
    }

    public class FileLightDto
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}
