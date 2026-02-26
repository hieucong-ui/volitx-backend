using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using System;

namespace Voltix.Infrastructure.Seeders
{
    public static class EContractSeeder
    {
        public static class EContractTemplateSeeder
        {
            private static readonly Guid DealerTier1TemplateId = Guid.Parse("bf236373-5b6a-435f-a5a6-2c29db2cae61");
            private static readonly Guid DealerTier2TemplateId = Guid.Parse("54b70bc8-e4bf-4da0-8d99-d478bcc4167f");
            private static readonly Guid DealerTier3TemplateId = Guid.Parse("529f745d-7074-4ac3-984a-e6f8cd9b1890");
            private static readonly Guid DealerTier4TemplateId = Guid.Parse("ea7d06d1-4ab7-433b-8c31-d425caf044c1");
            private static readonly Guid DealerTier5TemplateId = Guid.Parse("f9ef95dc-f943-42c6-992e-04739489eafd");

            private static readonly Guid BookingTemplateId = Guid.Parse("2e932187-140c-4ccf-807f-5e7cc1061663");

            private static readonly Guid CustomerDepositTemplateId = Guid.Parse("a3b5ed56-8d8f-4fd1-9f0a-4dbe2a0a44b7");
            private static readonly Guid CustomerFullPaymentTemplateId = Guid.Parse("d3f5c6e1-1f4e-4f4a-9f7a-2b6e8f0c3c9d");
            private static readonly Guid CustomerPayRemainderTemplateId = Guid.Parse("c7e8f9a2-3b4c-4d5e-8f9a-1b2c3d4e5f6a");

            private static readonly DateTime CreatedAtUtc = new DateTime(2025, 10, 06, 0, 0, 0, DateTimeKind.Utc);

            private const string CommonPartiesBlock = @"
  <div class=""meta-block"">
    <p><strong>BÊN A (Hãng / Doanh nghiệp):</strong></p>
    <p>- Tên pháp lý đầy đủ: {{ company.name }}</p>
    <p>- Địa chỉ trụ sở chính: {{ company.address }}</p>
    <p>- Mã số thuế: {{ company.taxNo }}</p>
    <p>- Điện thoại liên hệ: {{ company.phone }}</p>
    <p>- Email giao dịch: {{ company.email }}</p>
    <p>- Tài khoản thanh toán: {{ company.bankAccount }}</p>
    <p>- Ngân hàng mở tài khoản: {{ company.bankName }}</p>
    <p>- Người đại diện theo pháp luật / được uỷ quyền: {{ roles.A.representative }}</p>
    <p>- Chức vụ: {{ roles.A.title }}</p>
  </div>

  <div class=""meta-block"">
    <p><strong>BÊN B (Đại lý phân phối):</strong></p>
    <p>- Tên đơn vị / cửa hàng: {{ dealer.name }}</p>
    <p>- Địa chỉ kinh doanh / showroom: {{ dealer.address }}</p>
    <p>- Mã số thuế (nếu có): {{ dealer.taxNo }}</p>
    <p>- Điện thoại liên hệ: {{ dealer.phone }}</p>
    <p>- Email nhận thông báo hợp đồng / chính sách: {{ dealer.email }}</p>
    <p>- Người đại diện: {{ roles.B.representative }}</p>
    <p>- Chức vụ: {{ roles.B.title }}</p>
    <p>- Kênh liên hệ nhanh (Zalo/Hotline/CSKH): {{ dealer.contact }}</p>
    <p>- Tài khoản thanh toán: {{ dealer.bankAccount }}</p>
    <p>- Ngân hàng: {{ dealer.bankName }}</p>
  </div>
";

