using Microsoft.Extensions.Hosting;
using Rediter.Api.Services;

public class ImageCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ImageCleanupBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pictureService = scope.ServiceProvider
                    .GetRequiredService<PictureService>();

                await pictureService.CleanUnusedImages();
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}