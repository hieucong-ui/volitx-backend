using Amazon.Runtime.Internal.Transform;
using Microsoft.Extensions.Configuration;
using Voltix.Application.IService;
using Voltix.Domain.Constants;
using Voltix.Infrastructure.IRepository;
using System.Net;
using System.Net.Mail;

namespace Voltix.Application.Service
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public EmailService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> SendContractEmailAsync(string to, string fullName, string contractSubject, string downloadLink, byte[]? pdfBytes = null, string? pdfFileName = null, string? companyName = null, string? supportEmail = null)
        {
            var isSuccess = false;
            try
            {
                var template = await _unitOfWork.EmailTemplateRepository.GetByNameAsync("NotifyContractPdf");
                if (template is null)
                    throw new ArgumentNullException("Email template: NotifyContractPdf not found");

                var company = companyName ?? (_configuration["Company:Name"] ?? "SWP391");
                var support = supportEmail ?? (_configuration["Support:Email"] ?? "support@swp391.vn");

                var map = new Dictionary<string, string>
                {
                    { "{FullName}", fullName },
                    { "{ContractSubject}", contractSubject },
                    { "{DownloadLink}", downloadLink },
                    { "{Company}", company },
                    { "{SupportEmail}", support }
                };

                string subject = ReplacePlaceholders(template.SubjectLine ?? string.Empty, map);
                string body = ReplacePlaceholders(template.BodyContent ?? string.Empty, map);

                if (pdfBytes is null || pdfBytes.Length == 0)
                    return await SendEmailAsync(to, subject, body);

                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var userName = _configuration["EmailSettings:UserName"];
                var password = _configuration["EmailSettings:Password"];
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:Port");
                var enableSsl = _configuration.GetValue<bool?>("EmailSettings:EnableSsl") ?? true;

                if (fromEmail is null || userName is null || password is null || smtpHost is null)
                    throw new ArgumentNullException("Email configuration is missing");

                using var message = new MailMessage(fromEmail, to, subject, body) { IsBodyHtml = true };
                var fileName = string.IsNullOrWhiteSpace(pdfFileName) ? "contract.pdf" : pdfFileName;
                var stream = new MemoryStream(pdfBytes);
                var attachment = new Attachment(stream, fileName, System.Net.Mime.MediaTypeNames.Application.Pdf);
                message.Attachments.Add(attachment);

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = enableSsl
                };

                await smtpClient.SendMailAsync(message);
                isSuccess = true;
                return isSuccess;
            }
            catch
            {
                return isSuccess;
            }
        }

        private static string ReplacePlaceholders(string text, Dictionary<string, string> map)
        {
            if (string.IsNullOrEmpty(text) || map is null) return text;
            foreach (var kv in map)
                text = text.Replace(kv.Key, kv.Value ?? string.Empty);
            return text;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            var isSuccess = false;
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var userName = _configuration["EmailSettings:UserName"];
                var password = _configuration["EmailSettings:Password"];
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:Port");

                if (fromEmail is null || userName is null || password is null || smtpHost is null)
                {
                    throw new ArgumentNullException("Email configuration is missing");
                }

                var message = new MailMessage(fromEmail, to, subject, body);
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(message);

                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<bool> SendEmailFromTemplate(string to, string templateName, Dictionary<string, string> placeholders)
        {
            var isSuccess = false;
            try
            {
                var template = await _unitOfWork.EmailTemplateRepository.GetByNameAsync(templateName);
                if (template is null)
                {
                    throw new ArgumentNullException($"Email template: {templateName} not found");
                }
                var subject = template.SubjectLine;
                var body = template.BodyContent;
                foreach (var placeholder in placeholders)
                {
                    body = body.Replace($"{placeholder.Key}", placeholder.Value);
                }
                isSuccess = await SendEmailAsync(to, subject, body);
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<bool> SendResetPassword(string email, string resetLink)
        {
            return await SendEmailFromTemplate(email, "ResetPassword", new Dictionary<string, string>
            {
                { "{ResetLink}", resetLink }
            });
        }

        public async Task<bool> SendVerifyEmail(string to, string verifyLink)
        {
            return await SendEmailFromTemplate(to, "SendVerifyEmail", new Dictionary<string, string>
            {
                { "{Login}", verifyLink }
            });
        }

        public async Task<bool> SendEVMStaffAccountEmail(string to, string fullName, string password)
        {
            return await SendEmailFromTemplate(to, "SendEmployeeAccount", new Dictionary<string, string>
            {
                { "{EmployeeName}", fullName },
                { "{Email}" , to },
                { "{TempPassword}", password },
                { "{LoginLink}", StaticLinkUrl.WebUrl }
            });
        }

        public async Task<bool> SendDealerStaffAaccountEmail(string to, string fullName, string password, string dealerName)
        {
            return await SendEmailFromTemplate(to, "SendDealerStaff", new Dictionary<string, string>
            {
                { "{EmployeeName}", fullName },
                { "{DealerName}", dealerName },
                { "{Email}" , to },
                { "{TempPassword}", password },
                { "{LoginLink}", StaticLinkUrl.WebUrl }
            });
        }

        public Task<bool> NotifyAddedToDealerExistingUser(string to, string fullName, string roleInDealer, string dealerName)
        {
            return SendEmailFromTemplate(to, "NotifyAddedToDealerExistingUser", new Dictionary<string, string>
            {
                { "{EmployeeName}", fullName },
                { "{DealerName}", dealerName },
                { "{RoleInDealer}" , roleInDealer },
                { "{LoginLink}", StaticLinkUrl.WebUrl }
            });
        }

        public Task<bool> NotifyPaymentLinkToCustomer(string to, string customerName, int orderNo, decimal orderAmount, string paymentLink)
        {
            return SendEmailFromTemplate(to, "NotifyPaymentLinkToCustomer", new Dictionary<string, string>
            {
                { "{CustomerName}", customerName },
                { "{OrderNo}", orderNo.ToString() },
                { "{OrderAmount}" , orderAmount.ToString() },
                { "{PaymentLink}", paymentLink }
            });
        }

        public Task<bool> NotifyEContractUpdated(string to, string fullName, string UpdatedAt, string downloadLink)
        {
            return SendEmailFromTemplate(to, "NotifyEContractUpdated", new Dictionary<string, string>
            {
                { "{CustomerName}", fullName },
                { "{UpdatedAt}", UpdatedAt },
                { "{ViewLink}" , downloadLink },
                { "SupportEmail", _configuration["Company:Email"] ?? "" }
            });
        }

        public Task<bool> SendContractReviewAndConfirm(string to, string customerName, string contractName, string confirmLink)
        {
            return SendEmailFromTemplate(to, "PublicCustomer_ContractReviewAndConfirm", new Dictionary<string, string>
            {
                { "{CustomerName}", customerName },
                { "{ContractName}", contractName },
                { "{ViewLink}", confirmLink },
                { "{SupportEmail}",  _configuration["Company:Email"] ?? "" }
            });
        }
    }
}
