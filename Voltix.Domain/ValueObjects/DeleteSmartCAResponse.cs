using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class DeleteSmartCAResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // "2021-01-06T17:06:07.2893325" (không timezone) → DateTime
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("comId")]
        public int ComId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        // "2021-12-07T09:15:00" (không timezone) → DateTime
        [JsonPropertyName("validFrom")]
        public DateTime? ValidFrom { get; set; }

        [JsonPropertyName("validTo")]
        public DateTime? ValidTo { get; set; }

        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("signConfirmationEnabled")]
        public bool SignConfirmationEnabled { get; set; }

        [JsonPropertyName("isAccountLocked")]
        public bool IsAccountLocked { get; set; }

        // "c59125a9-5fee-44e2-b0ff-b229634bf0b2"
        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }

        [JsonPropertyName("signatureText")]
        public string? SignatureText { get; set; }

        [JsonPropertyName("roles")]
        public List<SmartCaRole>? Roles { get; set; }

        [JsonPropertyName("signMethod")]
        public ValueDesc? SignMethod { get; set; }

        [JsonPropertyName("status")]
        public ValueDesc? Status { get; set; }

        [JsonPropertyName("receiveOtpMethod")]
        public ValueDesc? ReceiveOtpMethod { get; set; }

        [JsonPropertyName("receiveNotificationMethod")]
        public ValueDesc? ReceiveNotificationMethod { get; set; }

        [JsonPropertyName("personalCertificateId")]
        public int? PersonalCertificateId { get; set; }

        [JsonPropertyName("smartCaId")]
        public int? SmartCaId { get; set; }

        // Không có schema cụ thể → để object (linh hoạt)
        [JsonPropertyName("extraValues")]
        public List<object>? ExtraValues { get; set; }

        [JsonPropertyName("usedMemory")]
        public long UsedMemory { get; set; }

        [JsonPropertyName("defaultSmartCa")]
        public SmartCaCertificate? DefaultSmartCa { get; set; }

        [JsonPropertyName("userCertificates")]
        public List<SmartCaCertificate>? UserCertificates { get; set; }

        [JsonPropertyName("smartCaGroups")]
        public List<object>? SmartCaGroups { get; set; }
    }

    public sealed class SmartCaRole
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public sealed class SmartCaCertificate
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("commonName")]
        public string? CommonName { get; set; }

        [JsonPropertyName("uid")]
        public string? Uid { get; set; }

        // Trong certificate: "2025-03-07T08:56:16Z" → có timezone → DateTimeOffset
        [JsonPropertyName("validFrom")]
        public DateTimeOffset? ValidFrom { get; set; }

        [JsonPropertyName("validTo")]
        public DateTimeOffset? ValidTo { get; set; }

        [JsonPropertyName("serialNumber")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("subjectDN")]
        public string? SubjectDn { get; set; }

        [JsonPropertyName("status")]
        public ValueDesc? Status { get; set; }

        [JsonPropertyName("smartCaServiceName")]
        public string? SmartCaServiceName { get; set; }

        [JsonPropertyName("smartCaSignMethod")]
        public ValueDesc? SmartCaSignMethod { get; set; }

        [JsonPropertyName("smartCaAccountType")]
        public ValueDesc? SmartCaAccountType { get; set; }

        [JsonPropertyName("smartCaUsername")]
        public string? SmartCaUsername { get; set; }

        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("provider")]
        public ValueDesc? Provider { get; set; }

        // "2025-03-17T08:56:42.4886305+07:00" → DateTimeOffset
        [JsonPropertyName("createdDate")]
        public DateTimeOffset? CreatedDate { get; set; }

        [JsonPropertyName("isBusinessSmartCaAccount")]
        public bool IsBusinessSmartCaAccount { get; set; }

        [JsonPropertyName("ownerId")]
        public int? OwnerId { get; set; }

        [JsonPropertyName("createdByUserId")]
        public int? CreatedByUserId { get; set; }
    }

    public sealed class ValueDesc
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
