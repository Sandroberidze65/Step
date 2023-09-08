using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class MessagesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;


    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;

    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByNameAsync(username);
        var recepient = await _userRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);

        if (recepient == null) return NotFound();

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

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
        return BadRequest("Failed To send message");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();
        var messages = await _messageRepository.GetMessageForUser(messageParams);
        Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){
        var currentUsername = User.GetUsername();

        return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id){
        var username = User.GetUsername();

        var message = await _messageRepository.GetMessage(id);

        if(message.SenderUsername != username && message.RecipientUsername != username) return Unauthorized();

        if(message.SenderUsername == username) message.SenderDeleted = true;

        if(message.RecipientUsername == username) message.RecepientDeleted = true;

        if( message.SenderDeleted && message.RecepientDeleted){
            _messageRepository.DeleteMessage(message);
        }

        if(await _messageRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting the message");
    }
}
