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
        private readonly PictureService _pictureService;
        private readonly PostUserLikeService _postUserLikeService;

        public PostService(PostRepository postrepository, PictureService pictureService, PostUserLikeService postUserLikeService) : base(postrepository)
        {
            _postrepository = postrepository;
            _pictureService = pictureService;
            _postUserLikeService = postUserLikeService;
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
                post.ParentPostId = null;

                if (dto.ParentPostId != null)
                {
                    post.ParentPostId = new Guid(dto.ParentPostId);

                    Post? parentPost = await _postrepository.GetByUuid(post.ParentPostId.Value);
                   
                    if (parentPost != null)
                    {
                        parentPost.CommentsCount++;
                        await _postrepository.Update(parentPost);
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

        public async Task<IList<PostFeedDTO>> GetPostsByUser(Guid userUuid, DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid currentUserId)
        {
            return await _postrepository.GetUserFeedAsync(userUuid, lastCreatedAt, lastId, pageSize, currentUserId);
        }

        public async Task<IList<PostFeedDTO>> SearchPosts(string query, DateTime? lastCreatedAt, Guid? lastId, int pageSize, bool onlyWithMedia = false, Guid? currentUserId = null)
        {
            return await _postrepository.SearchPosts(query, lastCreatedAt, lastId, pageSize, onlyWithMedia, currentUserId: currentUserId, fetchChildPosts: false);
        }

        public async Task<IList<PostFeedDTO>> SearchComments(DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid postParentId, bool onlyWithMedia = false, Guid? currentUserId = null)
        {
            return await _postrepository.SearchPosts("", lastCreatedAt, lastId, pageSize, parentPostID: postParentId, currentUserId: currentUserId, fetchChildPosts: false);
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
            return await _postrepository.GetAllMidiaNames(new Guid(userId));
        }

        public async Task LikePost(string postId, string userId)
        {
            Guid parsedPostId = Guid.Parse(postId);
            Guid parsedUserId = Guid.Parse(userId);

            Post? post = await _postrepository.GetByUuid(parsedPostId);

            if (post == null)
                throw new Exception("Post não encontrado");

            bool alreadyLiked = post.Likes.Any(l => l.UserId == parsedUserId);

            if (alreadyLiked)
                return;

            UserPostLike like = new UserPostLike
            {
                PostId = post.Id,
                UserId = parsedUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _postUserLikeService.InsertAsync(like);
            await _postrepository.IncrementLikesCount(post.Id);
        }

        public async Task UnlikePost(string postId, string userId)
        {
            try
            {
                Post? post = await _postrepository.GetByUuid(new Guid(postId));
                if (post == null)
                    throw new Exception("Post não encontrado");

                Guid parsedUserId = Guid.Parse(userId);
                var like = post.Likes.FirstOrDefault(l => l.UserId == parsedUserId);

                if (like == null)
                    return;

                post.Likes.Remove(like);
                post.LikesCount--;
                await _postrepository.Update(post);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<PostFeedDTO?> GetPostById(string postId, string userUuid)
        {
            return await _postrepository.GetPostById(new Guid(postId), new Guid(userUuid));
        }

        public async Task AddComment(string postId, NewPostDTO dto, string userUuid)
        {
            try
            {
                await NewPost(dto, userUuid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}