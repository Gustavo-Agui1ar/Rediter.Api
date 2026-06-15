using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Hubs;
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
                await channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (sender, ea) => await HandleMessageAsync(channel, ea, stoppingToken);

                await channel.BasicConsumeAsync(_queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "[RabbitMQ] Falha fatal no Worker de Chat.");
            }
        }

        private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken stoppingToken)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[RabbitMQ] Mensagem recebida na fila. Payload: {Json}", json);

                await ProcessNotificationAsync(json, stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                _logger.LogInformation("[RabbitMQ] Processamento finalizado com sucesso (ACK).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RabbitMQ] Erro interno ao repassar mensagem de chat.");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        }

        private async Task ProcessNotificationAsync(string json, CancellationToken stoppingToken)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var chatId = root.GetProperty("ChatId").GetGuid();
            var receiverElement = root.GetProperty("ReceiverId");
            var messageDto = JsonSerializer.Deserialize<MessageDTO>(root.GetProperty("Message").GetRawText());
            var receiverId = receiverElement.ValueKind != JsonValueKind.Null ? receiverElement.GetGuid() : (Guid?)null;

            if (receiverId == null || messageDto == null)
            {
                _logger.LogWarning("[RabbitMQ] Payload inválido: ReceiverId ou MessageDTO nulos.");
                return;
            }

            _logger.LogInformation("[RabbitMQ] Processando mensagem do chat: {ChatId}", chatId);
            await DispatchNotificationsAsync(chatId, receiverId.Value, messageDto, stoppingToken);
        }

        private async Task DispatchNotificationsAsync(Guid chatId, Guid receiverId, MessageDTO messageDto, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var senderId = await context.Set<Models.Chats.Message>()
                .Where(m => m.Id == messageDto.messageId)
                .Select(m => m.SenderId)
                .FirstOrDefaultAsync(stoppingToken);

            if (senderId == receiverId)
            {
                _logger.LogInformation("[RabbitMQ] O destinatário é o próprio remetente. SignalR e Push cancelados.");
                return;
            }

            _logger.LogInformation("[SignalR] Disparando evento para o grupo user:{ReceiverId}", receiverId);
            await _hubContext.Clients.Group($"user:{receiverId}").SendAsync("ReceiveMessage", chatId.ToString(), messageDto, cancellationToken: stoppingToken);

            var receiverInfo = await context.Set<User>()
                .Where(u => u.Id == receiverId)
                .Select(u => new { u.DeviceToken, u.LanguageCode, u.Name })
                .FirstOrDefaultAsync(stoppingToken);

            if (receiverInfo == null || string.IsNullOrEmpty(receiverInfo.DeviceToken))
            {
                _logger.LogWarning("[Firebase] O usuário {UserId} NÃO possui DeviceToken no banco. Push ignorado.", receiverId);
                return;
            }

            _logger.LogInformation("[Firebase] DeviceToken encontrado para {UserId}. Enviando Push...", receiverId);
            string languageCode = receiverInfo.LanguageCode ?? "pt";

            await EnviarPushNotificationFirebaseAsync(receiverInfo.DeviceToken, languageCode, chatId, receiverInfo.Name, messageDto.messageId, stoppingToken);
        }

        private async Task EnviarPushNotificationFirebaseAsync(string deviceToken, string languageCode, Guid chatId, string name, Guid messageId, CancellationToken stoppingToken)
        {
            try
            {
                bool isEnglish = languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase);

                var pushMessage = new Message()
                {
                    Token = deviceToken,
                    Notification = new Notification()
                    {
                        Title = isEnglish ? "New message" : "Nova mensagem",
                        Body = isEnglish ? $"You have a new message from {name}." : $"Você tem uma nova mensagem de {name}."
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "chatId", chatId.ToString() },
                        { "messageId", messageId.ToString() },
                        { "type", "new_chat_message" }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(pushMessage, stoppingToken);
                _logger.LogInformation("[Firebase] Push de chat enviado. MessageID: {Response}", response);
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex,
                    "Firebase error. Code={Code}",
                    ex.ErrorCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Firebase] Erro ao enviar Push de chat para o token {Token}", deviceToken);
            }
        }
    }
}