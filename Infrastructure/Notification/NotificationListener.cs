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

                        if (payload != null)
                        {
                            _logger.LogInformation("[RabbitMQ] Processando notificação para o usuário: {UserId}", payload.ReceiverUserId);

                            await _hubContext.Clients.Group($"user:{payload.ReceiverUserId}")
                                                     .SendAsync("ReceiveNotification", payload, cancellationToken: stoppingToken);

                            string? deviceToken = null;
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                                deviceToken = await context.Set<User>()
                                    .Where(u => u.Id == payload.ReceiverUserId)
                                    .Select(u => u.DeviceToken)
                                    .FirstOrDefaultAsync(stoppingToken); 
                            }

                            if (string.IsNullOrEmpty(deviceToken))
                            {
                                _logger.LogWarning("[RabbitMQ] Notificação para o usuário {UserId} não possui DeviceToken. Ignorando envio de push.", payload.ReceiverUserId);
                            }
                            else
                            {
                                await EnviarPushNotificationFirebaseAsync(deviceToken, payload, stoppingToken);
                            }
                        }

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

        private async Task EnviarPushNotificationFirebaseAsync(string deviceToken, NotificationDTO payload, CancellationToken stoppingToken)
        {
            try
            {
                var message = new Message()
                {
                    Token = deviceToken,
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = "Rediter",
                        Body = "Você tem uma nova notificação!"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "notificationId", payload.Id.ToString() },
                        { "type", "new_interaction" }
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