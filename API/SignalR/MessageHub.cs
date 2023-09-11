using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"];
        var groupName = GetgroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User.GetUsername();
        if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByNameAsync(username);
        var recepient = await _userRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);

        if (recepient == null) throw new HubException("Not found user");

        var message = new Message
        {
            Sender = sender,
            SenderId = sender.Id,
            Recepient = recepient,
            RecipientId = recepient.Id,
            SenderUsername = sender.UserName,
            RecipientUsername = recepient.UserName,
            Content = createMessageDto.Content,
        };

        var groupName = GetgroupName(sender.UserName, recepient.UserName);

        var group = await _messageRepository.GetMessageGroup(groupName);

        if(group.connections.Any(x=>x.Username == recepient.UserName)){
            message.DateRead = DateTime.UtcNow;
        }
        else{
            var connections = await PresenceTracker.GetConnectionFroUser(recepient.UserName);
            if(connections != null){
                await _presenceHub.Clients.Clients(connections)
                .SendAsync("NewMessageReceived", new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
        }
    }
    private string GetgroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName);
        var connetion = new Connection(Context.ConnectionId, Context.User.GetUsername());

        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.connections.Add(connetion);

        if(await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to add to group");
    }

    private async Task<Group> RemoveFromMessageGroup(){
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.connections.FirstOrDefault(x=>x.ConnectionId == Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);
        if(await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
        
    }
}