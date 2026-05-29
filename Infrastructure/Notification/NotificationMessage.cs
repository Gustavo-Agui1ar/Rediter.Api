using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Hubs;
using Rediter.Api.Models.Chats;
using Rediter.Api.Models.Users;
using System.Text;
using System.Text.Json;

namespace Rediter.Api.Infrastructure.Notifications
{
    public class ChatMessageListener : BackgroundService
    {
        private readonly IConnection _rabbitConnection;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ChatMessageListener> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _queueName = "chat_mensagens_fila";

        public ChatMessageListener(
            IConnection rabbitConnection,
            IHubContext<NotificationHub> hubContext,
            IServiceScopeFactory scopeFactory,
            ILogger<ChatMessageListener> logger)
        {
            _rabbitConnection = rabbitConnection;
            _hubContext = hubContext;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[RabbitMQ] Iniciando o consumo de mensagens de chat em background...");

            try
            {
                await using var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(queue: _queueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null,
                                                cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);

                        _logger.LogInformation("[RabbitMQ] Mensagem recebida na fila. Payload: {Json}", json);

                        using var doc = JsonDocument.Parse(json);
                        var chatId = doc.RootElement.GetProperty("ChatId").GetGuid();
                        var messageDtoJson = doc.RootElement.GetProperty("Message").GetRawText();
                        var messageDto = JsonSerializer.Deserialize<MessageDTO>(messageDtoJson);
                        var receiverElement = doc.RootElement.GetProperty("ReceiverId");
                        Guid? receiverId = receiverElement.ValueKind != JsonValueKind.Null ? receiverElement.GetGuid() : null;

                        if (messageDto != null && receiverId.HasValue)
                        {
                            _logger.LogInformation("[RabbitMQ] Processando mensagem do chat: {ChatId}", chatId);

                            bool isProprioUsuario = false;
                            string? deviceToken = null;

                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                                var senderId = await context.Set<Models.Chats.Message>()
                                    .Where(m => m.Id == messageDto.messageId)
                                    .Select(m => m.SenderId)
                                    .FirstOrDefaultAsync(stoppingToken);

                                if (senderId == receiverId.Value)
                                    isProprioUsuario = true;
                                else
                                {
                                    deviceToken = await context.Set<User>()
                                        .Where(u => u.Id == receiverId.Value)
                                        .Select(u => u.DeviceToken)
                                        .FirstOrDefaultAsync(stoppingToken);
                                }
                            }

                            if (isProprioUsuario)
                                _logger.LogInformation("[RabbitMQ] O destinatário é o próprio remetente. SignalR e Push cancelados.");
                            else
                            {
                                _logger.LogInformation("[SignalR] Disparando evento para o grupo user:{ReceiverId}", receiverId.Value);
                                await _hubContext.Clients.Group($"user:{receiverId.Value}")
                                    .SendAsync("ReceiveMessage", chatId.ToString(), messageDto, cancellationToken: stoppingToken);

                                if (!string.IsNullOrEmpty(deviceToken))
                                {
                                    _logger.LogInformation("[Firebase] DeviceToken encontrado para {UserId}. Enviando Push...", receiverId.Value);
                                    await EnviarPushNotificationFirebaseAsync(deviceToken, chatId, messageDto, stoppingToken);
                                }
                                else
                                    _logger.LogWarning("[Firebase] O usuário {UserId} NÃO possui DeviceToken no banco. Push ignorado.", receiverId.Value);
                            }
                        }
                        else
                            _logger.LogWarning("[RabbitMQ] ⚠️ MessageDTO ou ReceiverId inválidos no payload.");

                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        _logger.LogInformation("[RabbitMQ] Processamento finalizado com sucesso (ACK).");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RabbitMQ] Erro interno ao repassar mensagem de chat.");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[RabbitMQ] Falha fatal no Worker de Chat.");
            }
        }

        private async Task EnviarPushNotificationFirebaseAsync(string deviceToken, Guid chatId, MessageDTO message, CancellationToken stoppingToken)
        {
            try
            {
                var pushMessage = new FirebaseAdmin.Messaging.Message()
                {
                    Token = deviceToken,
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = "Nova mensagem",
                        Body = message.content.Length > 50 ? message.content.Substring(0, 47) + "..." : message.content
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "chatId", chatId.ToString() },
                        { "messageId", message.messageId.ToString() },
                        { "type", "new_chat_message" } 
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(pushMessage, stoppingToken);
                _logger.LogInformation("[Firebase] Push de chat enviado. MessageID: {Response}", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Firebase] Erro ao enviar Push de chat para o token {Token}", deviceToken);
            }
        }
    }
}