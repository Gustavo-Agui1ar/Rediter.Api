using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class PictureService : BaseService<Picture>
    {
        private readonly PictureRepository _pictureRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PictureService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public PictureService(
            PictureRepository pictureRepository,
            IWebHostEnvironment env,
            ILogger<PictureService> logger,
            IHttpClientFactory httpClientFactory) : base(pictureRepository)
        {
            _pictureRepository = pictureRepository;
            _env = env;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private string GetUploadsDirectory()
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        public async Task<Picture?> CreateImageFromGoogle(string imageUrl)
        {
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
                _logger.LogError(ex, "[PictureService] Error creating image from Google.");
                throw new Exception("Error creating image from Google: " + ex.Message);
            }
        }

        public async Task<Picture> CreatePicture(IFormFile file)
        {
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

                return picture;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (fileSavedToDisk && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _logger.LogError(ex, "[PictureService] Error creating picture.");
                throw new Exception("Erro ao salvar imagem: " + ex.Message);
            }
        }

        public async Task<(Stream? Stream, string? ContentType)> GetPictureStream(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (null, null);

            var fileName = Path.GetFileName(name);
            var folder = GetUploadsDirectory(); 
            var path = Path.Combine(folder, fileName);

            if (!File.Exists(path))
            {
                _logger.LogWarning("[PictureService] Imagem não encontrada no disco: {Path}", path);
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

            return (fileStream, contentType);
        }

        public async Task CleanUnusedImages()
        {
            var unusedPictures = await _pictureRepository.GetUnusedPictures();
            var folder = GetUploadsDirectory(); 

            foreach (var pic in unusedPictures)
            {
                var path = Path.Combine(folder, pic.FileName);

                if (File.Exists(path))
                    File.Delete(path);

                await _pictureRepository.Delete(pic);
            }
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