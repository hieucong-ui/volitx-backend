using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO.Log
{
    public class GetLogDTO
    {
        public Guid? DealerId { get; set; }
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public LogType LogType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
