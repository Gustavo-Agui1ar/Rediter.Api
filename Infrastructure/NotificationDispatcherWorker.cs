public class NotificationDispatcherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationQueue _queue;

    public NotificationDispatcherWorker(
        IServiceScopeFactory scopeFactory,
        NotificationQueue queue)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out var item))
            {
                using var scope = _scopeFactory.CreateScope();

                var dispatcher =
                    scope.ServiceProvider
                        .GetRequiredService<INotificationDispatcher>();

                await dispatcher.DispatchToUserAsync(
                    item.userId,
                    item.dto);
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}