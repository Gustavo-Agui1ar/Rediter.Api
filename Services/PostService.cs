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

        public async Task NewPost(NewPostDTO dto, string userUuid)
        {
                try
                {
                    Post post = new Post();

                    post.UserId = Guid.Parse(userUuid);
                    post.CreatedAt = 
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

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw new Exception(ex.Message);
                }
        }

        public async Task<IList<PostFeedDTO>> GetPostsByUser(string userUuid, DateTime? lastCreatedAt, string? lastId, int pageSize)
        {
            return await _postrepository.GetUserFeedAsync(userUuid, lastCreatedAt, lastId, pageSize);
        }

        public async Task<IList<PostFeedDTO>> SearchPosts(string query, DateTime? lastCreatedAt, string? lastId, int pageSize, bool onlyWithMedia = false)
        {
            return await _postrepository.SearchFeedAsync(query, lastCreatedAt, lastId, pageSize, onlyWithMedia);
        }

        public async Task UpdatePost(UpdatePostDTO dto, string postId)
        {
            try
            {
                Post? post = await _postrepository.GetByUuid(new Guid(postId));

                if (post == null)
                    throw new Exception("Post não encontrado");

                post.Content = dto.Text;
                post.LocationName = dto.LocationName;
                post.UpdatedAt = DateTime.Now;

                var retainedNames = dto.RetainedPictures ?? new List<string>();

                var imagesToRemove = post.PostImages
                    .Where(pi => pi.Picture != null && !retainedNames.Contains(pi.Picture.FileName))
                    .ToList();

                foreach (var piToRemove in imagesToRemove)
                {
                    post.PostImages.Remove(piToRemove);
                }

                if (dto.Pictures != null && dto.Pictures.Count > 0)
                {
                    int order = post.PostImages.Any() ? post.PostImages.Max(pi => pi.DisplayOrder) + 1 : 0;

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

                await _postrepository.Update(post);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeletePost(string postId)
        {
            using (var transaction = await _postrepository.BeginTransaction())
            {
                try
                {
                    Post? post = await _postrepository.GetByUuid(new Guid(postId));
                    
                    if (post == null)
                        throw new Exception("Post não encontrado");

                    await _postrepository.Delete(post);
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Ocorreu um erro ao deletar o post: " + ex.Message);
                }
            }
        }

        public async Task<IList<string>> GetAllMidiaNames(string userId)
        {
            return await _postrepository.GetAllMidiaNames(userId);
        }
    }
}