            private const string DealerTier1Html = @"
<!doctype html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN – {{ company.name }} (Đại lý chiến lược)</title>
  <style>
    @page { size: A4; margin: 10mm 14mm 14mm 14mm; }
    body { font-family: 'Noto Sans','DejaVu Sans','Arial',sans-serif; font-size: 12pt; line-height: 1.5; color: #000; }
    h1, h2, h3, h4 { text-align: center; margin: 4px 0; }
    .meta-block { margin-top: 10px; }
    .section-title { margin-top: 14px; margin-bottom: 4px; font-weight: bold; text-transform: uppercase; }
    .muted { color: #777; font-size: 10pt; }
    p { margin: 4px 0; }
    .signature-table { width: 100%; margin-top: 28px; table-layout: fixed; }
  </style>
</head>
<body>
  <h2>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h2>
  <h4>Độc lập - Tự do - Hạnh phúc</h4>
  <h2>HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN</h2>
  <p style=""text-align:center; margin-bottom:12px;""><b>(Áp dụng cho Đại lý chiến lược / Key Dealer)</b></p>

  <p><b>Căn cứ</b> Bộ luật Dân sự năm 2015;</p>
  <p><b>Căn cứ</b> Luật Thương mại năm 2005 và các văn bản hướng dẫn thi hành;</p>
  <p><b>Căn cứ</b> nhu cầu phát triển hệ thống đại lý phân phối sản phẩm xe máy điện của {{ company.name }};</p>
  <p><b>Căn cứ</b> nhu cầu kinh doanh và cam kết của {{ dealer.name }};</p>
  <p><b>Hôm nay, ngày {{ contract.date }}</b>, tại TP. Hồ Chí Minh, chúng tôi gồm có:</p>
" + CommonPartiesBlock + @"
  <div class=""section-title"">Điều 1. Mục đích và phạm vi hợp tác</div>
  <p>1.1. Bên A đồng ý chỉ định Bên B làm <b>Đại lý phân phối cấp chiến lược</b> các sản phẩm xe máy điện, linh kiện, phụ kiện và các dịch vụ hậu mãi do Bên A sản xuất / cung cấp / sở hữu thương hiệu.</p>
  <p>1.2. Phạm vi đại lý cơ bản: <b>Toàn lãnh thổ Việt Nam</b>. Trường hợp Bên A ban hành phụ lục quy định khu vực độc quyền hoặc khu vực ưu tiên thì Bên B phải tuân thủ đúng phạm vi đó.</p>
  <p>1.3. Mục tiêu hợp tác: mở rộng mạng lưới phân phối, chuẩn hóa hình ảnh thương hiệu, đảm bảo thống nhất chính sách giá và nâng cao trải nghiệm dịch vụ khách hàng cuối cùng.</p>

  <div class=""section-title"">Điều 2. Cấp đại lý và chính sách thương mại áp dụng</div>
  <p>2.1. Theo cấu hình hiện tại trên hệ thống quản lý đại lý của Bên A, Bên B được xếp vào <b>cấp: {{ dealer.tier.name }}</b>, tương ứng <b>Level {{ dealer.tier.level }}</b>.</p>
  <p>2.2. <b>Mô tả cấp:</b> {{ dealer.tier.description }}. Đây là nhóm đại lý được ưu tiên về hàng hóa, được cấp hạn mức công nợ cao hơn các cấp thông thường và được tham gia trước các chương trình hỗ trợ kinh doanh mới.</p>
  <p>2.3. <b>Quyền lợi thương mại cơ bản:</b></p>
  <p style=""margin-left:14px;"">
    - Được áp dụng mức <b>hoa hồng / chiết khấu cơ bản</b> là <b>{{ dealer.tier.baseCommissionPercent }}%</b> trên giá bán đại lý đang có hiệu lực;<br/>
    - Được cấp <b>hạn mức công nợ cơ bản</b> tối đa là <b>{{ dealer.tier.baseCreditLimit }} VND</b>; hạn mức này có thể tăng khi doanh số cao và lịch sử thanh toán tốt;<br/>
    - Được tham gia các chương trình thưởng doanh số, hỗ trợ trưng bày, hỗ trợ marketing mà Bên A áp dụng cho nhóm đại lý chiến lược;<br/>
    - Được ưu tiên giao hàng trước khi Bên A phân bổ cho các đại lý cấp thấp hơn trong trường hợp nguồn hàng có hạn.
  </p>
  <p>2.4. <b>Nghĩa vụ tài chính tương ứng:</b></p>
  <p style=""margin-left:14px;"">
    - Đối với đơn hàng thuộc nhóm yêu cầu đặt cọc/ứng trước, Bên B phải thanh toán tối thiểu <b>{{ dealer.tier.baseDepositPercent }}%</b> giá trị đơn hàng;<br/>
    - Nếu Bên B thanh toán chậm so với thời hạn đã ghi nhận trong đơn hàng hoặc phụ lục, phần dư nợ chậm trả sẽ bị áp dụng <b>mức phạt/lãi chậm thanh toán là {{ dealer.tier.baseLatePenaltyPercent }}%/tháng</b> tính trên số tiền thực tế chậm;<br/>
    - Việc chậm thanh toán từ 02 (hai) kỳ trở lên là căn cứ để Bên A tạm dừng giao hàng, thu hồi chính sách ưu đãi hoặc hạ cấp đại lý.
  </p>
  <p>2.5. <b>Thời điểm hiệu lực của cấp:</b> việc xếp hạng nêu trên được hệ thống của Bên A ghi nhận ngày <b>{{ dealer.tier.createdAt }}</b> và cập nhật gần nhất ngày <b>{{ dealer.tier.updatedAt }}</b>. Các lần điều chỉnh sau này của Bên A trên hệ thống (nếu có) được xem là cập nhật chính sách và có hiệu lực đối với Bên B kể từ thời điểm thông báo.</p>

  <div class=""section-title"">Điều 3. Sản phẩm, giá và phụ lục</div>
  <p>3.1. Danh mục sản phẩm, quy cách, màu sắc, cấu hình pin, phụ kiện kèm theo… được quy định tại <b>Phụ lục 01 – Danh mục sản phẩm</b> ban hành kèm theo hợp đồng này.</p>
  <p>3.2. Bảng giá và chính sách chiết khấu do Bên A ban hành từng thời kỳ. Khi có thay đổi, Bên A gửi thông báo qua email/hệ thống; Bên B được xem là đã nhận sau 24 giờ kể từ thời điểm gửi.</p>
  <p>3.3. Bên B phải tuân thủ giá bán lẻ khuyến nghị hoặc trong khung giá Bên A cho phép nhằm đảm bảo cạnh tranh lành mạnh giữa các đại lý.</p>

  <div class=""section-title"">Điều 4. Đặt hàng và giao nhận</div>
  <p>4.1. Bên B gửi đơn đặt hàng thông qua hệ thống đặt hàng điện tử, email hoặc văn bản theo mẫu.</p>
  <p>4.2. Trong vòng <b>02 (hai) ngày làm việc</b> kể từ khi nhận đủ thông tin, Bên A sẽ phản hồi xác nhận đơn hoặc đề nghị chỉnh sửa.</p>
  <p>4.3. Địa điểm giao hàng: kho của Bên A hoặc địa điểm giao nhận do Bên A chỉ định; chi phí vận chuyển sẽ theo chính sách tại thời điểm giao.</p>
  <p>4.4. Rủi ro về hàng hóa được chuyển từ Bên A sang Bên B kể từ thời điểm hai bên ký biên bản giao nhận.</p>

  <div class=""section-title"">Điều 5. Thanh toán và công nợ</div>
  <p>5.1. Hình thức thanh toán: chuyển khoản hoặc thanh khoản ngân hàng vào tài khoản của Bên A.</p>
  <p>5.2. Thời hạn thanh toán cụ thể của từng đơn sẽ được thể hiện trong đơn hàng hoặc phụ lục hợp đồng.</p>
  <p>5.3. Bên A có quyền điều chỉnh hạn mức công nợ đã cấp nếu Bên B không tuân thủ đúng hạn thanh toán.</p>

  <div class=""section-title"">Điều 6. Bảo hành, hỗ trợ kỹ thuật và khiếu nại</div>
  <p>6.1. Sản phẩm được bảo hành theo chính sách bảo hành hiện hành do Bên A công bố.</p>
  <p>6.2. Mọi khiếu nại về số lượng, chất lượng, sai khác chủng loại phải được Bên B thông báo cho Bên A trong vòng <b>03 (ba) ngày</b> kể từ ngày nhận hàng để được xem xét xử lý.</p>

  <div class=""section-title"">Điều 7. Bảo mật và hình ảnh thương hiệu</div>
  <p>7.1. Bên B cam kết giữ bí mật các thông tin về bảng giá, chiết khấu, cấu trúc xếp hạng đại lý, chính sách thưởng doanh số của Bên A.</p>
  <p>7.2. Bên B được quyền sử dụng hình ảnh, logo, ấn phẩm truyền thông của Bên A trong phạm vi thực hiện hợp đồng và phải tuân thủ bộ nhận diện do Bên A ban hành.</p>

  <div class=""section-title"">Điều 8. Báo cáo, kiểm tra và phối hợp</div>
  <p>8.1. Bên B có trách nhiệm báo cáo định kỳ theo mẫu của Bên A: doanh số, tồn kho, phản hồi khách hàng.</p>
  <p>8.2. Bên A có quyền cử người đến kiểm tra thực tế việc trưng bày, bảo quản hàng hoá và việc tuân thủ nhận diện thương hiệu.</p>

  <div class=""section-title"">Điều 9. Thời hạn hợp đồng và chấm dứt</div>
  <p>9.1. Hợp đồng có hiệu lực từ ngày <b>{{ contract.effectiveDate }}</b> và hết hiệu lực vào ngày <b>{{ contract.expiryDate }}</b>, trừ khi hai bên có thỏa thuận gia hạn bằng văn bản.</p>
  <p>9.2. Mỗi bên có quyền chấm dứt hợp đồng trước thời hạn nếu bên kia vi phạm nghĩa vụ trọng yếu và không khắc phục trong vòng <b>15 ngày</b> kể từ ngày nhận thông báo.</p>

  <div class=""section-title"">Điều 10. Giải quyết tranh chấp</div>
  <p>10.1. Hai bên ưu tiên giải quyết tranh chấp thông qua thương lượng.</p>
  <p>10.2. Trường hợp không thương lượng được, tranh chấp sẽ được đưa ra cơ quan có thẩm quyền tại <b>TP. Hồ Chí Minh</b> để giải quyết theo pháp luật Việt Nam.</p>

  <table class=""signature-table"">
    <tr>
      <td style=""width:47%; vertical-align:top; text-align:left; min-height:110px;"">
        <div style=""font-size:10pt; color:#777;"">ĐẠI DIỆN BÊN A</div>
        <div><strong>{{ roles.A.representative }}</strong></div>
        <div>{{ roles.A.title }}</div>
        <div style=""font-size:9pt; margin-top:6px;"">(Ký, ghi rõ họ tên, đóng dấu)</div>
        <div style=""font-size:1pt; color:#ffffff; opacity:0.01;"">{{ roles.A.signatureAnchor }}</div>
      </td>
      <td style=""width:6%;"">&nbsp;</td>
      <td style=""width:47%; vertical-align:top; text-align:right; min-height:110px;"">
        <div style=""font-size:10pt; color:#777;"">ĐẠI DIỆN BÊN B</div>
        <div><strong>{{ roles.B.representative }}</strong></div>
        <div>{{ roles.B.title }}</div>
        <div style=""font-size:9pt; margin-top:6px;"">(Ký, ghi rõ họ tên, đóng dấu)</div>
        <div style=""font-size:1pt; color:#ffffff; opacity:0.01;"">{{ roles.B.signatureAnchor }}</div>
      </td>
    </tr>
  </table>

  <div class=""muted"">Trang {{ page }} / {{ pages }}</div>
</body>
</html>
";

            private const string DealerTier2Html = @"
<!doctype html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN – {{ company.name }} (Đại lý cấp vàng)</title>
  <style>
    @page { size:A4; margin:10mm 14mm 14mm 14mm; }
    body { font-family:'Noto Sans','DejaVu Sans','Arial',sans-serif; font-size:12pt; line-height:1.5; }
    .section-title { margin-top:14px; font-weight:bold; text-transform:uppercase; }
    .muted { color:#777; font-size:10pt; }
  </style>
</head>
<body>
  <h2 style=""text-align:center;"">HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN</h2>
  <p style=""text-align:center; margin-bottom:10px;""><b>(Áp dụng cho Đại lý cấp vàng / doanh số ổn định)</b></p>

  <p><b>Căn cứ</b> Bộ luật Dân sự 2015;</p>
  <p><b>Căn cứ</b> Luật Thương mại 2005;</p>
  <p><b>Căn cứ</b> chiến lược phát triển mạng lưới phân phối của {{ company.name }};</p>
  <p><b>Căn cứ</b> đề nghị và hồ sơ đủ điều kiện của {{ dealer.name }};</p>
  <p><b>Hôm nay, ngày {{ contract.date }}</b>, tại TP. Hồ Chí Minh, chúng tôi gồm có:</p>
" + CommonPartiesBlock + @"
  <div class=""section-title"">Điều 1. Mục đích và phạm vi hợp tác</div>
  <p>1.1. Hai bên thống nhất thiết lập quan hệ đại lý để Bên B được phép phân phối, trưng bày, bán lẻ và triển khai các dịch vụ sau bán hàng đối với các sản phẩm xe máy điện mang thương hiệu {{ company.name }}.</p>
  <p>1.2. Phạm vi kinh doanh mặc định: <b>Toàn quốc</b>, trừ khi Bên A có văn bản hoặc phụ lục giới hạn khu vực nhằm bảo vệ sự cân bằng của hệ thống đại lý.</p>
  <p>1.3. Mục đích bao gồm nhưng không giới hạn: mở rộng độ phủ sản phẩm, gia tăng doanh số tại địa phương của Bên B, chuẩn hóa trải nghiệm khách hàng theo tiêu chuẩn của Bên A.</p>

  <div class=""section-title"">Điều 2. Cấp đại lý và chính sách thương mại</div>
  <p>2.1. Trên hệ thống quản lý của Bên A, Bên B được xếp ở <b>cấp đại lý: {{ dealer.tier.name }}</b>, tương ứng <b>Level {{ dealer.tier.level }}</b>.</p>
  <p>2.2. <b>Mô tả chi tiết cấp:</b> {{ dealer.tier.description }}. Cấp này thường dành cho các đại lý đã có lượng mua đều đặn, khả năng xoay vòng dòng tiền tốt, cam kết được doanh số tối thiểu hàng tháng/quý và tuân thủ tốt chính sách thương hiệu.</p>
  <p>2.3. <b>Quyền lợi tài chính cơ bản:</b></p>
  <p style=""margin-left:14px;"">
    - Mức chiết khấu / hoa hồng cơ bản được hưởng: <b>{{ dealer.tier.baseCommissionPercent }}%</b> trên giá bán đại lý Bên A công bố và còn hiệu lực;<br/>
    - Được hưởng <b>hạn mức công nợ cơ bản</b> tối đa: <b>{{ dealer.tier.baseCreditLimit }} VND</b> để chủ động nguồn vốn kinh doanh;<br/>
    - Được tham gia các chương trình thưởng nóng, khuyến mại, hỗ trợ vận chuyển nếu đáp ứng điều kiện mà Bên A quy định cho nhóm đại lý vàng;<br/>
    - Được ưu tiên thông tin về sản phẩm mới, mẫu mới, chính sách thúc đẩy bán hàng.
  </p>
  <p>2.4. <b>Nghĩa vụ tài chính tương ứng:</b></p>
  <p style=""margin-left:14px;"">
    - Các đơn thuộc nhóm phải đặt cọc/ứng trước, Bên B bắt buộc thanh toán tối thiểu <b>{{ dealer.tier.baseDepositPercent }}%</b> giá trị đơn để được giữ hàng;<br/>
    - Nếu thanh toán trễ so với lịch đã xác nhận, phần dư nợ chậm sẽ chịu <b>{{ dealer.tier.baseLatePenaltyPercent }}%/tháng</b> tính trên số tiền chậm thực tế;<br/>
    - Trường hợp Bên B thanh toán trễ từ hai kỳ trở lên hoặc không duy trì doanh số tối thiểu, Bên A có thể điều chỉnh hạn mức công nợ hoặc tạm dừng ưu đãi.
  </p>
  <p>2.5. Các thông số trên được hệ thống ghi nhận ngày <b>{{ dealer.tier.createdAt }}</b> và cập nhật gần nhất ngày <b>{{ dealer.tier.updatedAt }}</b>; mọi thay đổi về sau do Bên A ban hành qua hệ thống / email đều được xem là một phần của Hợp đồng này.</p>

  <div class=""section-title"">Điều 3. Sản phẩm, giá và phụ lục</div>
  <p>3.1. Danh mục sản phẩm áp dụng cho đại lý cấp vàng được thể hiện tại Phụ lục 01.</p>
  <p>3.2. Bảng giá bán sỉ/đại lý, chính sách khuyến mại, thưởng doanh số được gửi qua email hoặc hệ thống và có hiệu lực từ thời điểm nêu trong thông báo.</p>
  <p>3.3. Bên B không được tự ý bán phá giá, giảm sâu hoặc tặng kèm vượt quy định gây ảnh hưởng đến hệ thống đại lý xung quanh.</p>

  <div class=""section-title"">Điều 4. Đặt hàng – giao nhận – phân bổ hàng hóa</div>
  <p>4.1. Đặt hàng qua hệ thống hoặc biểu mẫu của Bên A; khi cần gấp có thể xác nhận qua email nhưng phải bổ sung đơn nội bộ sau.</p>
  <p>4.2. Thời gian phản hồi/xác nhận đơn của Bên A: tối đa <b>02 ngày làm việc</b> kể từ khi nhận đủ thông tin.</p>
  <p>4.3. Giao hàng tại kho Bên A hoặc địa điểm giao nhận do Bên A chỉ định; chi phí vận chuyển theo chính sách.</p>
  <p>4.4. Bên A được quyền ưu tiên phân bổ hàng cho đại lý chiến lược hoặc khu vực đang thiếu hàng, nhưng phải thông báo cho Bên B nếu ảnh hưởng đến đơn của Bên B.</p>

  <div class=""section-title"">Điều 5. Thanh toán và công nợ</div>
  <p>5.1. Thanh toán qua chuyển khoản/tài khoản ngân hàng của Bên A.</p>
  <p>5.2. Thời hạn thanh toán thể hiện trên từng đơn/phụ lục; nếu quá hạn sẽ chuyển sang trạng thái công nợ quá hạn và phải chịu phạt theo mục 2.4.</p>
  <p>5.3. Bên A có thể giảm hạn mức công nợ được cấp nếu Bên B vi phạm quá 02 lần trong 01 quý.</p>

  <div class=""section-title"">Điều 6. Bảo hành, hỗ trợ và khiếu nại</div>
  <p>6.1. Hàng hóa được bảo hành theo chính sách bảo hành hiện hành của Bên A.</p>
  <p>6.2. Mọi khiếu nại về số lượng, chất lượng, model/màu sắc sai khác phải được thông báo trong vòng <b>03 ngày làm việc</b> kể từ ngày nhận hàng.</p>
  <p>6.3. Bên A hỗ trợ tài liệu kỹ thuật, hướng dẫn sử dụng, hình ảnh trưng bày để Bên B bán hàng đúng chuẩn.</p>

  <div class=""section-title"">Điều 7. Hình ảnh, thương hiệu và bảo mật</div>
  <p>7.1. Bên B chỉ sử dụng logo, hình ảnh, ấn phẩm do Bên A cung cấp hoặc phê duyệt.</p>
  <p>7.2. Bên B không được cung cấp bảng giá, mức chiết khấu, cấu trúc xếp hạng đại lý cho đơn vị thứ ba nếu chưa được Bên A đồng ý.</p>

  <div class=""section-title"">Điều 8. Báo cáo và phối hợp</div>
  <p>8.1. Bên B gửi báo cáo bán hàng/tồn kho theo kỳ mà Bên A yêu cầu.</p>
  <p>8.2. Bên A có thể kiểm tra đột xuất để đánh giá việc tuân thủ chính sách và chất lượng dịch vụ.</p>

  <div class=""section-title"">Điều 9. Thời hạn và chấm dứt</div>
  <p>9.1. Hợp đồng có hiệu lực từ ngày <b>{{ contract.effectiveDate }}</b> đến ngày <b>{{ contract.expiryDate }}</b>.</p>
  <p>9.2. Một bên có quyền chấm dứt trước hạn nếu bên kia vi phạm nghĩa vụ trọng yếu và không khắc phục trong 15 ngày kể từ ngày nhận thông báo.</p>

  <div class=""section-title"">Điều 10. Giải quyết tranh chấp</div>
  <p>10.1. Ưu tiên thương lượng, hoà giải.</p>
  <p>10.2. Nếu không hoà giải được thì đưa ra cơ quan có thẩm quyền tại <b>TP. Hồ Chí Minh</b> để giải quyết theo pháp luật Việt Nam.</p>

  <table style=""width:100%; margin-top:28px; table-layout:fixed;"">
    <tr>
      <td style=""width:47%; text-align:left; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN A</div>
        <div><strong>{{ roles.A.representative }}</strong></div>
        <div>{{ roles.A.title }}</div>
        <div style=""font-size:9pt; margin-top:6px;"">(Ký, ghi rõ họ tên, đóng dấu)</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.A.signatureAnchor }}</div>
      </td>
      <td style=""width:6%;"">&nbsp;</td>
      <td style=""width:47%; text-align:right; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN B</div>
        <div><strong>{{ roles.B.representative }}</strong></div>
        <div>{{ roles.B.title }}</div>
        <div style=""font-size:9pt; margin-top:6px;"">(Ký, ghi rõ họ tên, đóng dấu)</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.B.signatureAnchor }}</div>
      </td>
    </tr>
  </table>

