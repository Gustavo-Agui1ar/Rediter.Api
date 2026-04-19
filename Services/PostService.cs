using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;
using System.Diagnostics;

namespace Rediter.Api.Services
{
    public class PostService : BaseService<Post>
    {
        private readonly PostRepository _postrepository;
        private readonly UserService _userservice;
        private readonly PictureService _pictureService;

        public PostService(PostRepository postrepository, PictureService pictureService, UserService userService) : base(postrepository)
        {
            _postrepository = postrepository;
            _pictureService = pictureService;
            _userservice = userService;
        }

        public async Task NewPost(NewPostDTO dto)
        {
            using (var transaction = await _postrepository.BeginTransaction())
            {
                try
                {
                    Post post = new Post();

                    post.User = await _userservice.GetUserByRefreshToken(dto.RefreshToken);
                    post.CreatedAt = DateTime.Now;
                    post.UpdatedAt = DateTime.Now;
                    post.Content = dto.Text;
                    post.LocationName = dto.LocationName;

                    if (dto.Pictures != null && dto.Pictures.Count > 0)
                    {
                        int order = 0;
                        foreach (IFormFile picture in dto.Pictures)
                        {
                            Picture pic = await _pictureService.CreatePicture(picture);

                            PostImage pi = new PostImage
                            {
                                PictureId = pic.Id,
                                DisplayOrder = order++,
                                CreatedAt = DateTime.Now,
                            };

                            post.PostImages.Add(pi);
                        }
                    }
                    await _postrepository.Insert(post);
                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}