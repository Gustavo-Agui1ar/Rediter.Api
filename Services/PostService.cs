using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using System.Diagnostics;

namespace Rediter.Api.Services
{
    public class PostService
    {
        private readonly Postrepository _postrepository;
        private readonly UserService _userservice;
        private readonly PictureService _pictureservice;

        public PostService(Postrepository postrepository, PictureService pictureService, UserService userService)
        {
            _postrepository = postrepository;
            _pictureservice = pictureService;
            _userservice = userService;
        }

        public async Task NewPost(NewPostDTO dto)
        {
            var transaction = _postrepository.BeginTransaction();

            try
            {
                Post post = new Post();

                post.User = await _userservice.GetUserByRefreshToken(dto.RefreshToken);
                post.CreatedAt = DateTime.Now;
                post.UpdatedAt = DateTime.Now;
                post.Content = dto.Text;
                post.LocationName = dto.LocationName;
            
                await _postrepository.Insert(post);
                // TODO terminar questão de insersão de postimage ao criar servico e repository para o mesmo.
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}