  <div class=""muted"">Trang {{ page }} / {{ pages }}</div>
</body>
</html>
";

            private const string DealerTier3Html = @"
<!doctype html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN – {{ company.name }} (Đại lý cấp bạc)</title>
  <style>
    @page { size:A4; margin:10mm 14mm 14mm 14mm; }
    body { font-family:'Noto Sans','DejaVu Sans','Arial',sans-serif; font-size:12pt; line-height:1.5; }
    .section-title { margin-top:14px; font-weight:bold; text-transform:uppercase; }
    .muted { color:#777; font-size:10pt; }
  </style>
</head>
<body>
  <h2 style=""text-align:center;"">HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN</h2>
  <p style=""text-align:center;""><b>(Áp dụng cho Đại lý cấp bạc – đang phát triển)</b></p>
  <p><b>Căn cứ</b> Bộ luật Dân sự 2015; <b>căn cứ</b> Luật Thương mại 2005; <b>căn cứ</b> nhu cầu phát triển thị trường của {{ company.name }}; <b>căn cứ</b> đề nghị làm đại lý của {{ dealer.name }}.</p>
  <p><b>Hôm nay, ngày {{ contract.date }}</b>, tại TP. Hồ Chí Minh, các bên gồm:</p>
" + CommonPartiesBlock + @"
  <div class=""section-title"">Điều 1. Nội dung và phạm vi</div>
  <p>1.1. Bên A chấp thuận để Bên B làm đại lý phân phối sản phẩm xe máy điện của Bên A trong phạm vi cho phép.</p>
  <p>1.2. Bên B có quyền trưng bày, giới thiệu, bán lẻ, bán lại cho khách hàng tổ chức/cá nhân, miễn là tuân thủ đúng chính sách giá và nhận diện của Bên A.</p>
  <p>1.3. Mục tiêu: gia tăng độ phủ sản phẩm tại khu vực Bên B đang khai thác, đồng thời đánh giá năng lực để xem xét nâng cấp đại lý.</p>

  <div class=""section-title"">Điều 2. Cấp đại lý và chính sách thương mại</div>
  <p>2.1. Cấp đại lý được ghi nhận trên hệ thống tại thời điểm ký: <b>{{ dealer.tier.name }}</b> (Level {{ dealer.tier.level }}).</p>
  <p>2.2. <b>Mô tả chi tiết cấp:</b> {{ dealer.tier.description }}. Đây là nhóm đại lý đã hoạt động ổn, có tiềm năng tăng trưởng nhưng vẫn cần tuân thủ chặt các quy định để được xét nâng hạng.</p>
  <p>2.3. <b>Quyền lợi:</b></p>
  <p style=""margin-left:14px;"">
    - Hưởng mức chiết khấu / hoa hồng cơ bản là <b>{{ dealer.tier.baseCommissionPercent }}%</b>;<br/>
    - Được cấp hạn mức công nợ tối đa <b>{{ dealer.tier.baseCreditLimit }} VND</b> để nhập hàng;<br/>
    - Được tham gia các chương trình kích cầu, hỗ trợ bán hàng do Bên A triển khai cho nhóm đại lý phát triển;<br/>
    - Được Bên A cung cấp ấn phẩm, hình ảnh, tài liệu đào tạo bán hàng.
  </p>
  <p>2.4. <b>Nghĩa vụ:</b></p>
  <p style=""margin-left:14px;"">
    - Đặt cọc tối thiểu <b>{{ dealer.tier.baseDepositPercent }}%</b> đối với các đơn thuộc diện phải ứng trước;<br/>
    - Trường hợp thanh toán chậm, phải chịu mức phạt/lãi chậm là <b>{{ dealer.tier.baseLatePenaltyPercent }}%/tháng</b> trên phần chậm;<br/>
    - Nếu không đạt doanh số tối thiểu trong nhiều kỳ liên tiếp hoặc vi phạm thanh toán, Bên A có thể giữ nguyên cấp hoặc hạ cấp.
  </p>
  <p>2.5. Ngày cấu hình cấp: <b>{{ dealer.tier.createdAt }}</b> – Ngày cập nhật gần nhất: <b>{{ dealer.tier.updatedAt }}</b>. Các thay đổi sau này nếu được cập nhật trên hệ thống và thông báo cho Bên B sẽ đương nhiên có hiệu lực.</p>

  <div class=""section-title"">Điều 3. Sản phẩm, giá và phụ lục</div>
  <p>3.1. Danh mục sản phẩm được phép kinh doanh thể hiện tại Phụ lục 01.</p>
  <p>3.2. Mức giá áp dụng là mức giá đại lý Bên A công bố. Khi có thay đổi, Bên A gửi thông báo qua email/hệ thống.</p>
  <p>3.3. Bên B không được bán thấp hơn giá sàn, không được tự ý đóng gói khuyến mại làm sai lệch chính sách của Bên A.</p>

  <div class=""section-title"">Điều 4. Đặt hàng – giao nhận</div>
  <p>4.1. Đơn hàng gửi qua hệ thống hoặc biểu mẫu chuẩn.</p>
  <p>4.2. Thời gian xác nhận: tối đa 02 ngày làm việc.</p>
  <p>4.3. Giao tại kho hoặc điểm nhận do Bên A chỉ định; rủi ro chuyển sang Bên B tại thời điểm ký nhận.</p>

  <div class=""section-title"">Điều 5. Thanh toán và công nợ</div>
  <p>5.1. Thanh toán qua chuyển khoản ngân hàng.</p>
  <p>5.2. Thời hạn thanh toán theo từng đơn/phụ lục; chậm sẽ chịu phạt theo Điều 2.4.</p>
  <p>5.3. Bên A có thể giảm hạn mức công nợ nếu Bên B vi phạm.</p>

  <div class=""section-title"">Điều 6. Bảo hành, khiếu nại</div>
  <p>6.1. Bảo hành theo chính sách hiện hành của Bên A.</p>
  <p>6.2. Khiếu nại trong vòng 03 ngày kể từ khi nhận hàng.</p>

  <div class=""section-title"">Điều 7. Hình ảnh và bảo mật</div>
  <p>7.1. Bên B phải trưng bày theo hướng dẫn nhận diện của Bên A.</p>
  <p>7.2. Không tiết lộ thông tin chính sách, bảng giá, cấu trúc tier.</p>

  <div class=""section-title"">Điều 8. Báo cáo và kiểm tra</div>
  <p>8.1. Bên B gửi báo cáo định kỳ cho Bên A.</p>
  <p>8.2. Bên A có quyền kiểm tra thực tế.</p>

  <div class=""section-title"">Điều 9. Hiệu lực và chấm dứt</div>
  <p>9.1. Hợp đồng có hiệu lực từ ngày <b>{{ contract.effectiveDate }}</b> đến ngày <b>{{ contract.expiryDate }}</b>.</p>
  <p>9.2. Một bên được quyền chấm dứt nếu bên kia vi phạm và không khắc phục trong 15 ngày.</p>

  <div class=""section-title"">Điều 10. Giải quyết tranh chấp</div>
  <p>10.1. Ưu tiên thương lượng.</p>
  <p>10.2. Không được thì đưa ra tòa/trọng tài tại <b>TP. Hồ Chí Minh</b>.</p>

  <table style=""width:100%; margin-top:28px; table-layout:fixed;"">
    <tr>
      <td style=""width:47%; text-align:left; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN A</div>
        <div><strong>{{ roles.A.representative }}</strong></div>
        <div>{{ roles.A.title }}</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.A.signatureAnchor }}</div>
      </td>
      <td style=""width:6%;"">&nbsp;</td>
      <td style=""width:47%; text-align:right; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN B</div>
        <div><strong>{{ roles.B.representative }}</strong></div>
        <div>{{ roles.B.title }}</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.B.signatureAnchor }}</div>
      </td>
    </tr>
  </table>

  <div class=""muted"">Trang {{ page }} / {{ pages }}</div>
