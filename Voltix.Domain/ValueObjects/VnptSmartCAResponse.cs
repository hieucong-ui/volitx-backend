using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class VnptSmartCAResponse
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("createdDate")] public DateTime CreatedDate { get; set; }

        [JsonProperty("comId")] public int ComId { get; set; }

        [JsonProperty("code")] public string Code { get; set; } = string.Empty;

        [JsonProperty("username")] public string Username { get; set; } = string.Empty;

        [JsonProperty("name")] public string Name { get; set; } = string.Empty;

        [JsonProperty("phone")] public string Phone { get; set; } = string.Empty;

        [JsonProperty("email")] public string Email { get; set; } = string.Empty;

        [JsonProperty("validFrom")] public DateTime? ValidFrom { get; set; }

        [JsonProperty("validTo")] public DateTime? ValidTo { get; set; }

        [JsonProperty("isValid")] public bool IsValid { get; set; }

        [JsonProperty("signConfirmationEnabled")] public bool SignConfirmationEnabled { get; set; }

        [JsonProperty("isAccountLocked")] public bool IsAccountLocked { get; set; }

        [JsonProperty("accountId")] public Guid AccountId { get; set; }

        [JsonProperty("signatureText")] public string? SignatureText { get; set; }

        [JsonProperty("roles")] public List<VnptRoleItem> Roles { get; set; } = new();

        [JsonProperty("signMethod")] public VnptEnumDto? SignMethod { get; set; }

        [JsonProperty("status")] public VnptEnumDto? Status { get; set; }

        [JsonProperty("receiveOtpMethod")] public VnptEnumDto? ReceiveOtpMethod { get; set; }

        [JsonProperty("receiveNotificationMethod")] public VnptEnumDto? ReceiveNotificationMethod { get; set; }

        [JsonProperty("personalCertificateId")] public int? PersonalCertificateId { get; set; }

        [JsonProperty("smartCaId")] public int? SmartCaId { get; set; }

        [JsonProperty("extraValues")] public List<object> ExtraValues { get; set; } = new();

        [JsonProperty("usedMemory")] public long UsedMemory { get; set; }

        [JsonProperty("defaultSmartCa")] public VnptSmartCaCertificate? DefaultSmartCa { get; set; }

        [JsonProperty("userCertificates")] public List<VnptSmartCaCertificate> UserCertificates { get; set; } = new();

        [JsonProperty("smartCaGroups")] public List<object> SmartCaGroups { get; set; } = new();
    }

    public sealed class VnptRoleItem
    {
        [JsonProperty("id")] public Guid Id { get; set; }

        [JsonProperty("code")] public string Code { get; set; } = string.Empty;

        [JsonProperty("name")] public string Name { get; set; } = string.Empty;

        [JsonProperty("description")] public string? Description { get; set; }
    }

    public sealed class VnptEnumDto
    {
        [JsonProperty("value")] public int Value { get; set; }

        [JsonProperty("description")] public string? Description { get; set; }
    }

    public sealed class VnptSmartCaCertificate
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; } = string.Empty;

        [JsonProperty("commonName")] public string? CommonName { get; set; }

        [JsonProperty("uid")] public string? Uid { get; set; }

        [JsonProperty("validFrom")] public DateTime? ValidFrom { get; set; }

        [JsonProperty("validTo")] public DateTime? ValidTo { get; set; }

        [JsonProperty("serialNumber")] public string? SerialNumber { get; set; }

        [JsonProperty("subjectDN")] public string? SubjectDn { get; set; }

        [JsonProperty("status")] public VnptEnumDto? Status { get; set; }

        [JsonProperty("smartCaServiceName")] public string? SmartCaServiceName { get; set; }

        [JsonProperty("smartCaSignMethod")] public VnptEnumDto? SmartCaSignMethod { get; set; }

        [JsonProperty("smartCaAccountType")] public VnptEnumDto? SmartCaAccountType { get; set; }

        [JsonProperty("smartCaUsername")] public string? SmartCaUsername { get; set; }

        [JsonProperty("isValid")] public bool IsValid { get; set; }

        [JsonProperty("provider")] public VnptEnumDto? Provider { get; set; }

        [JsonProperty("createdDate")] public DateTime? CreatedDate { get; set; }

        [JsonProperty("isBusinessSmartCaAccount")] public bool? IsBusinessSmartCaAccount { get; set; }

        [JsonProperty("ownerId")] public int? OwnerId { get; set; }

        [JsonProperty("createdByUserId")] public int? CreatedByUserId { get; set; }
    }
}
