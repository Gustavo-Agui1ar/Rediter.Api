using Microsoft.EntityFrameworkCore;
using Rediter.Api.Models;
using Rediter.Api.Repositories;

namespace Rediter.Api.Services
{
    public class PictureService
    {
        private readonly PictureRepository _pictureRepository;

        public PictureService(PictureRepository pictureRepository)
        {
            _pictureRepository = pictureRepository;
        }

        public async Task<Picture> CreatePicture(IFormFile file)
        {
            try
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(folder);

                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Picture picture = new Picture
                {
                    FileName = fileName,
                    StoragePath = filePath,
                    MimeType = file.ContentType,
                    Size = (int)file.Length
                };

                await _pictureRepository.Insert(picture);

                return picture;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao salvar imagem: " + ex.Message);
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
