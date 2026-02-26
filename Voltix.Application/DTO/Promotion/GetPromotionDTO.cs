using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Promotion
{
    public class GetPromotionDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Range(0, 100)]
        public decimal? Percentage { get; set; }
        public int? FixedAmount { get; set; }
        public Guid? ModelId { get; set; }
        public Guid? VersionId { get; set; }
        public DiscountType DiscountType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
