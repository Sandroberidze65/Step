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

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }


    public void AddMessage(Message message)
    {
        _context.messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.messages.Remove(message);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups.Include(x=>x.connections)
               .Where(x=>x.connections
               .Any(c=>c.ConnectionId == connectionId))
               .FirstOrDefaultAsync();
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

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context.Groups.Include(x=>x.connections).FirstOrDefaultAsync(x=>x.Name == groupName);
    }


    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recepientUserName)
    {
        var query = _context.messages.Where(
                            m => m.RecipientUsername == currentUserName && m.RecepientDeleted == false && m.SenderUsername == recepientUserName ||
                                m.RecipientUsername == recepientUserName && m.SenderDeleted == false && m.SenderUsername == currentUserName
                        ).OrderBy(m => m.MessageSent).AsQueryable();

        var unreadMessages = query.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList();

        if(unreadMessages.Any()){
            foreach(var message in unreadMessages){
                message.DateRead = DateTime.UtcNow;
            }
        }

        return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

}
