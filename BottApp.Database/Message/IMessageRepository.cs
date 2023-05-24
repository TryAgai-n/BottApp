using System;
using System.Threading.Tasks;

namespace BottApp.Database.Message;

public interface IMessageRepository
{
    Task<MessageModel> CreateModel(int userId, string? description, string? type, DateTime createdAt);
}