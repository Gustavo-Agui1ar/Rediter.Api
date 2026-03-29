namespace Rediter.Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configs;

        public EmailService(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

            _configs = configuration;
        }

        public async Task SendVerificationCodeAsync(string email, string name, string code)
        {
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("Rediter App", "noreply@rediter.com"));
            message.To.Add(new MimeKit.MailboxAddress(name, email));
            message.Subject = "Rediter - Código de Verificação";

            message.Body = new MimeKit.TextPart("html")
            {
                Text = $@"
                <h1>Olá, {name}!</h1>
                <p>Seja bem-vindo ao Rediter. Use o código abaixo para confirmar sua conta:</p>
                <h2 style='color: #4A90E2;'>{code}</h2>
                <p>Este código expira em 1 hora.</p>"
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                string host = _configs["EmailSettings:Host"] ?? throw new Exception("SMTP host not configured."); 
                int port = int.TryParse(_configs["EmailSettings:Port"], out int p) ? p : throw new Exception("SMTP port not configured or invalid.");
                string user = _configs["EmailSettings:User"] ?? throw new Exception("SMTP user not configured.");
                string password = _configs["EmailSettings:Password"] ?? throw new Exception("SMTP password not configured.");

                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(user, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send verification email: " + ex.Message);
            }
        }
    }
}
