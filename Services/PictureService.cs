using Microsoft.AspNetCore.StaticFiles;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;
using System.Transactions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Collections.Concurrent;

namespace Rediter.Api.Services
{
    public class PictureService : BaseService<Picture>
    {
        
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _thumbLocks = new();
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
            _logger.LogInformation("[PictureService] Iniciando salvamento de nova arquivo. Original: {OriginalFileName}, Tamanho: {FileSize} bytes", file.FileName, file.Length);

            bool isImage = file.ContentType.StartsWith("image/");
            var fileExtension = isImage ? ".jpg" : Path.GetExtension(file.FileName);

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var folder = GetUploadsDirectory();

            var filePath = Path.Combine(folder, fileName);
            string? thumbPath = isImage ? Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(fileName)}_thumb.jpg") : null;

            bool filesSavedToDisk = false;

            using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                if (isImage)
                {
                    using var imageStream = file.OpenReadStream();
                    using var image = await Image.LoadAsync(imageStream);

                    image.Mutate(x =>
                    {
                        bool isLandscape = image.Width > image.Height;
                        bool isPortrait = image.Height > image.Width;
                        bool isSquare = image.Width == image.Height;

                        if (isSquare)
                        {
                            x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Crop,
                                Size = new Size(1080, 1080)
                            });
                        }
                        else if (isLandscape)
                        {
                            x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(1920, 1080)
                            });
                        }
                        else if (isPortrait)
                        {
                            x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(1080, 1920)
                            });
                        }
                    });

                    await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 80 });
                    _logger.LogDebug("[PictureService] Imagem principal otimizada e salva: {FilePath}", filePath);

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Crop,
                        Size = new Size(256, 256)
                    }));

                    await image.SaveAsJpegAsync(thumbPath!, new JpegEncoder { Quality = 70 });
                    _logger.LogDebug("[PictureService] Thumbnail criado e salvo: {ThumbPath}", thumbPath);

                    filesSavedToDisk = true;
                }
                else
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        filesSavedToDisk = true;
                        _logger.LogDebug("[PictureService] Arquivo salvo no disco com sucesso: {FilePath}", filePath);
                    }
                }

                var finalFileInfo = new FileInfo(filePath);

                var picture = new Picture
                {
                    FileName = fileName,
                    StoragePath = filePath,
                    MimeType = isImage ? "image/jpeg" : file.ContentType,
                    Size = (int)finalFileInfo.Length
                };

                _pictureRepository.Insert(picture);
                await SaveChangesAsync();
                scope.Complete();

                _logger.LogInformation("[PictureService] Imagem {FileName} salva no banco e transação comitada.", fileName);

                return picture;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PictureService] Erro durante a criação da imagem {FileName}. Iniciando Rollback.", fileName);

                if (filesSavedToDisk)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("[PictureService] Arquivo principal residual removido do disco: {FilePath}", filePath);
                    }

                    if (thumbPath != null && File.Exists(thumbPath))
                    {
                        File.Delete(thumbPath);
                        _logger.LogInformation("[PictureService] Thumbnail residual removido do disco: {ThumbPath}", thumbPath);
                    }
                }

                throw new Exception("Erro ao salvar imagem: " + ex.Message);
            }
        }

        public async Task<(Stream? Stream, string? ContentType)> GetPictureStream(string? name, bool isThumb)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogDebug("[PictureService] GetPictureStream chamado com nome nulo ou vazio.");
                return (null, null);
            }

            var originalFileName = Path.GetFileName(name);
            var folder = GetUploadsDirectory();
            string fileName = originalFileName;

            if (isThumb)
            {
                var extension = Path.GetExtension(originalFileName);
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                var thumbFileName = $"{fileWithoutExtension}_thumb{extension}";
                var thumbPath = Path.Combine(folder, thumbFileName);

                if (File.Exists(thumbPath))
                {
                    fileName = thumbFileName;
                }
                else
                {
                    var originalPath = Path.Combine(folder, originalFileName);

                    if (File.Exists(originalPath))
                    {
                        var fileLock = _thumbLocks.GetOrAdd(thumbPath, _ => new SemaphoreSlim(1, 1));

                        await fileLock.WaitAsync();
                        try
                        {
                            if (!File.Exists(thumbPath))
                            {
                                using var image = await Image.LoadAsync(originalPath);

                                image.Mutate(x =>
                                {
                                    bool isLandscape = image.Width > image.Height;
                                    bool isPortrait = image.Height > image.Width;
                                    bool isSquare = image.Width == image.Height;

                                    if (isSquare)
                                    {
                                        x.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(256, 256) });
                                    }
                                    else if (isLandscape)
                                    {
                                        x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(256, 144) });
                                    }
                                    else if (isPortrait)
                                    {
                                        x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(144, 256) });
                                    }
                                });

                                await image.SaveAsync(thumbPath);
                                _logger.LogInformation("[PictureService] Thumb criada automaticamente: {Path}", thumbPath);
                            }

                            fileName = thumbFileName;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[PictureService] Erro ao criar thumb automaticamente para {Original}", originalFileName);
                            fileName = originalFileName; 
                        }
                        finally
                        {
                            fileLock.Release();
                        }
                    }
                }
            }

            var path = Path.Combine(folder, fileName);

            if (!File.Exists(path))
            {
                _logger.LogWarning("[PictureService] Imagem não encontrada no disco ao tentar carregar o stream: {Path}", path);
                return (null, null);
            }

            var contentType = GetContentType(Path.GetExtension(fileName).ToLowerInvariant());

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
            int markedForDeletion = 0;

            foreach (var pic in unusedPictures)
            {
                var mainPath = Path.Combine(folder, pic.FileName);
                var thumbName = $"{Path.GetFileNameWithoutExtension(pic.FileName)}_thumb.jpg";
                var thumbPath = Path.Combine(folder, thumbName);

                try
                {
                    if (File.Exists(mainPath))
                    {
                        File.Delete(mainPath);
                        _logger.LogDebug("[PictureService] Arquivo físico deletado: {Path}", mainPath);
                    }
                    else
                    {
                        _logger.LogWarning("[PictureService] Arquivo físico {FileName} não encontrado durante a limpeza.", pic.FileName);
                    }

                    if (File.Exists(thumbPath))
                    {
                        File.Delete(thumbPath);
                        _logger.LogDebug("[PictureService] Thumbnail deletado: {ThumbPath}", thumbPath);
                    }

                    _pictureRepository.Delete(pic);
                    markedForDeletion++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[PictureService] Erro ao limpar a imagem {FileName} ({PictureId}).", pic.FileName, pic.Id);
                }
            }

            if (markedForDeletion > 0)
                await SaveChangesAsync();

            _logger.LogInformation("[PictureService] Limpeza concluída. Total de imagens (e thumbs) removidas: {DeletedCount}", markedForDeletion);
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
                    return contentType;

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