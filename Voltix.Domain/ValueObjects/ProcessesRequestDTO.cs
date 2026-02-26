using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.ValueObjects
{
    public class ProcessesRequestDTO
    {
        public int OrderNo { get; set; }
        public string? ProcessedByUserCode { get; set; }
        public string? AccessPermissionCode { get; set; }
        public string? Position { get; set; }
        public int PageSign { get; set; }

        public ProcessesRequestDTO()
        {
        }

        public ProcessesRequestDTO(int orderNo, string? processedByUserCode, string? accessPermissionCode, string position, int pageSign)
        {
            OrderNo = orderNo;
            ProcessedByUserCode = processedByUserCode;
            AccessPermissionCode = accessPermissionCode;
            Position = position;
            PageSign = pageSign;
        }
    }
}
