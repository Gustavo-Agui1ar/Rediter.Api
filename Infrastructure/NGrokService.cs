using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Rediter.Api.Infrastructure;

public class NgrokHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly HttpClient _httpClient;
    private Process? _ngrokProcess; 

    public NgrokHostedService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
        _httpClient = new HttpClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return;

        Console.WriteLine("[NgrokService] Iniciando processo do Ngrok em background...");
        StartNgrokProcess();

        Console.WriteLine("[NgrokService] Consultando túnel ativo e configurando automação do Gist...");

        var publicUrl = await GetNgrokPublicUrlAsync(cancellationToken);

        if (!string.IsNullOrEmpty(publicUrl))
        {
            await UpdateGistAsync(publicUrl);
        }
        else
        {
            Console.WriteLine("[NgrokService] Aviso: Nenhum túnel Ngrok ativo foi encontrado na porta 4040 após várias tentativas.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return;

        Console.WriteLine("[SHUTDOWN] API desligando. Marcando Gist como OFFLINE...");

        await UpdateGistAsync("OFFLINE");

        if (_ngrokProcess != null && !_ngrokProcess.HasExited)
        {
            Console.WriteLine("[NgrokService] Encerrando o processo invisível do Ngrok...");
            _ngrokProcess.Kill();
            _ngrokProcess.Dispose();
        }
    }

    private void StartNgrokProcess()
    {
        try
        {
            var ngrokPath = _configuration["NGrok:ExecutablePath"] ?? "ngrok";
            var appPort = _configuration["NGrok:TargetPort"] ?? "6969";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ngrokPath,
                Arguments = $"http {appPort}",
                CreateNoWindow = true, 
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _ngrokProcess = Process.Start(processStartInfo);
            Console.WriteLine($"[NgrokService] Ngrok iniciado com sucesso (PID: {_ngrokProcess?.Id}).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NgrokService] Falha ao tentar iniciar o processo do Ngrok automaticamente. Verifique o caminho no appsettings. Erro: {ex.Message}");
        }
    }

    private async Task<string?> GetNgrokPublicUrlAsync(CancellationToken cancellationToken)
    {
        int maxRetries = 5;
        int delayMilliseconds = 2000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var ngrokResponse = await _httpClient.GetAsync("http://127.0.0.1:4040/api/tunnels", cancellationToken);

                if (ngrokResponse.IsSuccessStatusCode)
                {
                    var ngrokJson = await ngrokResponse.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(ngrokJson);

                    var tunnels = doc.RootElement.GetProperty("tunnels");

                    if (tunnels.GetArrayLength() > 0)
                    {
                        var publicUrl = tunnels[0].GetProperty("public_url").GetString();
                        return publicUrl?.Replace("http://", "https://");
                    }
                    else
                    {
                        Console.WriteLine($"[NgrokService] Ngrok respondeu, mas os túneis ainda não estão prontos. Tentativa {i + 1}...");
                    }
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine($"[NgrokService] Aguardando o Ngrok abrir a porta local... Tentativa {i + 1} de {maxRetries}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NgrokService] Erro inesperado ao ler a API local do Ngrok: {ex.Message}");
                return null;
            }

            await Task.Delay(delayMilliseconds, cancellationToken);
        }

        return null;
    }

    private async Task UpdateGistAsync(string urlOrStatus)
    {
        try
        {
            var gistId = _configuration["NGrok:Gist"]!;
            var githubToken = _configuration["NGrok:Token"]!;

            var gistContent = new
            {
                files = new Dictionary<string, object>
                {
                    {
                        "rediter_config.json", new
                        {
                            content = $"{{\n  \"apiUrl\": \"{urlOrStatus}\"\n}}"
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://api.github.com/gists/{gistId}");
            request.Headers.Add("Authorization", $"Bearer {githubToken}");
            request.Headers.Add("User-Agent", "Rediter-API-Config-Updater");
            request.Content = new StringContent(JsonSerializer.Serialize(gistContent), Encoding.UTF8, "application/json");

            var githubResponse = await _httpClient.SendAsync(request);

            if (githubResponse.IsSuccessStatusCode)
                Console.WriteLine($"[NgrokService] Gist sincronizado com sucesso -> Status: {urlOrStatus}");
            else
                Console.WriteLine($"[NgrokService] Erro ao sincronizar o Gist com o GitHub: {githubResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NgrokService] Falha de comunicação com a API do GitHub: {ex.Message}");
        }
    }
}