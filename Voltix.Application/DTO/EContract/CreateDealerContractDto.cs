using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Application.DTO
{
    public class CreateDealerContractDto
    {
        public Guid DealerId { get; set; }

        // TUỲ CHỌN (override mặc định trong cấu hình VNPT)
        public string? Subject { get; set; }                // Mặc định: "HĐ Đại lý – {Dealer.Name}"
        public string? Description { get; set; }            // Mô tả HĐ
        public int? TypeId { get; set; }                    // Mặc định: từ VnptEContractOptions.TypeId
        public int? DepartmentId { get; set; }              // Mặc định: từ VnptEContractOptions.DepartmentId
        public string? DocumentNo { get; set; }             // Nếu null sẽ auto: "HDDL-yyyyMMddHHmmss"

        // TUỲ CHỌN (cách ký & người duyệt phía hãng)
        // D = Digital; DR = Drawn (ký vẽ). Mặc định D.
        public string DealerPermissionCode { get; set; } = "D";
        // userCode của người duyệt phía Hãng trên VNPT (nếu để trống → lấy từ cấu hình)
        public string? CompanyApproverUserCode { get; set; }
    }
}
