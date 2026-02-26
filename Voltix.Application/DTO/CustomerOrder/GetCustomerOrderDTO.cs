using Voltix.Application.DTO.Customer;
using Voltix.Application.DTO.EContract;
using Voltix.Application.DTO.OrderDetail;
using Voltix.Application.DTO.QuoteDetail;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.CustomerOrder
{
    public record GetCustomerOrderDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid QuoteId { get; set; }
        public int OrderNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public OrderStatus Status { get; set; }
        public List<GetQuoteDetailDTO> QuoteDetails { get; set; } = new();
        public List<GetOrderDetailDTO> OrderDetails {  get; set; } = new();
        public GetCustomerDTO Customer { get; set; } = new();
        public List<GetEContractDTO> Econtracts { get; set; } = new();
    }
}
