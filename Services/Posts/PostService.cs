using Rediter.Api.DTOs.Posts;
using Rediter.Api.Models;
using Rediter.Api.Models.Posts;
using Rediter.Api.Repositories.Posts;
using Rediter.Api.Services.UtilitariesServices;
using System.Diagnostics;
using System.Transactions;

namespace Rediter.Api.Services.Posts
{
    public class PostService : BaseService<Post>
    {
        private readonly PostRepository _postrepository;
        private readonly PictureService _pictureService;

        public PostService(
            PostRepository postrepository,
            PictureService pictureService) : base(postrepository)
        {
            _postrepository = postrepository;
            _pictureService = pictureService;
        }

        public async Task NewPost(NewPostDTO dto, Guid userUuid)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    Post post = new Post
                    {
                        UserId = userUuid,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Content = dto.Text,
                        LocationName = dto.LocationName,
                        ParentPostId = null
                    };


                    if (!string.IsNullOrEmpty(dto.ParentPostId))
                    {
                        post.ParentPostId = new Guid(dto.ParentPostId);
                        Post? parentPost = await _postrepository.GetByUuid(post.ParentPostId.Value);

                        if (parentPost != null)
                        {
                            parentPost.CommentsCount++;
                            _postrepository.Update(parentPost);
                        }
                    }

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
                                CreatedAt = DateTime.UtcNow,
                            };
                            post.PostImages.Add(pi);
                        }
                    }

                    _postrepository.Insert(post);

                    await SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw new Exception("Erro ao criar o post: " + ex.Message);
                }
            }
        }

        public async Task UpdatePost(UpdatePostDTO dto, string postId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    Post? post = await _postrepository.GetByUuid(new Guid(postId));

                    if (post == null)
                        throw new Exception("Post não encontrado");

                    post.Content = dto.Text;
                    post.LocationName = dto.LocationName;
                    post.UpdatedAt = DateTime.UtcNow;

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
                                CreatedAt = DateTime.UtcNow,
                            };

                            post.PostImages.Add(pi);
                        }
                    }

                    _postrepository.Update(post);

                    await SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao atualizar o post: " + ex.Message);
                }
            }
        }

        public async Task DeletePost(Guid postId, Guid currentUserId, bool isAdmin)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    Post? post = await _postrepository.GetByUuid(postId);

                    if (post == null)
                        throw new Exception("Post não encontrado");

                    if (!isAdmin && post.UserId != currentUserId)
                        throw new Exception("Usuário não tem permissão para deletar este post");

                    _postrepository.Delete(post);

                    await SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocorreu um erro ao deletar o post: " + ex.Message);
                }
            }
        }
        public async Task<IList<PostFeedDTO>> GetPostsByUser(Guid userUuid, DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid currentUserId) => await _postrepository.GetUserFeedAsync(userUuid, lastCreatedAt, lastId, pageSize, currentUserId);
        public async Task<IList<PostFeedDTO>> SearchPosts(string query, DateTime? lastCreatedAt, Guid? lastId, int pageSize, bool onlyWithMedia = false, Guid? currentUserId = null) => await _postrepository.SearchPosts(query, lastCreatedAt, lastId, pageSize, onlyWithMedia, currentUserId: currentUserId, fetchChildPosts: false);
        public async Task<IList<PostFeedDTO>> SearchComments(DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid postParentId, bool onlyWithMedia = false, Guid? currentUserId = null) => await _postrepository.SearchPosts("", lastCreatedAt, lastId, pageSize, parentPostID: postParentId, currentUserId: currentUserId, fetchChildPosts: false);
        public async Task<IList<string>> GetAllMidiaNames(Guid userId) => await _postrepository.GetAllMidiaNames(userId);
        public async Task<PostFeedDTO?> GetPostById(Guid postId, Guid userUuid) => await _postrepository.GetPostById(postId, userUuid);
        public async Task AddComment(Guid postId, NewPostDTO dto, Guid userUuid) => await NewPost(dto, userUuid);
        public async Task<IList<PostFeedDTO>> GetLikedPostsByUser(Guid currentUserId, DateTime? lastCreatedAt, Guid? lastId, int pageSize) => await _postrepository.GetLikedPostsByUserAsync(currentUserId, lastCreatedAt, lastId, pageSize);
        public async Task<IList<PostFeedDTO>> GetDiscoverPosts(Guid currentUserId, int? lastScore, DateTime? lastCreatedAt, Guid? lastId, int pageSize) => await _postrepository.GetDiscoverPosts(currentUserId, lastScore, lastCreatedAt, lastId, pageSize);
        public async Task<IList<PostFeedDTO>> GetFollowingPosts(Guid currentUserId, DateTime? lastCreatedAt, Guid? lastId, int pageSize) => await _postrepository.GetFollowingPosts(currentUserId, lastCreatedAt, lastId, pageSize);
    }
}