using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rediter.Api.Hubs;
using Rediter.Api.Models;

namespace Rediter.Api.Services.Workers
{
    public class NotificationListener : BackgroundService
    {
        private readonly IConnection _rabbitConnection;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationListener> _logger;
        private readonly string _queueName = "notificacoes_fila";

        public NotificationListener(
            IConnection rabbitConnection,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationListener> logger)
        {
            _rabbitConnection = rabbitConnection;
            _hubContext = hubContext;
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
                        }

                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag,
                                                    multiple: false,
                                                    cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RabbitMQ] Erro ao processar e despachar a mensagem.");

                        // await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
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
    }
}