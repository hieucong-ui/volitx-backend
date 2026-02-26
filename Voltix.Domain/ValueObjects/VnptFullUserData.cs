using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class VnptFullUserData
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("createdDate")] public DateTime CreatedDate { get; set; }
        [JsonPropertyName("comId")] public int ComId { get; set; }
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("username")] public string Username { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("phone")] public string Phone { get; set; } = "";
        [JsonPropertyName("email")] public string Email { get; set; } = "";
        [JsonPropertyName("isValid")] public bool IsValid { get; set; }
        [JsonPropertyName("signConfirmationEnabled")] public bool SignConfirmationEnabled { get; set; }
        [JsonPropertyName("isAccountLocked")] public bool IsAccountLocked { get; set; }
        [JsonPropertyName("accountId")] public Guid AccountId { get; set; }
        [JsonPropertyName("signatureText")] public string SignatureText { get; set; } = "";

        [JsonPropertyName("roles")] public List<RoleDto> Roles { get; set; } = new();

        [JsonPropertyName("signMethod")] public NamedValueDto? SignMethod { get; set; }
        [JsonPropertyName("status")] public NamedValueDto? Status { get; set; }
        [JsonPropertyName("receiveOtpMethod")] public NamedValueDto? ReceiveOtpMethod { get; set; }
        [JsonPropertyName("receiveNotificationMethod")] public NamedValueDto? ReceiveNotificationMethod { get; set; }

        [JsonPropertyName("personalCertificateId")] public int? PersonalCertificateId { get; set; }

        [JsonPropertyName("extraValues")] public List<JsonElement>? ExtraValues { get; set; }

        [JsonPropertyName("usedMemory")] public long UsedMemory { get; set; }

        [JsonPropertyName("userCertificates")] public List<UserCertificateDto> UserCertificates { get; set; } = new();

        [JsonPropertyName("smartCaGroups")] public List<JsonElement>? SmartCaGroups { get; set; }

        [JsonPropertyName("waitingChangeRequest")] public bool WaitingChangeRequest { get; set; }
        [JsonPropertyName("twoFactorEnabled")] public bool TwoFactorEnabled { get; set; }
    }

    public sealed class RoleDto
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("description")] public string? Description { get; set; }
    }

    public sealed class NamedValueDto
    {
        [JsonPropertyName("value")] public int Value { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; } = "";
    }

    public sealed class UserCertificateDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }

        [JsonPropertyName("name")] public string? Name { get; set; }

        [JsonPropertyName("commonName")] public string CommonName { get; set; } = "";
        [JsonPropertyName("uid")] public string Uid { get; set; } = "";
        [JsonPropertyName("validFrom")] public DateTime ValidFrom { get; set; }
        [JsonPropertyName("validTo")] public DateTime ValidTo { get; set; }
        [JsonPropertyName("serialNumber")] public string SerialNumber { get; set; } = "";
        [JsonPropertyName("subjectDN")] public string SubjectDN { get; set; } = "";

        [JsonPropertyName("status")] public NamedValueDto? Status { get; set; }

        [JsonPropertyName("smartCaServiceName")] public string? SmartCaServiceName { get; set; }
        [JsonPropertyName("smartCaSignMethod")] public NamedValueDto? SmartCaSignMethod { get; set; }
        [JsonPropertyName("smartCaAccountType")] public NamedValueDto? SmartCaAccountType { get; set; }
        [JsonPropertyName("smartCaUsername")] public string? SmartCaUsername { get; set; }

        [JsonPropertyName("isValid")] public bool IsValid { get; set; }
        [JsonPropertyName("provider")] public NamedValueDto? Provider { get; set; }
        [JsonPropertyName("createdDate")] public DateTime CreatedDate { get; set; }
        [JsonPropertyName("isBusinessSmartCaAccount")] public bool IsBusinessSmartCaAccount { get; set; }
        [JsonPropertyName("ownerId")] public int OwnerId { get; set; }
        [JsonPropertyName("isAutoSignConfig")] public bool IsAutoSignConfig { get; set; }
        [JsonPropertyName("createdByUserId")] public int? CreatedByUserId { get; set; }
        [JsonPropertyName("isDefaultSmartCaCert")] public bool IsDefaultSmartCaCert { get; set; }

        [JsonPropertyName("issuerDN")] public string? IssuerDN { get; set; }

        [JsonPropertyName("signatureDisplayMode")] public int? SignatureDisplayMode { get; set; }
    }
}
