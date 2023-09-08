using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;
public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;


    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;

    }

    public void AddMessage(Message message)
    {
        _context.messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.messages.Remove(message);
    }

    public async Task<Message> GetMessage(int Id)
    {
        return await _context.messages.FindAsync(Id);
    }

    public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
    {
        var query = _context.messages.OrderByDescending(x => x.MessageSent).AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(u => u.Recepient.UserName == messageParams.Username && u.RecepientDeleted == false),
            "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
            _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.RecepientDeleted == false && u.DateRead == null),
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recepientUserName)
    {
        var messages = await _context.messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
                        .Include(u => u.Recepient).ThenInclude(p => p.Photos).Where(
                            m => m.RecipientUsername == currentUserName && m.RecepientDeleted == false && m.SenderUsername == recepientUserName ||
                                m.RecipientUsername == recepientUserName && m.SenderDeleted == false && m.SenderUsername == currentUserName
                        ).OrderBy(m => m.MessageSent).ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList();

        if(unreadMessages.Any()){
            foreach(var message in unreadMessages){
                message.DateRead = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}
