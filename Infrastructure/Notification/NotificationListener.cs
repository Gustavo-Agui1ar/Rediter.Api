using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rediter.Api.Data;
using Rediter.Api.Hubs;
using Rediter.Api.Models;
using Rediter.Api.Models.Users;
using System.Text;
using System.Text.Json;

namespace Rediter.Api.Infrastructure.Notifications
{
    public class NotificationListener : BackgroundService
    {
        private readonly IConnection _rabbitConnection;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationListener> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _queueName = "notificacoes_fila";

        public NotificationListener(
            IConnection rabbitConnection,
            IHubContext<NotificationHub> hubContext,
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationListener> logger)
        {
            _rabbitConnection = rabbitConnection;
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RabbitMQ] Iniciando o consumo de notificações em background...");

            try
            {
                await using var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(queue: _queueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null,
                                                cancellationToken: stoppingToken);

                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var mensagemJson = Encoding.UTF8.GetString(body);

                        var payload = JsonSerializer.Deserialize<NotificationDTO>(mensagemJson);

                        if (payload == null)
                        {
                            _logger.LogWarning("[RabbitMQ] Payload da notificação é nulo.");
                            return;
                        }

                        if (payload.ReceiverUserId == Guid.Empty)
                        {
                            _logger.LogWarning("[RabbitMQ] Payload da notificação possui ReceiverUserId vazio.");
                            return;
                        }

                        if (payload.ReceiverUserId == payload.SenderUserId)
                        {
                            _logger.LogWarning("[RabbitMQ] Notificação ignorada: SenderUserId é igual ao ReceiverUserId ({UserId}).", payload.ReceiverUserId);
                            return;
                        }

                        _logger.LogInformation("[RabbitMQ] Processando notificação para o usuário: {UserId}", payload.ReceiverUserId);

                        await _hubContext.Clients.Group($"user:{payload.ReceiverUserId}")
                                                    .SendAsync("ReceiveNotification", payload, cancellationToken: stoppingToken);

                        string? deviceToken = null;
                        string languageCode = "pt"; // Idioma fallback (padrão)

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                            var userInfo = await context.Set<User>()
                                .Where(u => u.Id == payload.ReceiverUserId)
                                .Select(u => new { u.DeviceToken, u.LanguageCode })
                                .FirstOrDefaultAsync(stoppingToken);

                            if (userInfo != null)
                            {
                                deviceToken = userInfo.DeviceToken;
                                languageCode = userInfo.LanguageCode ?? "pt";
                            }
                        }

                        if (string.IsNullOrEmpty(deviceToken))
                            _logger.LogWarning("[RabbitMQ] Notificação para o usuário {UserId} não possui DeviceToken. Ignorando envio de push.", payload.ReceiverUserId);
                        else
                            await EnviarPushNotificationFirebaseAsync(deviceToken, languageCode, payload, stoppingToken);

                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag,
                                                    multiple: false,
                                                    cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RabbitMQ] Erro ao processar e despachar a mensagem.");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(queue: _queueName,
                                                autoAck: false,
                                                consumer: consumer,
                                                cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[RabbitMQ] Falha fatal no Worker de Notificações.");
            }
        }

        private async Task EnviarPushNotificationFirebaseAsync(string deviceToken, string languageCode, NotificationDTO payload, CancellationToken stoppingToken)
        {
            try
            {
                bool isEnglish = languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase);

                string title;
                string body;

                switch (payload.Type)
                {
                    case "PostLiked":
                        title = isEnglish ? "New Like" : "Nova Curtida";
                        body = isEnglish ? "Someone liked your post!" : "Alguém curtiu sua publicação!";
                        break;
                    case "CommentAdded":
                        title = isEnglish ? "New Comment" : "Novo Comentário";
                        body = isEnglish ? "Someone commented on your post!" : "Alguém comentou na sua publicação!";
                        break;
                    default:
                        title = "Rediter";
                        body = isEnglish ? "You have a new notification!" : "Você tem uma nova notificação!";
                        break;
                }

                var message = new Message()
                {
                    Token = deviceToken,

                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = title,
                        Body = body
                    },

                    Data = new Dictionary<string, string>()
                    {
                        { "notificationId", payload.Id.ToString() },
                        { "type", payload.Type ?? "new_interaction" }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message, stoppingToken);
                _logger.LogInformation("[Firebase] Push enviado com sucesso. MessageID: {Response}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Firebase] Erro ao enviar Push Notification para o token {Token}", deviceToken);
            }
        }
    }
}