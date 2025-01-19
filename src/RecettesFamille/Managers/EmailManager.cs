namespace RecettesFamille.Managers;

using System;
using System.IO;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

public class EmailManager(IConfiguration Config)
{
    private const string _smtpServer = "smtp.gmail.com";
    private const int _smtpPort = 587;
    private readonly string _smtpPassword;

    public async Task<bool> SendEmailAsync(string from, string to, string subject, string bodyText, string bodyHtml, string[] attachmentPaths = null)
    {
        
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(from, from));
            email.To.Add(new MailboxAddress(to, to));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = bodyText,
                HtmlBody = bodyHtml
            };

            // Ajout des pièces jointes si fournies
            if (attachmentPaths != null)
            {
                foreach (var filePath in attachmentPaths)
                {
                    if (File.Exists(filePath))
                    {
                        bodyBuilder.Attachments.Add(filePath);
                    }
                }
            }

            email.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(Config["SMTP_USER"], Config["SMTP_PASSWORD"]);
            await client.SendAsync(email);
            await client.DisconnectAsync(true);

            Console.WriteLine("✅ Email envoyé avec succès !");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de l'envoi de l'email : {ex.Message}");
            return false;
        }
    }
}