</body>
</html>
";

            private const string DealerTier4Html = @"
<!doctype html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN – {{ company.name }} (Đại lý tiêu chuẩn)</title>
  <style>
    @page { size:A4; margin:10mm 14mm 14mm 14mm; }
    body { font-family:Arial,sans-serif; font-size:12pt; line-height:1.5; }
    .section-title { margin-top:14px; font-weight:bold; text-transform:uppercase; }
    .muted { color:#777; font-size:10pt; }
  </style>
</head>
<body>
  <h3 style=""text-align:center;"">HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN</h3>
  <p style=""text-align:center;""><b>(Áp dụng cho Đại lý cấp tiêu chuẩn)</b></p>
  <p><b>Căn cứ</b> Bộ luật Dân sự 2015; <b>căn cứ</b> Luật Thương mại 2005; <b>căn cứ</b> nhu cầu phân phối sản phẩm của {{ company.name }}; <b>căn cứ</b> năng lực hiện tại của {{ dealer.name }}.</p>
  <p><b>Hôm nay, ngày {{ contract.date }}</b>, tại TP. Hồ Chí Minh, chúng tôi gồm:</p>
" + CommonPartiesBlock + @"
  <div class=""section-title"">Điều 1. Nội dung</div>
  <p>1.1. Bên A cho phép Bên B bán lẻ, trưng bày, giới thiệu sản phẩm xe máy điện của Bên A theo chính sách và hướng dẫn.</p>
  <p>1.2. Đây là cấp đại lý phổ biến, yêu cầu Bên B tuân thủ nghiêm các quy định về hình ảnh và giá để đảm bảo đồng nhất toàn hệ thống.</p>

  <div class=""section-title"">Điều 2. Chính sách theo cấp đại lý</div>
  <p>2.1. Cấp hiện tại trên hệ thống: <b>{{ dealer.tier.name }}</b> (Level {{ dealer.tier.level }}).</p>
  <p>2.2. <b>Mô tả cấp:</b> {{ dealer.tier.description }} – phù hợp với đại lý đã đủ điều kiện cơ bản, cần duy trì doanh số và thanh toán đúng hạn.</p>
  <p>2.3. <b>Chính sách thương mại:</b></p>
  <p style=""margin-left:14px;"">
    - Chiết khấu / hoa hồng cơ bản: <b>{{ dealer.tier.baseCommissionPercent }}%</b>;<br/>
    - Hạn mức công nợ cơ bản: <b>{{ dealer.tier.baseCreditLimit }} VND</b>;<br/>
    - Đặt cọc / ứng trước: <b>{{ dealer.tier.baseDepositPercent }}%</b> giá trị đơn hàng;<br/>
    - Phạt / lãi chậm thanh toán: <b>{{ dealer.tier.baseLatePenaltyPercent }}%/tháng</b> trên số tiền chậm;<br/>
    - Ngày cấu hình cấp: {{ dealer.tier.createdAt }}; Ngày cập nhật gần nhất: {{ dealer.tier.updatedAt }}.
  </p>

  <div class=""section-title"">Điều 3. Sản phẩm, giá và phụ lục</div>
  <p>3.1. Danh mục sản phẩm: Phụ lục 01.</p>
  <p>3.2. Giá và khuyến mại theo thông báo của Bên A; Bên B phải bán theo đúng khung giá cho phép.</p>

  <div class=""section-title"">Điều 4. Đặt hàng – giao hàng</div>
  <p>4.1. Đặt qua hệ thống; xác nhận tối đa 02 ngày làm việc.</p>
  <p>4.2. Giao tại kho hoặc điểm nhận do Bên A chỉ định; rủi ro chuyển sang Bên B khi ký nhận.</p>

  <div class=""section-title"">Điều 5. Thanh toán</div>
  <p>5.1. Thanh toán chuyển khoản.</p>
  <p>5.2. Quá hạn bị phạt theo Điều 2.3.</p>

  <div class=""section-title"">Điều 6. Bảo hành và khiếu nại</div>
  <p>6.1. Thực hiện theo chính sách bảo hành hiện hành.</p>
  <p>6.2. Khiếu nại trong 03 ngày.</p>

  <div class=""section-title"">Điều 7. Hình ảnh và bảo mật</div>
  <p>7.1. Bên B chỉ sử dụng hình ảnh do Bên A duyệt.</p>
  <p>7.2. Không tiết lộ bảng giá, chiết khấu, cấu trúc tier.</p>

  <div class=""section-title"">Điều 8. Báo cáo và kiểm tra</div>
  <p>8.1. Bên B báo cáo bán hàng theo yêu cầu.</p>
  <p>8.2. Bên A có quyền kiểm tra thực tế.</p>

  <div class=""section-title"">Điều 9. Thời hạn và chấm dứt</div>
  <p>9.1. Hợp đồng có hiệu lực từ <b>{{ contract.effectiveDate }}</b> đến <b>{{ contract.expiryDate }}</b>.</p>
  <p>9.2. Vi phạm không khắc phục trong 15 ngày là căn cứ chấm dứt.</p>

  <div class=""section-title"">Điều 10. Tranh chấp</div>
  <p>10.1. Thương lượng trước.</p>
  <p>10.2. Không được thì giải quyết tại <b>TP. Hồ Chí Minh</b>.</p>

  <table style=""width:100%; margin-top:28px; table-layout:fixed;"">
    <tr>
      <td style=""width:47%; text-align:left; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN A</div>
        <div><strong>{{ roles.A.representative }}</strong></div>
        <div>{{ roles.A.title }}</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.A.signatureAnchor }}</div>
      </td>
      <td style=""width:6%;"">&nbsp;</td>
      <td style=""width:47%; text-align:right; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN B</div>
        <div><strong>{{ roles.B.representative }}</strong></div>
        <div>{{ roles.B.title }}</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.B.signatureAnchor }}</div>
      </td>
    </tr>
  </table>
