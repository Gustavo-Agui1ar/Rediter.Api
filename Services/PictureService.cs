using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Rediter.Api.Services
{
    public class PictureService : BaseService<Picture>
    {
        private readonly PictureRepository _pictureRepository;

        public PictureService(PictureRepository pictureRepository) : base(pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }

        public async Task<Picture?> CreateImageFromGoogle(string imageUrl)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(imageUrl);
                    response.EnsureSuccessStatusCode();

                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    var fileExtension = contentType switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        _ => throw new Exception("Unsupported image type.")
                    };

                    var fileName = Guid.NewGuid().ToString() + fileExtension;

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
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating image from Google: " + ex.Message);
            }
        }

        public async Task<Picture> CreatePicture(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, fileName);
            bool fileSavedToDisk = false;

            using (var transaction = await _pictureRepository.BeginTransaction())
            {
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        fileSavedToDisk = true;
                    }

                    Picture picture = new Picture
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

                    if (fileSavedToDisk && System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    throw new Exception("Erro ao salvar imagem: " + ex.Message);
                }
            }
        }

        public async Task<(Stream? Stream, string? ContentType)> GetPictureStream(string? name, int compressionLevel = 100)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (null, null);

            var fileName = Path.GetFileName(name);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            if (!File.Exists(path))
                return (null, null);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
                contentType = "application/octet-stream";

            if (!contentType.StartsWith("image/"))
            {
                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                return (fileStream, contentType);
            }

            var extension = Path.GetExtension(path).ToLowerInvariant();
            var outputStream = new MemoryStream();
            string finalContentType;

            using (var image = await Image.LoadAsync(path))
            {
                switch (extension)
                {
                    case ".png":
                        await CompressPngAsync(image, outputStream);
                        finalContentType = "image/png";
                        break;
                    case ".jpg":
                    case ".jpeg":
                        await CompressJpegAsync(image, outputStream, compressionLevel);
                        finalContentType = "image/jpeg";
                        break;
                    default:
                        throw new ArgumentException("Unsupported image format");
                }
            }

            outputStream.Position = 0;

            return (outputStream, finalContentType);
        }

        private async Task CompressPngAsync(Image image, Stream outputStream)
        {
            var encoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression,
                IgnoreMetadata = true
            };

            await image.SaveAsPngAsync(outputStream, encoder);
        }

        private async Task CompressJpegAsync(Image image, Stream outputStream, int compressionLevel)
        {
            var encoder = new JpegEncoder
            {
                Quality = Math.Clamp(compressionLevel, 1, 100)
            };

            await image.SaveAsJpegAsync(outputStream, encoder);
        }

        public async Task CleanUnusedImages()
        {
            var unusedPictures = await _pictureRepository.GetUnusedPictures();

            foreach (var pic in unusedPictures)
            {
                var path = Path.Combine("wwwroot/uploads", pic.FileName);

                if (File.Exists(path))
                    File.Delete(path);

                await _pictureRepository.Delete(pic);
            }
        }

    }
}
