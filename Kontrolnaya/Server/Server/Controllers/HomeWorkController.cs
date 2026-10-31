using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Server.Framework.attribute;
using Server.Framework.Handlers;

namespace Server.Controllers
{
    [HttpController("home-work")]
    internal class HomeWorkController
    {
        private const string GroupNumber = "11-541";
        private const string FullName = "Фаизов Марсель Алмазович";

        private const string SmtpHost = "smtp.yandex.ru";
        private const int SmtpPort = 465;
        private const string SmtpLogin = "marself4izov@yandex.ru";
        private const string SmtpPassword = "fbggmvwdmetafcmc";

        [Get("send")]
        public async Task Send(HandlerContext context, string to)
        {
            byte[] zip = await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "project.zip"));

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(SmtpLogin));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = "Домашняя работа";

            var builder = new BodyBuilder
            {
                TextBody = $"Группа: {GroupNumber}\nФИО: {FullName}"
            };
            builder.Attachments.Add("project.zip", zip);
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.SslOnConnect);
            await smtp.AuthenticateAsync(SmtpLogin, SmtpPassword);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);

            await context.SendTextAsync("OK", 200, "text/plain; charset=utf-8");
        }
    }
}