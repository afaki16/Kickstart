using Kickstart.Application.Common.Results;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Models;
using Kickstart.Infrastructure.Configuration;
using Kickstart.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Kickstart.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _smtp = configuration.GetSection("NotificationService:Mail").Get<SmtpSettings>()
                    ?? new SmtpSettings();
            _logger = logger;
        }

        public async Task<Result> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtp.Host, _smtp.Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                    EnableSsl = _smtp.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtp.FromAddress, _smtp.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("E-posta gönderildi: {To}, Konu: {Subject}", to, subject);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilemedi: {To}", to);
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "E-posta gönderilemedi."));
            }
        }

        public async Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            const string subject = "E-posta Adresinizi Doğrulayın";

            var content = $@"
<h2 style=""color:#333;"">E-posta Doğrulama</h2>
<p>Hesabınızı aktif etmek için aşağıdaki butona tıklayın.</p>
{EmailTemplates.CtaButton("E-postamı Doğrula", confirmationLink)}
<p style=""color:#666;"">Bu isteği siz yapmadıysanız dikkate almayın.</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }

        public async Task<Result> SendPasswordResetAsync(string email, string firstName, string resetLink)
        {
            const string subject = "Şifre Sıfırlama";

            var content = $@"
<h2 style=""color:#333;"">Şifre Sıfırlama</h2>
<p>Merhaba <strong>{firstName}</strong>,</p>
<p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın.</p>
{EmailTemplates.CtaButton("Şifremi Sıfırla", resetLink)}
<p style=""color:#666;"">Bu link <strong>24 saat</strong> geçerlidir.</p>
<p style=""color:#666;"">Bu isteği siz yapmadıysanız dikkate almayın.</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }

        public async Task<Result> SendWelcomeEmailAsync(string email, string userName)
        {
            const string subject = "Hoş Geldiniz!";

            var content = $@"
<h2 style=""color:#333;"">Hoş Geldiniz, <strong>{userName}</strong>!</h2>
<p>Hesabınız başarıyla oluşturuldu. Artık platformumuzu kullanabilirsiniz.</p>
<p style=""color:#666;"">İyi kullanımlar dileriz.</p>";

            return await SendEmailAsync(email, subject, EmailTemplates.Wrap(subject, content));
        }
    }
}
