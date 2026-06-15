using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Security;
using MimeKit;

namespace Rediter.Api.Services.UtilitariesServices
{
    public class EmailService
    {
        private readonly IConfiguration _configs;

        public EmailService(IConfiguration configuration)
        {
            _configs = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
        }

        public async Task SendEmailAsync(string fromName, string fromEmail, string toName, string toEmail, string subject, string bodyHtml)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = bodyHtml
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                string host = _configs["EmailSettings:Host"] ?? throw new Exception("SMTP host not configured.");
                int port = int.TryParse(_configs["EmailSettings:Port"], out int p) ? p : throw new Exception("SMTP port not configured or invalid.");
                string user = _configs["EmailSettings:User"] ?? throw new Exception("SMTP user not configured.");
                string password = _configs["EmailSettings:Password"] ?? throw new Exception("SMTP password not configured.");

                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send email: " + ex.Message);
            }
        }

        public async Task SendVerificationCodeAsync(string email, string name, string code)
        {
            string subject = "Rediter - Seu Código de Verificação";

            string bodyHtml = $@"
            <!DOCTYPE html>
            <html lang=""pt-BR"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <style>
                    body {{ 
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; 
                        background-color: #f6f9fc; 
                        margin: 0; 
                        padding: 40px 20px; 
                        -webkit-font-smoothing: antialiased; 
                        color: #333333;
                    }}
                    .container {{ 
                        max-width: 540px; 
                        margin: 0 auto; 
                        background-color: #ffffff; 
                        border-radius: 6px; 
                        padding: 40px; 
                        box-shadow: 0 2px 8px rgba(0,0,0,0.04); 
                        border: 1px solid #eaeaea;
                    }}
                    .logo {{ 
                        text-align: center; 
                        margin-bottom: 40px; 
                        font-size: 22px;
                        font-weight: bold;
                        color: #4A90E2; /* Cor azul do Rediter */
                    }}
                    .title {{ 
                        font-size: 16px; 
                        margin-bottom: 10px; 
                        color: #333333;
                    }}
                    .code {{ 
                        font-size: 32px; 
                        font-weight: bold; 
                        color: #111111; 
                        margin: 0 0 30px 0; 
                        letter-spacing: 2px;
                    }}
                    .text {{ 
                        font-size: 14px; 
                        line-height: 1.6; 
                        margin-bottom: 20px; 
                        color: #333333;
                    }}
                    .footer-text {{
                        font-size: 14px;
                        line-height: 1.6;
                        color: #333333;
                        margin-top: 30px;
                    }}
                    a {{
                        color: #4A90E2;
                        text-decoration: none;
                    }}
                </style>
            </head>
            <body>
                <div class=""container"">
                    <div class=""logo"">
                        Rediter
                    </div>
                    
                    <div class=""title"">
                        Seu código de verificação para o Rediter é
                    </div>
                    <div class=""code"">
                        {code}
                    </div>
                    
                    <div class=""text"">
                        Olá, {name},
                    </div>
                    
                    <div class=""text"">
                        Este é o seu código de verificação de uso único. O código é válido por apenas 60 minutos.
                    </div>
                    
                    <div class=""text"">
                        Precisa de ajuda? Basta responder a este e-mail ou entrar em contato pelo endereço <a href=""mailto:suporte@rediter.com"">suporte@rediter.com</a>. Ficaremos felizes em ajudar.
                    </div>
                    
                    <div class=""footer-text"">
                        Obrigado,<br>
                        Equipe Rediter
                    </div>
                </div>
            </body>
            </html>";

            await SendEmailAsync(
                fromName: "Rediter App",
                fromEmail: "noreply@rediter.com",
                toName: name,
                toEmail: email,
                subject: subject,
                bodyHtml: bodyHtml
            );
        }
    }
}