using RabbitMQ.Client;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using System.Text;
using System.Text.Json;

namespace Rediter.Api.Services.Dispatchers
{
    public class RabbitMQNotificationDispatcher : INotificationDispatcher
    {
        private readonly IConnection _rabbitConnection;
        private readonly UserRepository _userRepository;
        private readonly string _queueName = "notificacoes_fila";

        public RabbitMQNotificationDispatcher(IConnection rabbitConnection, UserRepository userRepository)
        {
            _rabbitConnection = rabbitConnection;
            _userRepository = userRepository;
        }

        public async Task DispatchToUserAsync(Guid userId, NotificationDTO notificationDto)
        {
            var sender = await _userRepository.GetByUuid(notificationDto.SenderUserId);
            notificationDto.SenderUsername = sender?.Name ?? "Alguém";

            var mensagemJson = JsonSerializer.Serialize(notificationDto);
            var body = Encoding.UTF8.GetBytes(mensagemJson);

            using var channel = await _rabbitConnection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                                 queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            await channel.BasicPublishAsync(
                                 exchange: "",
                                 routingKey: _queueName,
                                 body: body);

            await Task.CompletedTask;
        }
    }
}