</body>
</html>
";


            private const string DealerTier5Html = @"
<!doctype html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN – {{ company.name }} (Đại lý mới)</title>
  <style>
    @page { size:A4; margin:10mm 14mm 14mm 14mm; }
    body { font-family:Arial,sans-serif; font-size:12pt; line-height:1.5; }
    .section-title { margin-top:14px; font-weight:bold; text-transform:uppercase; }
    .muted { color:#777; font-size:10pt; }
  </style>
</head>
<body>
  <h3 style=""text-align:center;"">HỢP ĐỒNG ĐẠI LÝ PHÂN PHỐI XE MÁY ĐIỆN</h3>
  <p style=""text-align:center;""><b>(Áp dụng cho Đại lý mới – giai đoạn đánh giá 3–6 tháng)</b></p>
  <p><b>Căn cứ</b> Bộ luật Dân sự 2015; <b>căn cứ</b> Luật Thương mại 2005; <b>căn cứ</b> chính sách phát triển hệ thống đại lý của {{ company.name }}; <b>căn cứ</b> đề nghị làm đại lý của {{ dealer.name }}.</p>
  <p><b>Hôm nay, ngày {{ contract.date }}</b>, tại TP. Hồ Chí Minh, các bên gồm:</p>
" + CommonPartiesBlock + @"
  <div class=""section-title"">Điều 1. Mục đích</div>
  <p>1.1. Bên A cho phép Bên B – là đơn vị mới tham gia hệ thống – được kinh doanh, trưng bày, giới thiệu và bán lẻ sản phẩm xe máy điện của Bên A.</p>
  <p>1.2. Bên A sẽ theo dõi, đánh giá trong giai đoạn đầu (thường từ 03 đến 06 tháng) để quyết định việc giữ nguyên chính sách, nâng cấp hoặc chấm dứt hợp tác.</p>
  <p>1.3. Trong giai đoạn này, Bên B phải tuyệt đối tuân thủ về thanh toán và hình ảnh để tạo uy tín với Bên A.</p>

  <div class=""section-title"">Điều 2. Chính sách tạm áp dụng theo cấp đại lý</div>
  <p>2.1. Hệ thống ghi nhận Bên B ở cấp: <b>{{ dealer.tier.name }}</b> (Level {{ dealer.tier.level }}).</p>
  <p>2.2. <b>Mô tả chi tiết cấp:</b> {{ dealer.tier.description }} – đây là cấp cơ sở, được mở để khởi động kinh doanh, chưa có đầy đủ ưu đãi như các cấp cao hơn.</p>
  <p>2.3. <b>Các thông số thương mại áp dụng ngay:</b></p>
  <p style=""margin-left:14px;"">
    - Chiết khấu / hoa hồng cơ bản: <b>{{ dealer.tier.baseCommissionPercent }}%</b>;<br/>
    - Hạn mức công nợ cơ bản (nếu được cấp): <b>{{ dealer.tier.baseCreditLimit }} VND</b>;<br/>
    - Đặt cọc / ứng trước tối thiểu: <b>{{ dealer.tier.baseDepositPercent }}%</b> giá trị đơn hàng;<br/>
    - Phạt / lãi chậm thanh toán: <b>{{ dealer.tier.baseLatePenaltyPercent }}%/tháng</b> tính trên dư nợ chậm;<br/>
    - Ngày cấu hình cấp: {{ dealer.tier.createdAt }}; ngày cập nhật gần nhất: {{ dealer.tier.updatedAt }}.
  </p>
  <p>2.4. <b>Yêu cầu đặc biệt đối với đại lý mới:</b></p>
  <p style=""margin-left:14px;"">
    - Phải thanh toán đúng hạn trong 03 đơn hàng liên tiếp đầu tiên;<br/>
    - Phải treo biển hiệu, trưng bày theo đúng hướng dẫn của Bên A;<br/>
    - Phải báo cáo bán hàng hằng tuần trong 03 tháng đầu;<br/>
    - Nếu vi phạm 02 lần trở lên, Bên A có quyền hạ hạn mức công nợ về 0 và yêu cầu thanh toán 100% trước khi giao hàng.
  </p>
  <p>2.5. Sau thời gian đánh giá, Bên A sẽ căn cứ doanh số thực tế, tốc độ thanh toán và mức độ hợp tác để: (i) nâng cấp lên Tier cao hơn; hoặc (ii) giữ nguyên; hoặc (iii) chấm dứt.</p>

  <div class=""section-title"">Điều 3. Sản phẩm và giá</div>
  <p>3.1. Danh mục sản phẩm áp dụng cho đại lý mới được quy định tại Phụ lục 01.</p>
  <p>3.2. Giá bán và khuyến mại theo thông báo của Bên A.</p>

  <div class=""section-title"">Điều 4. Đặt hàng – giao hàng</div>
  <p>4.1. Đặt qua hệ thống; cần thanh toán/đặt cọc trước theo tỷ lệ nêu ở Điều 2.</p>
  <p>4.2. Giao tại kho hoặc điểm nhận do Bên A chỉ định.</p>

  <div class=""section-title"">Điều 5. Thanh toán</div>
  <p>5.1. Thanh toán chuyển khoản.</p>
  <p>5.2. Quá hạn bị phạt theo Điều 2.3.</p>

  <div class=""section-title"">Điều 6. Bảo hành và khiếu nại</div>
  <p>6.1. Theo chính sách hiện hành.</p>
  <p>6.2. Khiếu nại trong 03 ngày.</p>

  <div class=""section-title"">Điều 7. Hình ảnh – bảo mật – đào tạo</div>
  <p>7.1. Bên B phải tham gia các buổi hướng dẫn/bồi dưỡng do Bên A tổ chức.</p>
  <p>7.2. Không tiết lộ thông tin hệ thống.</p>

  <div class=""section-title"">Điều 8. Theo dõi và tái đánh giá</div>
  <p>8.1. Bên A sẽ theo dõi doanh số và thanh toán của Bên B trong 03–06 tháng đầu.</p>
  <p>8.2. Nếu Bên B không đạt yêu cầu, Bên A có quyền chấm dứt hợp đồng mà không phải bồi thường.</p>

  <div class=""section-title"">Điều 9. Thời hạn</div>
  <p>9.1. Hợp đồng có hiệu lực từ ngày <b>{{ contract.effectiveDate }}</b> đến ngày <b>{{ contract.expiryDate }}</b>.</p>

  <div class=""section-title"">Điều 10. Tranh chấp</div>
  <p>10.1. Thương lượng.</p>
  <p>10.2. Không được thì giải quyết tại <b>TP. Hồ Chí Minh</b>.</p>

  <table style=""width:100%; margin-top:28px; table-layout:fixed;"">
    <tr>
      <td style=""width:47%; text-align:left; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN A</div>
        <div><strong>{{ roles.A.representative }}</strong></div>
        <div>{{ roles.A.title }}</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.A.signatureAnchor }}</div>
      </td>
      <td style=""width:6%;"">&nbsp;</td>
      <td style=""width:47%; text-align:right; vertical-align:top;"">
        <div class=""muted"">ĐẠI DIỆN BÊN B</div>
        <div><strong>{{ roles.B.representative }}</strong></div>
        <div>{{ roles.B.title }}</div>
        <div style=""font-size:1pt; color:#fff; opacity:0.01;"">{{ roles.B.signatureAnchor }}</div>
      </td>
    </tr>
  </table>
</body>
</html>
";

            private const string BookingContractHtml = @"<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8"" />
