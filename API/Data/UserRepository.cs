using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;


namespace API.Data;
public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
    {
        var query = _context.Users
        .Where(x => x.UserName == username)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        .AsQueryable();

        if(isCurrentUser){
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = _context.Users.AsQueryable();
        query = query.Where(x => x.UserName != userParams.CurrentUsername);
        query = query.Where(x => x.Gender == userParams.Gender);

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch{
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
         userParams.PageNumber, userParams.PageSize);
    }


    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByNameAsync(string username)
    {
        return await _context.Users.Include(p=>p.Photos).SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<AppUser> GetUserByPhotoId(int photoId)
    {
        return await _context.Users.Include(u=>u.Photos).IgnoreQueryFilters()
        .Where(u=>u.Photos.Any(p=>p.Id == photoId)).FirstOrDefaultAsync();

    }


    public async Task<string> GetUserGender(string username)
    {
        return await _context.Users.Where(x=>x.UserName == username).Select(x=>x.Gender).FirstOrDefaultAsync();
    }


    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users.Include(p=>p.Photos).ToListAsync();
    }

    public void update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }
}
