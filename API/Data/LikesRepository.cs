using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;
public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;


    public LikesRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
    {
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if(likesParams.Predicate == "liked"){
            likes=likes.Where(l => l.SourceUserId == likesParams.UserId);
            users = likes.Select(l => l.TargetUser);
        } 

        if(likesParams.Predicate == "likedBy"){
            likes=likes.Where(l => l.TargetUserId == likesParams.UserId);
            users = likes.Select(l => l.SourceUser);
        }

        var likedUsers = users.Select(u => new LikeDto{
            UserName = u.UserName,
            KnownAs = u.KnownAs,
            Age = u.DateOfBirth.CalculateAge(),
            PhotoUrl = u.Photos.FirstOrDefault(x=>x.IsMain).Url,
            Id = u.Id
        });

        return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.pageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
        return await _context.Users.Include(x=>x.LikedUsers).FirstOrDefaultAsync(x=>x.Id == userId);
    }
}