<title>XÁC NHẬN ĐẶT XE – ĐIỀU XE VỀ ĐẠI LÝ</title>
<style>
  @page { size: A4; margin: 10mm 10mm 12mm 10mm; }
  body { background:#fff; font-family: 'Noto Sans', DejaVu Sans, Arial, sans-serif; font-size: 12pt; line-height: 1.45; }
  h1, h2, h3 { text-align: center; margin: 6px 0; }
  .meta { margin-top: 8px; }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 16px; }
  .section-title { margin-top: 12px; font-weight: bold; text-transform: uppercase; }
  table { width: 100%; border-collapse: collapse; margin-top: 8px; }
  th, td { border: 1px solid #444; padding: 6px 8px; vertical-align: top; }
  .right { text-align: right; }
  .muted { color: #777; font-size: 10pt; }
  .note { white-space: pre-line; }
  thead { display: table-header-group; }

  /* Chữ ký: 1 hàng – 2 cột (không khung) */
  .sign-table { width:100%; table-layout:fixed; border-collapse:collapse; margin-top:24px; break-inside:avoid; page-break-inside:avoid; }
  .sign-table tr, .sign-table td { break-inside:avoid; page-break-inside:avoid; }
  .sign-table td { width:50%; padding:0 6px; vertical-align:bottom; }

  /* Ô chứa chữ ký (để neo anchor tuyệt đối) */
  .sign-slot { position:relative; padding:10px 10px 10px 10px; }

  /* Ẩn anchor: chữ trắng, opacity rất nhỏ, cỡ chữ nhỏ — vẫn giữ trong text layer để tool dò */
  .anchor {
    position:absolute; bottom:10px; left:10px;
    font-size:1pt; line-height:1;
    color:#ffffff;          /* sửa đúng mã màu */
    opacity:0.01;           /* tránh 0 để không bị loại khỏi text layer */
    letter-spacing:-0.2pt;
    user-select:none;
  }
</style>
</head>
<body>
  <h2>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h2>
  <h3>Độc lập - Tự do - Hạnh phúc</h3><br>
  <h1>XÁC NHẬN ĐẶT XE – ĐIỀU XE VỀ ĐẠI LÝ</h1><br><br>

  <div class=""meta grid"">
    <div><b>Ngày lập:</b> {{ booking.date }}</div>
    <div><b>Đại lý (Bên đề nghị):</b> {{ dealer.name }}</div>
    <div><b>Địa chỉ Đại lý:</b> {{ dealer.address }}</div>
    <div><b>Liên hệ Đại lý:</b> {{ dealer.contact }}</div>
    <div><b>Hãng/Doanh nghiệp (Bên phê duyệt):</b> {{ company.name }} | MST: {{ company.taxNo }}</div>
    <div><b>Tổng số lượng:</b> {{ booking.total }}</div>
  </div>

  <div class=""section-title"">CHI TIẾT ĐỀ NGHỊ ĐIỀU XE</div>
  <table>
    <thead>
    <tbody>
      {{ booking.rows }}
    </tbody>
  </table>

  <div class=""section-title"">GHI CHÚ</div>
  <div class=""note"">{{ booking.note }}</div>

  <div class=""section-title"">ĐIỀU KHOẢN ÁP DỤNG</div>
  <div class=""note"">
    1) Mục đích: Đại lý đề nghị Hãng phân bổ/điều xe về kho nhận nêu trên để phục vụ bán hàng. <br><br>
    2) Thời hạn & lịch điều xe: Hãng sắp xếp nguồn hàng và lịch vận chuyển theo khả năng cung ứng; thời gian dự kiến có thể thay đổi do tồn kho/ logistics.<br><br>
    3) Trách nhiệm phối hợp: Hai bên phối hợp xác nhận lịch xuất – nhận xe; Đại lý chuẩn bị mặt bằng/kho bãi, nhân sự tiếp nhận và hồ sơ cần thiết theo hướng dẫn của Hãng.
    <br><br>
  </div>

  <table class=""sign-table"">
    <tr>
      <td>
        <div class=""sign-slot"">
          <div><b>ĐẠI DIỆN ĐẠI LÝ (Bên đề nghị)</b></div>
          <div class=""muted"">Ký, ghi rõ họ tên (đóng dấu nếu có)</div>
          <div class=""anchor"">ĐẠI_DIỆN_BÊN_A</div>
        </div>
      </td>
      <td>
        <div class=""sign-slot"">
          <div><b>ĐẠI DIỆN HÃNG (Bên phê duyệt)</b></div>
          <div class=""muted"">Ký, ghi rõ họ tên (đóng dấu)</div>
          <div class=""anchor"">ĐẠI_DIỆN_BÊN_B</div>
        </div>
      </td>
    </tr>
  </table>
</body>
</html>
";


            private const string CustomerDepositContractHtml = @"
