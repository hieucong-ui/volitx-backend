using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Entities;
using Voltix.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Seeders
{
    public static class EmailSeeder
    {
        public static void SeedEmailTemplate(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailTemplate>().HasData(
   new
   {
       Id = Guid.Parse("8f6d9d5d-2f91-4df8-9f70-ec5a09c4f111"),
       TemplateName = "ResetPassword",
       SenderName = "EVSystem Support",
       SenderEmail = "hoangtuzami@gmail.com",
       Category = "Authentication",
       SubjectLine = "Yêu cầu đặt lại mật khẩu",
       PreHeaderText = "Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản",
       PersonalizationTags = "{ResetLink}",
       BodyContent = @"

<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Hệ thống xe điện ABX - Đặt lại mật khẩu</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #dc3545; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>Electric Vehicle Management System</h2>
                            <p style='margin: 0;'>Đặt lại mật khẩu</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Xin chào,</p>
                            <p style='font-size: 16px;'>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn tại <strong>Hệ thống xe điện ABX</strong>.</p>
                            <p style='font-size: 16px;'>Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{ResetLink}' style='background-color: #dc3545; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Đặt lại mật khẩu</a>
                            </div>
                            <p style='font-size: 14px; color: #666;'>Nếu bạn không yêu cầu thao tác này, vui lòng bỏ qua email này. Tài khoản của bạn vẫn an toàn.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'>Đội ngũ <strong>EVSytem Team</strong></p>
                            <p style='margin: 0;'>Mọi thắc mắc xin liên hệ: <a href='https://electricvehiclesystem.click' style='color: #dc3545; text-decoration: none;'>https://electricvehiclesystem.click</a></p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
       FooterContent = "SWP391 Support Team",
       CallToAction = "<a href=\"{ResetLink}\">Đặt lại mật khẩu</a>",
       Language = "Vietnamese",
       RecipientType = "Customer",
       CreatedBy = "System",
       CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
       UpdatedBy = "System",
       UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
       Status = EmailStatus.Active
   });

            modelBuilder.Entity<EmailTemplate>().HasData(
              new
              {
                  Id = Guid.Parse("c1b4c5a2-0e9b-4f11-aa2f-3cf19b0f9e0b"),
                  TemplateName = "NotifyContractPdf",
                  SenderName = "EVSystem Support",
                  SenderEmail = "hoangtuzami@gmail.com",
                  Category = "Contract",
                  SubjectLine = "Thông báo hợp đồng",
                  PreHeaderText = "Hợp đồng của bạn đã sẵn sàng để xem/tải (PDF).",
                  // Liệt kê các placeholder bạn dự định thay thế
                  PersonalizationTags = "{FullName},{ContractSubject},{DownloadLink},{Company},{SupportEmail}",
                  BodyContent = @"

<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Thông báo hợp đồng</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #0ea5e9; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>EVSystem Support</h2>
                            <p style='margin: 0;'>Thông báo hợp đồng</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Xin chào <strong>{FullName}</strong>,</p>
                            <p style='font-size: 16px;'>Hợp đồng <strong>{ContractSubject}</strong> của bạn đã sẵn sàng.</p>
                            <p style='font-size: 16px;'>Bạn có thể xem hoặc tải về bản PDF bằng cách nhấn nút bên dưới:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{DownloadLink}' style='background-color: #0ea5e9; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Xem/Tải hợp đồng (PDF)</a>
                            </div>
                            <p style='font-size: 14px; color: #666;'>Nếu nút không hiển thị, bạn có thể mở liên kết: <a href='{DownloadLink}' style='color: #0ea5e9; text-decoration: none;'>{DownloadLink}</a></p>
                            <p style='font-size: 14px; color: #666;'>Nếu cần hỗ trợ, vui lòng liên hệ: <a href='mailto:{SupportEmail}' style='color: #0ea5e9; text-decoration: none;'>{SupportEmail}</a>.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'><strong>{Company}</strong> - Đội ngũ hỗ trợ EVSystem Support</p>
                            <p style='margin: 0;'>Đây là email tự động, vui lòng không phản hồi.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
                  FooterContent = "EVSystem Support",
                  CallToAction = "<a href=\"{DownloadLink}\">Xem/Tải hợp đồng (PDF)</a>",
                  Language = "Vietnamese",
                  RecipientType = "Customer",
                  CreatedBy = "System",
                  CreatedAt = new DateTime(2025, 10, 09, 0, 0, 0, DateTimeKind.Utc),
                  UpdatedBy = "System",
                  UpdatedAt = new DateTime(2025, 10, 09, 0, 0, 0, DateTimeKind.Utc),
                  Status = EmailStatus.Active
              });
            modelBuilder.Entity<EmailTemplate>().HasData(
    new
    {
        Id = Guid.Parse("b7f0a7cb-0d7c-4f1e-9c8e-9c1d4a2b7f11"),
        TemplateName = "DealerWelcome",
        SenderName = "EVSystem Support",
        SenderEmail = "hoangtuzami@gmail.com",
        Category = "Dealer",
        SubjectLine = "Chúc mừng! Tài khoản đại lý của bạn đã được kích hoạt",
        PreHeaderText = "Thông tin đăng nhập và liên kết truy cập hệ thống",
        PersonalizationTags = "{FullName},{DealerName},{UserName},{Password},{LoginUrl},{Company},{SupportEmail}",
        BodyContent = @"

<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Chào mừng Đại lý</title>
</head>
<body style='font-family: Segoe UI, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                    <tr>
                        <td style='background-color: #16a34a; padding: 20px; text-align: center; color: white;'>
                            <h2 style='margin: 0;'>{Company}</h2>
                            <p style='margin: 0;'>Chào mừng Đại lý mới</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 30px;'>
                            <p style='font-size: 16px;'>Xin chào <strong>{FullName}</strong>,</p>
                            <p style='font-size: 16px;'>Chúc mừng bạn đã trở thành đại lý của công ty chúng tôi.</p>
                            <p style='font-size: 16px;'>Dưới đây là thông tin đăng nhập của bạn:</p>
                            <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:6px; margin: 14px 0;'>
                                <tr>
                                    <td style='padding: 12px 16px; font-size: 14px;'>Tên đăng nhập:</td>
                                    <td style='padding: 12px 16px; font-size: 14px;'><strong>{UserName}</strong></td>
                                </tr>
                                <tr style='background-color:#ffffff;'>
                                    <td style='padding: 12px 16px; font-size: 14px;'>Mật khẩu:</td>
                                    <td style='padding: 12px 16px; font-size: 14px;'><strong>{Password}</strong></td>
                                </tr>
                                <tr>
                                    <td style='padding: 12px 16px; font-size: 14px;'>Đường dẫn đăng nhập:</td>
                                    <td style='padding: 12px 16px; font-size: 14px;'><a href='{LoginUrl}' style='color:#16a34a; text-decoration:none;'>{LoginUrl}</a></td>
                                </tr>
                            </table>

                            <div style='text-align: center; margin: 26px 0;'>
                                <a href='{LoginUrl}' style='background-color: #16a34a; color: white; padding: 14px 28px; text-decoration: none; font-size: 16px; border-radius: 6px; display: inline-block;'>Đăng nhập ngay</a>
                            </div>

                            <p style='font-size: 14px; color: #666;'>
                                Vì lý do bảo mật, vui lòng <strong>đổi mật khẩu ngay</strong> sau lần đăng nhập đầu tiên (Mục: Hồ sơ/Tài khoản &rarr; Đổi mật khẩu).
                            </p>
                            <p style='font-size: 14px; color: #666;'>
                                Nếu cần hỗ trợ, vui lòng liên hệ: <a href='mailto:{SupportEmail}' style='color:#16a34a; text-decoration:none;'>{SupportEmail}</a>.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style='background-color: #f1f1f1; padding: 20px; text-align: center; font-size: 13px; color: #999;'>
                            <p style='margin: 0;'>Trân trọng,</p>
                            <p style='margin: 0;'><strong>{Company}</strong> - Đội ngũ hỗ trợ EVSystem Support</p>
                            <p style='margin: 0;'>Đây là email tự động, vui lòng không phản hồi.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>",
        FooterContent = "EVSystem Support",
        CallToAction = "<a href=\"{LoginUrl}\">Đăng nhập ngay</a>",
        Language = "Vietnamese",
        RecipientType = "Customer",
        CreatedBy = "System",
        CreatedAt = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc),
        UpdatedBy = "System",
        UpdatedAt = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc),
        Status = EmailStatus.Active
    });

            modelBuilder.Entity<EmailTemplate>().HasData(
new
{
    Id = Guid.Parse("A89C8D8B-BA15-4E60-A74E-8A3B9C2B7A77"),
    TemplateName = "SendEmployeeCredentials",
    SenderName = "EVSystem Support",
    SenderEmail = "hoangtuzami@gmail.com",
    Category = "Onboarding",
    SubjectLine = "Chào mừng bạn đến với Hệ thống xe điện ABX – Thông tin tài khoản đăng nhập",
    PreHeaderText = "Tài khoản nhân viên đã được tạo. Đăng nhập ngay để kích hoạt và thay đổi mật khẩu.",
    PersonalizationTags = "{EmployeeName},{Username},{TempPassword},{LoginLink}",
    BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Tài khoản nhân viên - EVSystem</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background-color: #f5f6fa; color: #333; }
        .container { width: 100%; background-color: #f5f6fa; padding: 40px 0; }
        .email-card { width: 600px; background: #fff; border-radius: 12px; margin: auto; overflow: hidden; box-shadow: 0 6px 18px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(90deg, #dc3545, #b02a37); color: #fff; text-align: center; padding: 24px 16px; }
        .header h1 { margin: 0; font-size: 24px; letter-spacing: 0.5px; }
        .content { padding: 36px 40px; line-height: 1.7; }
        .content p { font-size: 15px; margin-bottom: 12px; }
        .credentials { background-color: #f8f9fa; border: 1px solid #eee; border-radius: 8px; padding: 16px 20px; margin: 20px 0; }
        .credentials td { padding: 8px 0; font-size: 15px; }
        .btn { display: inline-block; padding: 14px 28px; background: #dc3545; color: #fff !important; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: 600; transition: all 0.3s ease; }
        .btn:hover { background: #b02a37; }
        .note { font-size: 13px; color: #777; margin-top: 24px; line-height: 1.6; }
        .footer { background-color: #f1f1f1; text-align: center; padding: 18px; font-size: 13px; color: #888; }
        .footer a { color: #dc3545; text-decoration: none; }
        @media (max-width: 620px) {
            .email-card { width: 95%; }
            .content { padding: 24px; }
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='email-card'>
            <div class='header'>
                <h1>Electric Vehicle Management System</h1>
                <p>Chào mừng bạn gia nhập đội ngũ EVSystem</p>
            </div>
            <div class='content'>
                <p>Xin chào <strong>{EmployeeName}</strong>,</p>
                <p>Tài khoản nhân viên của bạn tại <strong>Hệ thống xe điện ABX</strong> đã được khởi tạo thành công.</p>
                <p>Vui lòng sử dụng thông tin dưới đây để đăng nhập lần đầu:</p>
                <table class='credentials' width='100%'>
                    <tr>
                        <td><strong>Tên đăng nhập:</strong></td>
                        <td>{Username}</td>
                    </tr>
                    <tr>
                        <td><strong>Mật khẩu tạm thời:</strong></td>
                        <td>{TempPassword}</td>
                    </tr>
                </table>
                <p style='text-align: center; margin: 30px 0;'>
                    <a href='{LoginLink}' class='btn'>Đăng nhập ngay</a>
                </p>
                <p class='note'>
                    🔒 <strong>Lưu ý bảo mật:</strong><br>
                    - Vui lòng đổi mật khẩu ngay trong lần đăng nhập đầu tiên.<br>
                    - Không chia sẻ thông tin tài khoản với người khác.<br>
                    - Nếu bạn không yêu cầu tạo tài khoản này, vui lòng liên hệ bộ phận quản trị hệ thống.
                </p>
            </div>
            <div class='footer'>
                <p>Trân trọng,<br><strong>EVSystem Team</strong></p>
                <p>Website: <a href='https://electricvehiclesystem.click'>electricvehiclesystem.click</a></p>
            </div>
        </div>
    </div>
</body>
</html>",
    FooterContent = "EVSystem Team – Electric Vehicle Management System",
    CallToAction = "<a href=\"{LoginLink}\" class=\"btn\">Đăng nhập ngay</a>",
    Language = "Vietnamese",
    RecipientType = "Employee",
    CreatedBy = "System",
    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    UpdatedBy = "System",
    UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    Status = EmailStatus.Active
});


            modelBuilder.Entity<EmailTemplate>().HasData(
new
{
    Id = Guid.Parse("9B2A78CF-5E46-4E56-AC42-91E4CFD012AB"),
    TemplateName = "SendDealerStaffCredentials_Pro",
    SenderName = "EVSystem Support",
    SenderEmail = "hoangtuzami@gmail.com",
    Category = "Onboarding",
    SubjectLine = "[EVSystem] Cấp tài khoản nhân viên đại lý {DealerName}",
    PreHeaderText = "Chào mừng bạn gia nhập hệ thống đại lý EVSystem – tài khoản nhân viên đã được khởi tạo.",
    PersonalizationTags = "{EmployeeName},{DealerName},{Username},{TempPassword},{LoginLink}",
    BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Tài khoản nhân viên đại lý - EVSystem</title>
    <style>
        body { font-family: 'Segoe UI', Roboto, Arial, sans-serif; margin: 0; padding: 0; background-color: #f3f4f6; color: #333; }
        .container { padding: 40px 0; background: #f3f4f6; }
        .card {
            width: 640px; margin: auto; background: #fff; border-radius: 14px;
            box-shadow: 0 8px 24px rgba(0,0,0,0.08); overflow: hidden;
        }
        .header {
            background: linear-gradient(90deg, #B71C1C, #E53935);
            color: #fff; text-align: center; padding: 28px 20px;
        }
        .header h1 {
            margin: 0; font-size: 26px; letter-spacing: 0.3px; font-weight: 700;
        }
        .header p {
            margin-top: 8px; font-size: 15px; opacity: 0.9;
        }
        .content {
            padding: 40px 45px; line-height: 1.75; color: #333;
        }
        .content p {
            font-size: 15px; margin-bottom: 14px;
        }
        .info-box {
            background: #f9fafb; border: 1px solid #e0e0e0;
            border-radius: 10px; padding: 18px 22px; margin: 26px 0;
        }
        .info-box td { padding: 8px 0; font-size: 15px; }
        .info-box td:first-child { width: 180px; font-weight: 600; color: #444; }
        .btn {
            display: inline-block; padding: 14px 32px; background: #E53935;
            color: #fff; text-decoration: none; border-radius: 8px;
            font-weight: 600; font-size: 16px; letter-spacing: 0.3px;
            transition: all 0.3s ease;
        }
        .btn:hover { background: #C62828; transform: translateY(-2px); }
        .note {
            font-size: 13px; color: #666; margin-top: 28px;
            line-height: 1.7; border-top: 1px dashed #ddd; padding-top: 18px;
        }
        .footer {
            background: #fafafa; text-align: center; padding: 22px;
            font-size: 13px; color: #777; border-top: 1px solid #eee;
        }
        .footer a { color: #E53935; text-decoration: none; }
        @media (max-width: 640px) {
            .card { width: 95%; }
            .content { padding: 28px; }
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='card'>
            <div class='header'>
                <h1>Electric Vehicle Management System</h1>
                <p>Chào mừng bạn đến với hệ thống đại lý EVSystem</p>
            </div>
            <div class='content'>
                <p>Xin chào <strong>{EmployeeName}</strong>,</p>
                <p>Chúng tôi rất vui mừng thông báo rằng tài khoản nhân viên của bạn tại <strong>Đại lý {DealerName}</strong> đã được khởi tạo thành công trên <strong>Hệ thống xe điện EVSystem</strong>.</p>

                <div class='info-box'>
                    <table width='100%'>
                        <tr>
                            <td>Đại lý:</td>
                            <td>{DealerName}</td>
                        </tr>
                        <tr>
                            <td>Tên đăng nhập:</td>
                            <td>{Username}</td>
                        </tr>
                        <tr>
                            <td>Mật khẩu tạm thời:</td>
                            <td>{TempPassword}</td>
                        </tr>
                    </table>
                </div>

                <p style='text-align:center; margin:32px 0;'>
                    <a href='{LoginLink}' class='btn'>Đăng nhập và kích hoạt</a>
                </p>

                <p class='note'>
                    🔐 <strong>Lưu ý bảo mật:</strong><br>
                    • Vui lòng đăng nhập và đổi mật khẩu ngay trong lần đầu sử dụng.<br>
                    • Không chia sẻ thông tin tài khoản của bạn với bất kỳ ai.<br>
                    • Nếu bạn không yêu cầu tạo tài khoản này, vui lòng liên hệ quản trị viên hệ thống để được hỗ trợ ngay.
                </p>
            </div>
            <div class='footer'>
                <p>Trân trọng,<br><strong>EVSystem Support Team</strong></p>
                <p>Website: <a href='https://electricvehiclesystem.click'>electricvehiclesystem.click</a></p>
            </div>
        </div>
    </div>
</body>
</html>",
    FooterContent = "EVSystem Team – Electric Vehicle Management System",
    CallToAction = "<a href=\"{LoginLink}\" class=\"btn\">Đăng nhập và kích hoạt</a>",
    Language = "Vietnamese",
    RecipientType = "DealerEmployee",
    CreatedBy = "System",
    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    UpdatedBy = "System",
    UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    Status = EmailStatus.Active
});

            modelBuilder.Entity<EmailTemplate>().HasData(
new
{
    Id = Guid.Parse("D9C3C4C2-0A5A-4E2A-8D3C-9A3B4C5D6E7F"),
    TemplateName = "NotifyAddedToDealerExistingUser",
    SenderName = "EVSystem Support",
    SenderEmail = "hoangtuzami@gmail.com",
    Category = "Notification",
    SubjectLine = "[EVSystem] Bạn đã được thêm vào Đại lý {DealerName}",
    PreHeaderText = "Tài khoản của bạn đã được liên kết với đại lý. Đăng nhập để bắt đầu làm việc.",
    PersonalizationTags = "{EmployeeName},{DealerName},{RoleInDealer},{LoginLink}",
    BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Thông báo thêm vào đại lý</title>
  <style>
    body { font-family: 'Segoe UI', Roboto, Arial, sans-serif; margin:0; padding:0; background:#f3f4f6; color:#333; }
    .wrap { padding:40px 0; }
    .card { width:640px; margin:auto; background:#fff; border-radius:14px; box-shadow:0 8px 24px rgba(0,0,0,0.08); overflow:hidden; }
    .header { background:linear-gradient(90deg,#B71C1C,#E53935); color:#fff; text-align:center; padding:28px 20px; }
    .header h1 { margin:0; font-size:26px; font-weight:700; }
    .header p { margin:8px 0 0; font-size:15px; opacity:.95; }
    .content { padding:40px 45px; line-height:1.75; }
    .content p { font-size:15px; margin:0 0 12px; }
    .box { background:#f9fafb; border:1px solid #e0e0e0; border-radius:10px; padding:18px 22px; margin:22px 0; }
    .box table { width:100%; border-collapse:collapse; }
    .box td { padding:8px 0; font-size:15px; vertical-align:top; }
    .box td:first-child { width:190px; font-weight:600; color:#444; }
    .btn { display:inline-block; padding:14px 32px; background:#E53935; color:#fff !important; text-decoration:none; border-radius:8px; font-weight:600; font-size:16px; letter-spacing:.3px; transition:.25s; }
    .btn:hover { background:#C62828; transform:translateY(-2px); }
    .note { font-size:13px; color:#666; margin-top:24px; border-top:1px dashed #ddd; padding-top:16px; }
    .footer { background:#fafafa; text-align:center; padding:22px; font-size:13px; color:#777; border-top:1px solid #eee; }
    .footer a { color:#E53935; text-decoration:none; }
    @media (max-width:640px){ .card{width:95%} .content{padding:28px} }
  </style>
</head>
<body>
  <div class='wrap'>
    <div class='card'>
      <div class='header'>
        <h1>Electric Vehicle Management System</h1>
        <p>Thông báo: Liên kết tài khoản với đại lý</p>
      </div>
      <div class='content'>
        <p>Xin chào <strong>{EmployeeName}</strong>,</p>
        <p>Tài khoản EVSystem của bạn đã được thêm vào <strong>Đại lý {DealerName}</strong> với vai trò <strong>{RoleInDealer}</strong>.</p>

        <div class='box'>
          <table>
            <tr>
              <td>Đại lý:</td>
              <td>{DealerName}</td>
            </tr>
            <tr>
              <td>Vai trò của bạn:</td>
              <td>{RoleInDealer}</td>
            </tr>
          </table>
        </div>

        <p style='text-align:center; margin:28px 0;'>
          <a href='{LoginLink}' class='btn'>Đăng nhập để bắt đầu</a>
        </p>

        <p class='note'>
          🔐 <strong>Lưu ý bảo mật:</strong><br>
          • Không chia sẻ tài khoản cho người khác.<br>
          • Nếu bạn không mong đợi thay đổi này, vui lòng liên hệ quản trị hệ thống ngay.
        </p>
      </div>
      <div class='footer'>
        <p>Trân trọng,<br><strong>EVSystem Support Team</strong></p>
        <p>Website: <a href='https://electricvehiclesystem.click'>electricvehiclesystem.click</a></p>
      </div>
    </div>
  </div>
</body>
</html>",
    FooterContent = "EVSystem Team – Electric Vehicle Management System",
    CallToAction = "<a href=\"{LoginLink}\" class=\"btn\">Đăng nhập để bắt đầu</a>",
    Language = "Vietnamese",
    RecipientType = "DealerEmployee",
    CreatedBy = "System",
    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    UpdatedBy = "System",
    UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    Status = EmailStatus.Active
});

            modelBuilder.Entity<EmailTemplate>().HasData(
new
{
    Id = Guid.Parse("A8D5E7F1-13B2-4F0C-BE9F-9D5E6C2B8F7A"),
    TemplateName = "NotifyPaymentLinkToCustomer",
    SenderName = "EVSystem",
    SenderEmail = "hoangtuzami@gmail.com",
    Category = "Payment",
    SubjectLine = "[EVSystem] Liên kết thanh toán đơn hàng",
    PreHeaderText = "Đơn hàng của bạn đã được tạo. Vui lòng thanh toán để hoàn tất quá trình đặt hàng.",
    PersonalizationTags = "{CustomerName},{OrderNo},{OrderAmount},{PaymentLink}",
    BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Thanh toán đơn hàng</title>
  <style>
    body { font-family: 'Segoe UI', Roboto, Arial, sans-serif; margin:0; padding:0; background:#f3f4f6; color:#333; }
    .wrap { padding:40px 0; }
    .card { width:640px; margin:auto; background:#fff; border-radius:14px; box-shadow:0 8px 24px rgba(0,0,0,0.08); overflow:hidden; }
    .header { background:linear-gradient(90deg,#1565C0,#42A5F5); color:#fff; text-align:center; padding:28px 20px; }
    .header h1 { margin:0; font-size:26px; font-weight:700; }
    .header p { margin:8px 0 0; font-size:15px; opacity:.95; }
    .content { padding:40px 45px; line-height:1.75; }
    .content p { font-size:15px; margin:0 0 12px; }
    .box { background:#f9fafb; border:1px solid #e0e0e0; border-radius:10px; padding:18px 22px; margin:22px 0; }
    .box table { width:100%; border-collapse:collapse; }
    .box td { padding:8px 0; font-size:15px; vertical-align:top; }
    .box td:first-child { width:190px; font-weight:600; color:#444; }
    .btn { display:inline-block; padding:14px 32px; background:#1976D2; color:#fff !important; text-decoration:none; border-radius:8px; font-weight:600; font-size:16px; letter-spacing:.3px; transition:.25s; }
    .btn:hover { background:#0D47A1; transform:translateY(-2px); }
    .note { font-size:13px; color:#666; margin-top:24px; border-top:1px dashed #ddd; padding-top:16px; }
    .footer { background:#fafafa; text-align:center; padding:22px; font-size:13px; color:#777; border-top:1px solid #eee; }
    .footer a { color:#1976D2; text-decoration:none; }
    @media (max-width:640px){ .card{width:95%} .content{padding:28px} }
  </style>
</head>
<body>
  <div class='wrap'>
    <div class='card'>
      <div class='header'>
        <h1>Electric Vehicle Management System</h1>
        <p>Thanh toán đơn hàng #{OrderNo}</p>
      </div>
      <div class='content'>
        <p>Xin chào <strong>{CustomerName}</strong>,</p>
        <p>Cảm ơn bạn đã đặt hàng tại EVSystem. Vui lòng thanh toán để xác nhận đơn hàng của bạn.</p>

        <div class='box'>
          <table>
            <tr>
              <td>Mã đơn hàng:</td>
              <td><strong>#{OrderNo}</strong></td>
            </tr>
            <tr>
              <td>Số tiền cần thanh toán:</td>
              <td><strong>{OrderAmount} VND</strong></td>
            </tr>
          </table>
        </div>

        <p style='text-align:center; margin:28px 0;'>
          <a href='{PaymentLink}' class='btn'>Thanh toán ngay</a>
        </p>

        <p class='note'>
          💡 <strong>Lưu ý:</strong><br>
          • Liên kết thanh toán chỉ có hiệu lực trong 15 phút kể từ thời điểm tạo.<br>
          • Nếu bạn gặp sự cố khi thanh toán, vui lòng liên hệ bộ phận hỗ trợ EVSystem.<br>
          • Sau khi thanh toán thành công, đơn hàng sẽ được xử lý ngay lập tức.
        </p>
      </div>
      <div class='footer'>
        <p>Trân trọng,<br><strong>EVSystem Billing Team</strong></p>
        <p>Website: <a href='https://electricvehiclesystem.click'>electricvehiclesystem.click</a></p>
      </div>
    </div>
  </div>
</body>
</html>",
    FooterContent = "EVSystem Billing – Electric Vehicle Management System",
    CallToAction = "<a href=\"{PaymentLink}\" class=\"btn\">Thanh toán ngay</a>",
    Language = "Vietnamese",
    RecipientType = "Customer",
    CreatedBy = "System",
    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    UpdatedBy = "System",
    UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
    Status = EmailStatus.Active
});

            modelBuilder.Entity<EmailTemplate>().HasData(
    new
    {
        Id = Guid.Parse("f3c5b8d2-4a67-4a5f-9c32-11c5b4f8d2a1"),
        TemplateName = "NotifyEContractUpdated",
        SenderName = "EVSystem",
        SenderEmail = "hoangtuzami@gmail.com",
        Category = "Contract",
        SubjectLine = "[EVSystem] Hợp đồng điện tử của bạn đã được cập nhật",
        PreHeaderText = "Hợp đồng điện tử gắn với tài khoản của bạn vừa được cập nhật. Vui lòng xem lại để đảm bảo quyền lợi.",
        PersonalizationTags = "{CustomerName},{UpdatedAt},{ViewLink},{SupportEmail}",
        BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Thông báo cập nhật hợp đồng điện tử</title>
  <style>
    body { font-family: 'Segoe UI', Roboto, Arial, sans-serif; margin:0; padding:0; background:#f3f4f6; color:#111827; }
    .wrap { padding:40px 0; }
    .card {
        width:640px; margin:auto; background:#ffffff; border-radius:14px;
        box-shadow:0 8px 24px rgba(15,23,42,0.10); overflow:hidden;
    }
    .header {
        background:linear-gradient(90deg,#F59E0B,#F97316);
        color:#ffffff; text-align:center; padding:26px 20px;
    }
    .header h1 { margin:0; font-size:24px; font-weight:700; letter-spacing:0.3px; }
    .header p { margin:8px 0 0; font-size:14px; opacity:.95; }
    .content { padding:36px 42px; line-height:1.7; }
    .content p { font-size:15px; margin:0 0 12px; }
    .info-box {
        background:#f9fafb; border:1px solid #e5e7eb;
        border-radius:10px; padding:16px 20px; margin:22px 0;
    }
    .info-box table { width:100%; border-collapse:collapse; }
    .info-box td { padding:6px 0; font-size:14px; vertical-align:top; }
    .info-box td:first-child { width:180px; font-weight:600; color:#374151; }
    .btn {
        display:inline-block; padding:14px 30px; background:#F97316;
        color:#ffffff !important; text-decoration:none; border-radius:8px;
        font-weight:600; font-size:15px; letter-spacing:0.3px;
        transition:all 0.25s ease;
    }
    .btn:hover { background:#EA580C; transform:translateY(-2px); }
    .note {
        font-size:13px; color:#6b7280; margin-top:24px;
        border-top:1px dashed #e5e7eb; padding-top:16px; line-height:1.6;
    }
    .footer {
        background:#fafafa; text-align:center; padding:20px;
        font-size:13px; color:#6b7280; border-top:1px solid #e5e7eb;
    }
    .footer a { color:#F97316; text-decoration:none; }
    @media (max-width:640px){
        .card{width:95%}
        .content{padding:26px}
    }
  </style>
</head>
<body>
  <div class='wrap'>
    <div class='card'>
      <div class='header'>
        <h1>Electric Vehicle Management System</h1>
        <p>Thông báo cập nhật hợp đồng điện tử</p>
      </div>
      <div class='content'>
        <p>Xin chào <strong>{CustomerName}</strong>,</p>
        <p>
          Hợp đồng điện tử gắn với tài khoản của bạn trên <strong>EVSystem</strong> đã được cập nhật.
        </p>
        <p>
          Để đảm bảo quyền lợi và nắm rõ các điều khoản mới nhất, vui lòng đăng nhập và kiểm tra lại nội dung hợp đồng.
        </p>

        <div class='info-box'>
          <table>
            <tr>
              <td>Loại tài liệu:</td>
              <td>Hợp đồng điện tử (eContract)</td>
            </tr>
            <tr>
              <td>Thời điểm cập nhật:</td>
              <td><strong>{UpdatedAt}</strong></td>
            </tr>
          </table>
        </div>

        <p style='text-align:center; margin:26px 0;'>
          <a href='{ViewLink}' class='btn'>Xem hợp đồng đã cập nhật</a>
        </p>

        <p class='note'>
          ℹ️ <strong>Lưu ý:</strong><br>
          • Vui lòng đọc kỹ lại nội dung hợp đồng trước khi tiếp tục các bước tiếp theo trong quy trình.<br>
          • Nếu bạn có bất kỳ câu hỏi hoặc cần hỗ trợ thêm, vui lòng liên hệ bộ phận hỗ trợ EVSystem qua email:
            <a href='mailto:{SupportEmail}'>{SupportEmail}</a>.<br>
          • Đây là email được gửi tự động, vui lòng không trả lời trực tiếp email này.
        </p>
      </div>
      <div class='footer'>
        <p>Trân trọng,<br><strong>EVSystem Support Team</strong></p>
        <p>Website: <a href='https://electricvehiclesystem.click'>electricvehiclesystem.click</a></p>
      </div>
    </div>
  </div>
</body>
</html>",
        FooterContent = "EVSystem Support – Electric Vehicle Management System",
        CallToAction = "<a href=\"{ViewLink}\" class=\"btn\">Xem hợp đồng đã cập nhật</a>",
        Language = "Vietnamese",
        RecipientType = "Customer",
        CreatedBy = "System",
        CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
        UpdatedBy = "System",
        UpdatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc),
        Status = EmailStatus.Active
    });

            modelBuilder.Entity<EmailTemplate>().HasData(
new
{
    Id = Guid.Parse("E3F7C9B1-3C44-4A89-9AF3-98D7C1B56CC1"),
    TemplateName = "PublicCustomer_ContractReviewAndConfirm",
    SenderName = "EVSystem",
    SenderEmail = "hoangtuzami@gmail.com",
    Category = "EContract",
    SubjectLine = "Hợp đồng điện tử của bạn đã sẵn sàng – vui lòng xem và xác nhận",
    PreHeaderText = "Bạn nhận được một hợp đồng điện tử từ EVSystem. Vui lòng xem và xác nhận.",
    PersonalizationTags = "{CustomerName},{ContractName},{ViewLink},{SupportEmail}",
    BodyContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Xác nhận hợp đồng điện tử</title>
  <style>
    body { font-family: 'Segoe UI', Roboto, Arial, sans-serif; margin:0; padding:0; background:#f5f6fa; color:#111827; }
    .wrap { padding:40px 0; }
    .card {
        width:640px; margin:auto; background:#ffffff; border-radius:14px;
        box-shadow:0 8px 20px rgba(0,0,0,0.08); overflow:hidden;
    }
    .header {
        background:#2563EB; color:#ffffff; text-align:center; padding:26px 24px;
    }
    .header h1 { margin:0; font-size:23px; font-weight:700; }
    .content { padding:36px 42px; line-height:1.7; }
    .content p { font-size:15px; margin-bottom:14px; }
    .btn-primary {
        display:inline-block; padding:14px 32px; background:#2563EB;
        color:#ffffff !important; text-decoration:none; border-radius:8px;
        font-size:15px; font-weight:600; transition:0.25s ease;
    }
    .btn-primary:hover { background:#1E40AF; }
    .footer {
        background:#fafafa; text-align:center; padding:20px;
        font-size:13px; color:#6b7280; border-top:1px solid #e5e7eb;
    }
    .footer a { color:#2563EB; text-decoration:none; }
  </style>
</head>

<body>
  <div class='wrap'>
    <div class='card'>
      <div class='header'>
        <h1>Hợp đồng điện tử cần xác nhận</h1>
      </div>

      <div class='content'>
        <p>Xin chào <strong>{CustomerName}</strong>,</p>

        <p>
          Bạn nhận được một hợp đồng điện tử từ <strong>EVSystem</strong> liên quan đến:
          <strong>{ContractName}</strong>.
        </p>

        <p>
          Vui lòng nhấn nút bên dưới để xem toàn bộ nội dung hợp đồng. 
          Trên trang hiển thị hợp đồng, bạn có thể lựa chọn:
        </p>

        <ul style='font-size:15px; line-height:1.7;'>
          <li><strong>Xác nhận hợp đồng</strong> nếu bạn đồng ý</li>
          <li><strong>Từ chối / Không đồng ý</strong> nếu bạn không chấp thuận</li>
        </ul>

        <div style='text-align:center; margin:30px 0;'>
          <a href='{ViewLink}' class='btn-primary'>Xem hợp đồng</a>
        </div>

        <p style='font-size:14px; color:#6b7280; margin-top:20px;'>
          Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ:
          <a href='mailto:{SupportEmail}'>{SupportEmail}</a>.
        </p>
      </div>

      <div class='footer'>
        <p>Trân trọng,<br><strong>EVSystem</strong></p>
      </div>
    </div>
  </div>
</body>
</html>",
    FooterContent = "EVSystem – Hệ thống hợp đồng điện tử",
    CallToAction = "<a href=\"{ViewLink}\" class=\"btn-primary\">Xem hợp đồng</a>",
    Language = "Vietnamese",
    RecipientType = "PublicCustomer",
    CreatedBy = "System",
    CreatedAt = new DateTime(2025, 02, 01, 0, 0, 0, DateTimeKind.Utc),
    UpdatedBy = "System",
    UpdatedAt = new DateTime(2025, 02, 01, 0, 0, 0, DateTimeKind.Utc),
    Status = EmailStatus.Active
});


        }
    }
}
