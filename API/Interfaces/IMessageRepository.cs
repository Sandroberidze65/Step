using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;
public interface IMessageRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message> GetMessage(int Id);
    Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recepientUserName);
    Task<bool> SaveAllAsync(); 
}