using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Rediter.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            Console.WriteLine($"User connected: {userId}");

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"user:{userId}"
            );

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            Console.WriteLine($"User disconnected: {userId}");

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"user:{userId}"
            );

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChatGroup(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task SendTyping(Guid chatId, bool isTyping)
        {
            await Clients.GroupExcept(chatId.ToString(), Context.ConnectionId)
                         .SendAsync("ReceiveTyping", chatId, isTyping);
        }
    }
}