<!doctype html>
<html lang=""vi"">
<head>
  <meta charset=""utf-8"" />
  <title>HỢP ĐỒNG ĐẶT CỌC MUA XE – ĐƠN HÀNG #{{ order.no }}</title>
  <style>
    @page { size:A4; margin:10mm 14mm 14mm 14mm; }
    body { background:#fff; font-family:'Noto Sans','DejaVu Sans','Arial',sans-serif; font-size:12pt; line-height:1.5; color:#000; }
    h1,h2,h3,h4 { text-align:center; margin:4px 0; }
    p { margin:4px 0; }
    .muted { color:#777; font-size:10pt; }
    .section-title { margin-top:12px; font-weight:bold; text-transform:uppercase; }
    table { width:100%; border-collapse:collapse; margin-top:8px; }
    th,td { border:1px solid #444; padding:6px 8px; vertical-align:top; }
    thead { display: table-header-group; }
    .grid { display:grid; grid-template-columns:1fr 1fr; gap:6px 16px; }
    .signature-table { width:100%; margin-top:24px; table-layout:fixed; border-collapse:collapse; }
    .signature-table td { width:50%; vertical-align:bottom; padding:0 6px; }
    .sign-slot { position:relative; padding:10px; }
    .anchor { position:absolute; bottom:10px; left:10px; font-size:1pt; line-height:1; color:#ffffff; opacity:0.01; letter-spacing:-0.2pt; user-select:none; }
  </style>
</head>
<body>
  <h2>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h2>
  <h4>Độc lập - Tự do - Hạnh phúc</h4>
  <h2>HỢP ĐỒNG ĐẶT CỌC MUA XE</h2>
  <p style=""text-align:center""><b>Mã đơn hàng: #{{ order.no }} • Ngày lập: {{ order.date }}</b></p>

  <div class=""section-title"">Các bên</div>
  <div class=""grid"">
    <div>
      <p><b>BÊN A (ĐẠI LÝ / ĐƠN VỊ BÁN):</b></p>
      <p>- Tên đơn vị: {{ dealer.name }}</p>
      <p>- Địa chỉ: {{ dealer.address }}</p>
      <p>- MST: {{ dealer.taxNo }}</p>
      <p>- Điện thoại: {{ dealer.phone }}</p>
      <p>- Email: {{ dealer.email }}</p>
      <p>- Đại diện: {{ roles.A.representative }} ({{ roles.A.title }})</p>
      <p>- Tài khoản: {{ dealer.bankAccount }} tại {{ dealer.bankName }}</p>
    </div>
    <div>
      <p><b>BÊN B (KHÁCH HÀNG):</b></p>
      <p>- Họ và tên: {{ customer.fullName }}</p>
      <p>- Số ĐT: {{ customer.phone }}</p>
      <p>- Email: {{ customer.email }}</p>
      <p>- CCCD/Hộ chiếu: {{ customer.idNo }}</p>
      <p>- Địa chỉ: {{ customer.address }}</p>
    </div>
  </div>

  <div class=""section-title"">Điều 1. Thông tin đơn hàng và khoản đặt cọc</div>
  <div class=""grid"">
    <div><b>Tổng giá trị đơn hàng (dự kiến):</b> {{ money.orderTotal }}</div>
    <div><b>Số tiền đặt cọc:</b> {{ money.deposit }}</div>
    <div><b>Số tiền còn lại:</b> {{ money.remaining }}</div>
    <div><b>Phương thức đặt cọc:</b> {{ order.paymentMethod }}</div>
  </div>
  <p><i>Ghi chú:</i> Giá trị đơn hàng có thể thay đổi theo cấu hình/màu sắc/thời điểm giao nhận. Mọi điều chỉnh giá sẽ được hai bên xác nhận bằng phụ lục/phiếu điều chỉnh đính kèm.</p>

  <div class=""section-title"">Điều 2. Danh mục xe đặt mua</div>
  <table>
    <thead>
      <tr>
        <th>#</th>
        <th>Mẫu xe / Phiên bản</th>
        <th>Màu</th>
        <th>Số lượng</th>
        <th>Ghi chú</th>
      </tr>
    </thead>
    <tbody>
      {{ order.vehicleRows }}
    </tbody>
  </table>

  <div class=""section-title"">Điều 3. Thời hạn giữ hàng & giao hàng</div>
  <p>3.1. Bên A giữ hàng tương ứng số lượng trong Điều 2 tối đa <b>{{ policy.holdDays }}</b> ngày kể từ ngày đặt cọc, trừ khi hai bên có thỏa thuận khác bằng văn bản.</p>
  <p>3.2. Dự kiến giao/nhận tại: {{ logistics.place }} • Thời gian dự kiến: {{ logistics.eta }} (có thể điều chỉnh theo tồn kho & vận chuyển).</p>

  <div class=""section-title"">Điều 4. Thanh toán</div>
  <p>4.1. Bên B thanh toán phần còn lại <b>trước hoặc tại thời điểm nhận xe</b> theo một trong các hình thức: chuyển khoản, thẻ, tiền mặt (tuân thủ quy định pháp luật về hạn mức tiền mặt).</p>
  <p>4.2. Quá hạn thanh toán quá <b>{{ policy.lateDays }}</b> ngày so với lịch dự kiến, Bên A có quyền: (i) chấm dứt giữ hàng; và/hoặc (ii) áp dụng phí lưu kho/chi phí phát sinh thực tế (nếu có).</p>

  <div class=""section-title"">Điều 5. Chính sách đặt cọc & hoàn hủy</div>
  <p>5.1. Khoản đặt cọc mang tính <b>bảo đảm nghĩa vụ mua</b>; nếu Bên B đơn phương hủy không do lỗi của Bên A, khoản cọc <b>không hoàn lại</b>.</p>
  <p>5.2. Trường hợp Bên A không thể cung ứng xe đúng cấu hình đã xác nhận trong thời hạn giữ hàng (Điều 3.1) và Bên B không đồng ý phương án thay thế, Bên A hoàn trả toàn bộ tiền cọc trong vòng <b>07 ngày làm việc</b>.</p>
  <p>5.3. Nếu hai bên thống nhất thay đổi cấu hình/phiên bản/màu, hợp đồng này vẫn có hiệu lực và được điều chỉnh bằng phụ lục.</p>

  <div class=""section-title"">Điều 6. Bảo hành, chất lượng & trách nhiệm</div>
  <p>6.1. Xe được bảo hành theo chính sách bảo hành hiện hành của hãng/nhà sản xuất.</p>
  <p>6.2. Khi nhận xe, Bên B có trách nhiệm kiểm tra ngoại quan, phụ kiện, chứng từ; mọi khiếu nại sai khác phải thông báo trong vòng <b>03 ngày</b> để được hỗ trợ.</p>

  <div class=""section-title"">Điều 7. Bảo mật & dữ liệu</div>
  <p>7.1. Bên B đồng ý để Bên A sử dụng thông tin liên hệ nhằm phục vụ bảo hành, nhắc bảo dưỡng, thông báo chương trình (có thể hủy đăng ký bất kỳ lúc nào).</p>

  <div class=""section-title"">Điều 8. Hiệu lực, chấm dứt & giải quyết tranh chấp</div>
  <p>8.1. Hợp đồng có hiệu lực từ ngày ký đến khi hai bên hoàn tất nghĩa vụ, hoặc được thay thế bởi hợp đồng mua bán chính thức khi giao xe.</p>
  <p>8.2. Tranh chấp ưu tiên thương lượng; không thành sẽ đưa ra cơ quan có thẩm quyền tại <b>TP. Hồ Chí Minh</b> theo pháp luật Việt Nam.</p>

  <table class=""signature-table"">
    <tr>
      <td>
        <div class=""sign-slot"">
          <div class=""muted""><b>ĐẠI DIỆN BÊN A (ĐẠI LÝ)</b></div>
          <div><b>{{ roles.A.representative }}</b></div>
          <div>{{ roles.A.title }}</div>
          <div class=""muted"">Ký, ghi rõ họ tên, đóng dấu</div>
          <div class=""anchor"">{{ roles.A.signatureAnchor }}</div>
        </div>
      </td>
      <td>
        <div class=""sign-slot"">
          <div class=""muted""><b>ĐẠI DIỆN BÊN B (KHÁCH HÀNG)</b></div>
          <div><b>{{ customer.fullName }}</b></div>
          <div class=""muted"">Ký, ghi rõ họ tên</div>
          <div class=""anchor"">{{ roles.B.signatureAnchor }}</div>
        </div>
      </td>
    </tr>
  </table>

  <div class=""muted"">Trang {{ page }} / {{ pages }}</div>
</body>
</html>";
            private const string CustomerPayFullEContractHtml = @"

<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8""/>
<title>Phiếu xác nhận thanh toán đầy đủ</title>
<style>
  *{box-sizing:border-box} html,body{font-family:DejaVu Sans, Arial, sans-serif;font-size:12px;color:#111}
  .wrap{max-width:820px;margin:0 auto;padding:28px}
  .tc{text-align:center} .tr{text-align:right} .b{font-weight:700} .muted{color:#666}
  .hr{height:1px;background:#000;margin:6px auto 0;width:240px}
  .sep{margin:6px 0 14px;text-align:center;color:#000} .sep span{display:inline-block;padding:0 10px}
  .head-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin:10px 0 8px}
  .box{border:1px solid #ddd;border-radius:8px;padding:12px}
  .box h3{margin:0 0 8px;font-size:13px;text-transform:uppercase;letter-spacing:.2px}
  .kv p{margin:3px 0}
  table{width:100%;border-collapse:collapse;margin:10px 0}
  th,td{border:1px solid #ddd;padding:8px;vertical-align:top}
  th{background:#f6f6f6;font-weight:700}
  .right{text-align:right}
  .sig{display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-top:22px}
  .sigbox{border:1px dashed #aaa;border-radius:8px;padding:12px;min-height:150px;position:relative}
  .sigttl{font-weight:700;margin-bottom:6px}
  .anchor{position:absolute;bottom:8px;left:12px;font-size:10px;color:#999}
  .small{font-size:11px;color:#555}
</style>
</head>
<body>
<div class=""wrap"">

  <!-- TIÊU NGỮ QUỐC GIA -->
  <div class=""tc"">
    <div class=""b"" style=""font-size:14px;letter-spacing:.5px"">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
    <div style=""margin-top:2px"">Độc lập • Tự do • Hạnh phúc</div>
    <div class=""hr""></div>
  </div>

  <!-- THÔNG TIN DOANH NGHIỆP/ĐƠN VỊ PHÁT HÀNH -->
  <div class=""head-grid"" style=""margin-top:12px"">
    <div>
      <div class=""b"">ĐƠN VỊ: {{dealer.name}}</div>
      <div>Địa chỉ: {{dealer.address}}</div>
      <div>MST: {{dealer.taxNo}}</div>
      <div>Điện thoại: {{dealer.phone}} &nbsp;|&nbsp; Email: {{dealer.email}}</div>
      <div>TK: {{dealer.bankAccount}} &nbsp;|&nbsp; Ngân hàng: {{dealer.bankName}}</div>
    </div>
    <div class=""tr"">
      <div>Số: <b>{{order.no}}</b></div>
      <div>Ngày lập: <b>{{order.date}}</b></div>
      <div>Hình thức thanh toán: <b>{{order.paymentMethod}}</b></div>
    </div>
  </div>

  <!-- TIÊU ĐỀ VĂN BẢN -->
  <div class=""tc"" style=""margin-top:6px"">
    <div class=""b"" style=""font-size:18px;text-transform:uppercase"">PHIẾU XÁC NHẬN THANH TOÁN ĐẦY ĐỦ</div>
    <div class=""muted"">Căn cứ đơn đặt hàng số: <b>{{order.no}}</b></div>
  </div>

  <!-- CĂN CỨ & THỜI ĐỊA ĐIỂM -->
  <div style=""margin-top:12px"">
    <div><i>Căn cứ</i> Bộ luật Dân sự 2015 và các quy định pháp luật có liên quan;</div>
    <div><i>Căn cứ</i> nhu cầu mua bán giữa các bên;</div>
    <div style=""margin-top:6px"">Hôm nay, ngày <b>{{order.date}}</b>, tại <b>{{dealer.address}}</b>, chúng tôi gồm có:</div>
  </div>

  <!-- THÔNG TIN CÁC BÊN -->
  <div class=""head-grid"" style=""margin-top:10px"">
    <div class=""box"">
      <h3>Bên A (Đơn vị bán)</h3>
      <div class=""kv"">
        <p>Tên: <b>{{dealer.name}}</b></p>
        <p>Địa chỉ: {{dealer.address}}</p>
        <p>MST: {{dealer.taxNo}}</p>
        <p>Đại diện: <b>{{roles.A.representative}}</b> — Chức vụ: {{roles.A.title}}</p>
        <p>Điện thoại: {{dealer.phone}} — Email: {{dealer.email}}</p>
      </div>
    </div>
    <div class=""box"">
      <h3>Bên B (Khách hàng)</h3>
      <div class=""kv"">
        <p>Họ tên: <b>{{customer.fullName}}</b></p>
        <p>CCCD/CMND: {{customer.idNo}}</p>
        <p>Địa chỉ: {{customer.address}}</p>
        <p>Điện thoại: {{customer.phone}} — Email: {{customer.email}}</p>
      </div>
    </div>
  </div>

  <!-- NỘI DUNG XÁC NHẬN -->
  <div class=""box"" style=""margin-top:12px"">
    <h3>Nội dung xác nhận</h3>
    <p>Bên A xác nhận đã <b>nhận đủ</b> tiền thanh toán cho đơn hàng số <b>{{order.no}}</b> với giá trị:</p>
    <table>
      <tbody>
        <tr><td>Tổng giá trị đơn hàng</td><td class=""right""><b>{{money.orderTotal}}</b></td></tr>
        <tr><td>Số tiền Bên B đã thanh toán (đợt này)</td><td class=""right"">{{money.deposit}}</td></tr>
        <tr><td>Số tiền còn lại sau xác nhận</td><td class=""right""><b>{{money.remaining}}</b></td></tr>
      </tbody>
    </table>
    <p>Chi tiết xe theo đơn hàng:</p>
    <table>
      <thead>
        <tr>
          <th style=""width:80px"" class=""right"">STT</th>
          <th>Model – Version</th>
          <th style=""width:160px"">Màu</th>
          <th style=""width:120px"" class=""right"">Số lượng</th>
        </tr>
      </thead>
      <tbody>
        {{order.vehicleRows}}
      </tbody>
    </table>
    <div class=""head-grid"" style=""margin-top:6px"">
      <div>
        <div class=""b"">Logistics</div>
        <div>Nơi bàn giao: <b>{{logistics.place}}</b></div>
        <div>Dự kiến: <b>{{logistics.eta}}</b></div>
      </div>
      <div>
        <div class=""b"">Lưu ý chính sách</div>
        <div>Giữ hàng tối đa: <b>{{policy.holdDays}}</b> ngày; quá hạn <b>{{policy.lateDays}}</b> ngày có thể phát sinh chi phí lưu kho/điều phối.</div>
      </div>
    </div>
  </div>

  <!-- ĐIỀU KHOẢN -->
  <div class=""box"" style=""margin-top:10px"">
    <h3>Điều khoản và cam kết</h3>
    <ol class=""small"">
      <li>Phiếu xác nhận này là một phần không tách rời hồ sơ mua bán/đơn đặt hàng số <b>{{order.no}}</b>.</li>
      <li>Bên A sẽ sắp xếp bàn giao theo tiến độ logistics nêu trên.</li>
      <li>Hai bên cam kết cung cấp thông tin trung thực và phối hợp đầy đủ trong quá trình giao nhận.</li>
    </ol>
  </div>

  <!-- CHỮ KÝ -->
  <div class=""sig"">
    <div class=""sigbox tc"">
      <div class=""sigttl"">ĐẠI DIỆN BÊN A</div>
      <div><i>(Ký, ghi rõ họ tên, đóng dấu)</i></div>
      <div class=""anchor"">ĐẠI_DIỆN_BÊN_A</div>
    </div>
    <div class=""sigbox tc"">
      <div class=""sigttl"">ĐẠI DIỆN BÊN B</div>
      <div><b>{{customer.fullName}}</b></div>
      <div><i>(Ký, ghi rõ họ tên)</i></div>
      <div class=""anchor"">ĐẠI_DIỆN_BÊN_B</div>
    </div>
  </div>

</div>
</body>
</html>
";

            private const string CustomerPayRemainderEContractHtml = @"
<!doctype html>
<html lang=""vi"">
<head>
<meta charset=""utf-8""/>
<title>Phiếu xác nhận thanh toán phần còn lại</title>
<style>
  *{box-sizing:border-box} html,body{font-family:DejaVu Sans, Arial, sans-serif;font-size:12px;color:#111}
  .wrap{max-width:820px;margin:0 auto;padding:28px}
  .tc{text-align:center} .tr{text-align:right} .b{font-weight:700} .muted{color:#666}
  .hr{height:1px;background:#000;margin:6px auto 0;width:240px}
  .head-grid{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin:10px 0 8px}
  .box{border:1px solid #ddd;border-radius:8px;padding:12px}
  .box h3{margin:0 0 8px;font-size:13px;text-transform:uppercase;letter-spacing:.2px}
  .kv p{margin:3px 0}
  table{width:100%;border-collapse:collapse;margin:10px 0}
  th,td{border:1px solid #ddd;padding:8px;vertical-align:top}
  th{background:#f6f6f6;font-weight:700}
  .right{text-align:right}
  .sig{display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-top:22px}
  .sigbox{border:1px dashed #aaa;border-radius:8px;padding:12px;min-height:150px;position:relative}
  .sigttl{font-weight:700;margin-bottom:6px}
  .anchor{position:absolute;bottom:8px;left:12px;font-size:10px;color:#999}
  .small{font-size:11px;color:#555}
</style>
</head>
<body>
<div class=""wrap"">

  <!-- TIÊU NGỮ QUỐC GIA -->
  <div class=""tc"">
    <div class=""b"" style=""font-size:14px;letter-spacing:.5px"">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
    <div style=""margin-top:2px"">Độc lập • Tự do • Hạnh phúc</div>
    <div class=""hr""></div>
  </div>

  <!-- THÔNG TIN DOANH NGHIỆP/ĐƠN VỊ PHÁT HÀNH -->
  <div class=""head-grid"" style=""margin-top:12px"">
    <div>
      <div class=""b"">ĐƠN VỊ: {{dealer.name}}</div>
      <div>Địa chỉ: {{dealer.address}}</div>
      <div>MST: {{dealer.taxNo}}</div>
      <div>Điện thoại: {{dealer.phone}} &nbsp;|&nbsp; Email: {{dealer.email}}</div>
      <div>TK: {{dealer.bankAccount}} &nbsp;|&nbsp; Ngân hàng: {{dealer.bankName}}</div>
    </div>
    <div class=""tr"">
      <div>Số: <b>{{order.no}}</b></div>
      <div>Ngày lập: <b>{{order.date}}</b></div>
      <div>Hình thức thanh toán: <b>{{order.paymentMethod}}</b></div>
    </div>
  </div>

  <!-- TIÊU ĐỀ VĂN BẢN -->
  <div class=""tc"" style=""margin-top:6px"">
    <div class=""b"" style=""font-size:18px;text-transform:uppercase"">PHIẾU XÁC NHẬN THANH TOÁN PHẦN CÒN LẠI</div>
    <div class=""muted"">Căn cứ đơn đặt hàng số: <b>{{order.no}}</b></div>
  </div>

  <!-- CĂN CỨ & THỜI ĐỊA ĐIỂM -->
  <div style=""margin-top:12px"">
    <div><i>Căn cứ</i> Bộ luật Dân sự 2015 và các quy định pháp luật có liên quan;</div>
    <div><i>Căn cứ</i> các thỏa thuận thanh toán giữa các bên;</div>
    <div style=""margin-top:6px"">Hôm nay, ngày <b>{{order.date}}</b>, tại <b>{{dealer.address}}</b>, chúng tôi gồm có:</div>
  </div>

  <!-- THÔNG TIN CÁC BÊN -->
  <div class=""head-grid"" style=""margin-top:10px"">
    <div class=""box"">
      <h3>Bên A (Đơn vị bán)</h3>
      <div class=""kv"">
        <p>Tên: <b>{{dealer.name}}</b></p>
        <p>Địa chỉ: {{dealer.address}}</p>
        <p>MST: {{dealer.taxNo}}</p>
        <p>Đại diện: <b>{{roles.A.representative}}</b> — Chức vụ: {{roles.A.title}}</p>
        <p>Điện thoại: {{dealer.phone}} — Email: {{dealer.email}}</p>
      </div>
    </div>
    <div class=""box"">
      <h3>Bên B (Khách hàng)</h3>
      <div class=""kv"">
        <p>Họ tên: <b>{{customer.fullName}}</b></p>
        <p>CCCD/CMND: {{customer.idNo}}</p>
        <p>Địa chỉ: {{customer.address}}</p>
        <p>Điện thoại: {{customer.phone}} — Email: {{customer.email}}</p>
      </div>
    </div>
  </div>

  <!-- NỘI DUNG XÁC NHẬN -->
  <div class=""box"" style=""margin-top:12px"">
    <h3>Nội dung xác nhận</h3>
    <p>Bên A xác nhận đã <b>nhận thanh toán phần còn lại</b> cho đơn hàng số <b>{{order.no}}</b> với giá trị như sau:</p>
    <table>
      <tbody>
        <tr><td>Tổng giá trị đơn hàng</td><td class=""right""><b>{{money.orderTotal}}</b></td></tr>
        <tr><td>Số tiền thanh toán trong kỳ (phần còn lại)</td><td class=""right"">{{money.deposit}}</td></tr>
        <tr><td>Số tiền còn lại sau xác nhận</td><td class=""right""><b>{{money.remaining}}</b></td></tr>
      </tbody>
    </table>
    <p>Chi tiết xe theo đơn hàng:</p>
    <table>
      <thead>
        <tr>
          <th style=""width:80px"" class=""right"">STT</th>
          <th>Model – Version</th>
          <th style=""width:160px"">Màu</th>
          <th style=""width:120px"" class=""right"">Số lượng</th>
        </tr>
      </thead>
      <tbody>
        {{order.vehicleRows}}
      </tbody>
    </table>
    <div class=""head-grid"" style=""margin-top:6px"">
      <div>
        <div class=""b"">Logistics</div>
        <div>Nơi bàn giao: <b>{{logistics.place}}</b></div>
        <div>Dự kiến: <b>{{logistics.eta}}</b></div>
      </div>
      <div>
        <div class=""b"">Lưu ý chính sách</div>
        <div>Giữ hàng tối đa: <b>{{policy.holdDays}}</b> ngày; quá hạn <b>{{policy.lateDays}}</b> ngày có thể phát sinh chi phí lưu kho/điều phối.</div>
      </div>
    </div>
  </div>

  <!-- ĐIỀU KHOẢN -->
  <div class=""box"" style=""margin-top:10px"">
    <h3>Điều khoản và cam kết</h3>
    <ol class=""small"">
      <li>Phiếu xác nhận là căn cứ bổ sung cho hồ sơ thanh toán của đơn đặt hàng số <b>{{order.no}}</b>.</li>
      <li>Sau khi hoàn tất, Bên A thực hiện bàn giao theo tiến độ logistics nêu trên.</li>
      <li>Hai bên chịu trách nhiệm về tính chính xác của thông tin và thực hiện đầy đủ nghĩa vụ liên quan.</li>
    </ol>
  </div>

  <!-- CHỮ KÝ -->
  <div class=""sig"">
    <div class=""sigbox tc"">
      <div class=""sigttl"">ĐẠI DIỆN BÊN A</div>
      <div><i>(Ký, ghi rõ họ tên, đóng dấu)</i></div>
      <div class=""anchor"">ĐẠI_DIỆN_BÊN_A</div>
    </div>
    <div class=""sigbox tc"">
      <div class=""sigttl"">ĐẠI DIỆN BÊN B</div>
      <div><b>{{customer.fullName}}</b></div>
      <div><i>(Ký, ghi rõ họ tên)</i></div>
      <div class=""anchor"">ĐẠI_DIỆN_BÊN_B</div>
    </div>
  </div>

  <div class=""small"" style=""margin-top:10px"">
    Phiếu được lập thành 02 (hai) bản có giá trị pháp lý như nhau, mỗi bên giữ 01 (một) bản.
  </div>

</div>
</body>
</html>

";

            public static void SeedDealerEContract(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<EContractTemplate>().HasData(
                    new
                    {
                        Id = DealerTier1TemplateId,
                        Code = "DEALER_TIER_1",
                        Name = "Hợp đồng Đại lý – Tier 1",
                        ContentHtml = DealerTier1Html,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = DealerTier2TemplateId,
                        Code = "DEALER_TIER_2",
                        Name = "Hợp đồng Đại lý – Tier 2",
                        ContentHtml = DealerTier2Html,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = DealerTier3TemplateId,
                        Code = "DEALER_TIER_3",
                        Name = "Hợp đồng Đại lý – Tier 3",
                        ContentHtml = DealerTier3Html,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = DealerTier4TemplateId,
                        Code = "DEALER_TIER_4",
                        Name = "Hợp đồng Đại lý – Tier 4",
                        ContentHtml = DealerTier4Html,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = DealerTier5TemplateId,
                        Code = "DEALER_TIER_5",
                        Name = "Hợp đồng Đại lý – Tier 5",
                        ContentHtml = DealerTier5Html,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = BookingTemplateId,
                        Code = "BOOKINGECONTRACT",
                        Name = "Xác nhận đặt xe – điều xe về đại lý",
                        ContentHtml = BookingContractHtml,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = CustomerDepositTemplateId,
                        Code = "CUSTOMER_DEPOSIT_CONTRACT",
                        Name = "Hợp đồng đặt cọc mua xe (Khách hàng)",
                        ContentHtml = CustomerDepositContractHtml,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = CustomerFullPaymentTemplateId,
                        Code = "CUSTOMER_PAY_FULL_E_CONTRACT",
                        Name = "Phiếu xác nhận thanh toán đầy đủ (Khách hàng)",
                        ContentHtml = CustomerPayFullEContractHtml,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    },
                    new
                    {
                        Id = CustomerPayRemainderTemplateId,
                        Code = "CUSTOMER_PAY_REMAINDER_E_CONTRACT",
                        Name = "Phiếu xác nhận thanh toán phần còn lại (Khách hàng)",
                        ContentHtml = CustomerPayRemainderEContractHtml,
                        CreatedAt = CreatedAtUtc,
                        IsDeleted = false
                    }
                );
            }
        }
    }
}
