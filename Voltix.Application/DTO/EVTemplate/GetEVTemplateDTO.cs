using Voltix.Application.DTO.ElectricVehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.EVTemplate
{
    public class GetEVTemplateDTO
    {
        public Guid Id { get; set; }
        public ViewVersionName? Version { get; set; }
        public ViewColorName? Color { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public List<string> ImgUrl { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
    public class ViewVersionName
    {
        public Guid VersionId { get; set; }
        public string? VersionName { get; set; }
        public Guid ModelId { get; set; }
        public string? ModelName { get; set; }
    }

    public class ViewColorName
    {
        public Guid ColorId { get; set; }
        public string? ColorName { get; set; }
    }
}
