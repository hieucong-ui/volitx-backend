using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Entities
{
    public class EContract
    {
        public Guid Id { get; private set; }
        public string HtmlTemaple { get; private set; }
        public string? Name { get; set; }
        public EContractStatus Status { get; private set; } = EContractStatus.Draft;
        public EcontractType Type { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public string CreatedBy { get; private set; } = null!;
        public string OwnerBy { get; private set; } = null!;
        public Guid? CustomerOrderId { get; private set; }

        public ApplicationUser Owner { get; private set; } = null!;
        public BookingEV? BookingEV { get; private set; }
        public CustomerOrder? CustomerOrder { get; private set; }

        private EContract() { }
        public EContract(Guid id, string htmlTemaple, string? name, string createdBy, string ownerBy, EContractStatus status, EcontractType type)
        {
            Id = id;
            HtmlTemaple = htmlTemaple;
            Name = name;
            Status = status;
            CreatedBy = createdBy;
            OwnerBy = ownerBy;
            Type = type;
        }

        public EContract(Guid id, string htmlTemaple, string? name, string createdBy, string ownerBy, Guid customerOrderId, EContractStatus status, EcontractType type)
        {
            Id = id;
            HtmlTemaple = htmlTemaple;
            Name = name;
            Status = status;
            CreatedBy = createdBy;
            OwnerBy = ownerBy;
            Type = type;
            CustomerOrderId = customerOrderId;
        }

        public void UpdateHtmlTemplate(string htmlTemplate, string? name)
        {
            HtmlTemaple = htmlTemplate;
            Name = name;
        }

        public void UpdateStatus(EContractStatus status)
        {
            Status = status;
        }
    }
}
