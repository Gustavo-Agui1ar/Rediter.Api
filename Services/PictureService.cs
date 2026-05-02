using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;
using Microsoft.Extensions.Configuration;

namespace Rediter.Api.Services
{
    public class PictureService : BaseService<Picture>
    {
        private readonly PictureRepository _pictureRepository;
        private readonly ILogger<PictureService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public PictureService(
            PictureRepository pictureRepository,
            ILogger<PictureService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration) : base(pictureRepository)
        {
            _pictureRepository = pictureRepository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        private string GetUploadsDirectory()
        {
            string? folder;

            if (OperatingSystem.IsWindows())
            {
                folder = _configuration["UploadSettings:StoragePathWindows"];
                _logger.LogDebug("[PictureService] Sistema operacional Windows detectado. Caminho configurado: {Folder}", folder);
            }
            else
            {
                folder = _configuration["UploadSettings:StoragePathLinux"];
                _logger.LogDebug("[PictureService] Sistema operacional não-Windows detectado. Caminho configurado: {Folder}", folder);
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                var currentDir = Directory.GetCurrentDirectory();
                var parentDir = Directory.GetParent(currentDir)?.FullName ?? currentDir;
                folder = Path.Combine(parentDir, "RediterUploads");

                _logger.LogWarning("[PictureService] Caminho de upload não configurado no appsettings. Usando caminho de fallback: {Folder}", folder);
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                _logger.LogInformation("[PictureService] Diretório de uploads criado: {Folder}", folder);
            }

            return folder;
        }

        public async Task<Picture?> CreateImageFromGoogle(string imageUrl)
        {
            _logger.LogInformation("[PictureService] Iniciando download da imagem do Google a partir da URL: {ImageUrl}", imageUrl);

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                var fileExtension = contentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => throw new Exception($"Unsupported image type: {contentType}")
                };

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                _logger.LogDebug("[PictureService] Imagem baixada com sucesso. Content-Type: {ContentType}. Nome gerado: {FileName}", contentType, fileName);

                using var internetStream = await response.Content.ReadAsStreamAsync();
                using var memoryStream = new MemoryStream();
                await internetStream.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                IFormFile formFile = new FormFile(memoryStream, 0, memoryStream.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };

                return await CreatePicture(formFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PictureService] Erro ao criar imagem a partir do Google. URL: {ImageUrl}", imageUrl);
                throw new Exception("Error creating image from Google: " + ex.Message);
            }
        }

        public async Task<Picture> CreatePicture(IFormFile file)
        {
            _logger.LogInformation("[PictureService] Iniciando salvamento de nova imagem. Arquivo original: {OriginalFileName}, Tamanho: {FileSize} bytes", file.FileName, file.Length);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var folder = GetUploadsDirectory();
            var filePath = Path.Combine(folder, fileName);

            bool fileSavedToDisk = false;

            using var transaction = await _pictureRepository.BeginTransaction();
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    fileSavedToDisk = true;
                    _logger.LogDebug("[PictureService] Arquivo salvo no disco com sucesso: {FilePath}", filePath);
                }

                var picture = new Picture
                {
                    FileName = fileName,
                    StoragePath = filePath,
                    MimeType = file.ContentType,
                    Size = (int)file.Length
                };

                await _pictureRepository.Insert(picture);
                await transaction.CommitAsync();

                _logger.LogInformation("[PictureService] Imagem {FileName} salva no banco de dados e transação comitada com sucesso.", fileName);

                return picture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PictureService] Erro durante a criação da imagem {FileName}. Iniciando Rollback.", fileName);

                await transaction.RollbackAsync();
                _logger.LogDebug("[PictureService] Rollback do banco de dados concluído para a imagem {FileName}.", fileName);

                if (fileSavedToDisk && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("[PictureService] Arquivo residual removido do disco após erro: {FilePath}", filePath);
                }

                throw new Exception("Erro ao salvar imagem: " + ex.Message);
            }
        }

        public async Task<(Stream? Stream, string? ContentType)> GetPictureStream(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogDebug("[PictureService] GetPictureStream chamado com nome nulo ou vazio.");
                return (null, null);
            }

            var fileName = Path.GetFileName(name);
            var folder = GetUploadsDirectory();
            var path = Path.Combine(folder, fileName);

            if (!File.Exists(path))
            {
                _logger.LogWarning("[PictureService] Imagem não encontrada no disco ao tentar carregar o stream: {Path}", path);
                return (null, null);
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = GetContentType(extension);

            var fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            _logger.LogDebug("[PictureService] Stream criado com sucesso para o arquivo: {Path} ({ContentType})", path, contentType);

            return (fileStream, contentType);
        }

        public async Task CleanUnusedImages()
        {
            _logger.LogInformation("[PictureService] Iniciando rotina de limpeza de imagens não utilizadas.");

            var unusedPictures = await _pictureRepository.GetUnusedPictures();

            if (!unusedPictures.Any())
            {
                _logger.LogInformation("[PictureService] Nenhuma imagem ociosa encontrada para limpeza.");
                return;
            }

            var folder = GetUploadsDirectory();
            int deletedCount = 0;

            foreach (var pic in unusedPictures)
            {
                var path = Path.Combine(folder, pic.FileName);

                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        _logger.LogDebug("[PictureService] Arquivo físico deletado: {Path}", path);
                    }
                    else
                    {
                        _logger.LogWarning("[PictureService] O arquivo físico da imagem {FileName} não foi encontrado durante a limpeza.", pic.FileName);
                    }

                    await _pictureRepository.Delete(pic);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[PictureService] Erro ao limpar a imagem {FileName} ({PictureId}).", pic.FileName, pic.Id);
                }
            }

            _logger.LogInformation("[PictureService] Limpeza de imagens concluída. Total de imagens removidas: {DeletedCount}", deletedCount);
        }

        private string GetContentType(string? extension)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(extension))
                    return "application/octet-stream";

                extension = extension.Trim().ToLowerInvariant();

                if (!extension.StartsWith("."))
                    extension = "." + extension;

                if (_contentTypeProvider.TryGetContentType($"file{extension}", out var contentType))
                {
                    return contentType;
                }

                return extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".bmp" => "image/bmp",
                    ".svg" => "image/svg+xml",

                    ".mp4" => "video/mp4",
                    ".mov" => "video/quicktime",
                    ".avi" => "video/x-msvideo",
                    ".mkv" => "video/x-matroska",

                    ".mp3" => "audio/mpeg",
                    ".wav" => "audio/wav",

                    ".pdf" => "application/pdf",
                    ".json" => "application/json",
                    ".txt" => "text/plain",

                    _ => "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[PictureService] Falha ao detectar Content-Type da extensão {Extension}", extension);
                return "application/octet-stream";
            }
        }
    }
}