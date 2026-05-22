using Microsoft.EntityFrameworkCore;
using Rediter.Api.DTOs;
using Rediter.Api.Interfaces;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;
using System.Transactions;

namespace Rediter.Api.Services
{
    public class PostUserLikeService : BaseService<UserPostLike>
    {
        private readonly UserPostLikeRepository _postUserLikeRepository;
        private readonly PostRepository _postRepository;
        private readonly NotificationService _notificationService;

        public PostUserLikeService(
            UserPostLikeRepository postUserLikeRepository,
            PostRepository postRepository,
            NotificationService notificationService) : base(postUserLikeRepository)
        {
            _postUserLikeRepository = postUserLikeRepository;
            _postRepository = postRepository;
            _notificationService = notificationService;
        }
        public async Task<bool> Exists(Guid postId, Guid userId)
        {
            return await _postUserLikeRepository.Exists(postId, userId);
        }

        public async Task LikePost(Guid postId, Guid userId)
        {
            Post? post = await _postRepository.GetByUuid(postId);
            if (post == null) throw new Exception("Post não encontrado");

            bool alreadyLiked = await Exists(postId, userId);
            if (alreadyLiked) return;

            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    UserPostLike like = new UserPostLike { PostId = post.Id, UserId = userId, CreatedAt = DateTime.UtcNow };
                    post.LikesCount++;

                    _postUserLikeRepository.Insert(like);
                    _postRepository.Update(post);

                    Notification notification = new Notification
                    {
                        RecipientUserId = post.UserId,
                        SenderUserId = userId,
                        PostId = post.Id,
                        Type = NotificationType.PostLiked,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _notificationService.Insert(notification);
                    await SaveChangesAsync();

                    scope.Complete();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocorreu um erro ao curtir o post: " + ex.Message);
                }
            }
        }

        public async Task UnlikePost(Guid postId, Guid userId)
        {
            try
            {
                Post? post = await _postRepository.GetByUuid(postId);

                if (post == null)
                    throw new Exception("Post não encontrado");

                UserPostLike? like =
                    await _postUserLikeRepository
                        .GetByPostAndUser(postId, userId);

                if (like == null)
                    return;

                _postUserLikeRepository.Delete(like);

                if (post.LikesCount > 0)
                    post.LikesCount--;

                _postRepository.Update(post);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Erro ao remover curtida: " + ex.Message);
            }
        }
    }
}