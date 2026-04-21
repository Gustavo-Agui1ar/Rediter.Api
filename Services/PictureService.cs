using Microsoft.EntityFrameworkCore;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

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

        public async Task<Stream?> GetPictureStream(string? name)
        {

            if (string.IsNullOrEmpty(name))
                return null;

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", name);

            if (!File.Exists(path))
                return null;

            return new FileStream(path, FileMode.Open, FileAccess.Read);
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
