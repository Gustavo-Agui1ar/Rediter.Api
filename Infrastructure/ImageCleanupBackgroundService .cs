using Rediter.Api.Services;


namespace Rediter.Api.Infrastructure
{

public class ImageCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageCleanupBackgroundService> _logger;

    public ImageCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<ImageCleanupBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var pictureService = scope.ServiceProvider
                        .GetRequiredService<PictureService>();
                    await pictureService.CleanUnusedImages();
                    _logger.LogInformation("Limpeza de imagens não utilizadas concluída com sucesso.");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker do ImageCleanup interrompido com sucesso para desligamento.");
        }
    }
}
}