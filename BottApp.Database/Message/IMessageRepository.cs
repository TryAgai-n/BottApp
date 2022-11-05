using System;
using System.Threading.Tasks;
using BottApp.Utils;

namespace BottApp.Database.Message;

public interface IMessageRepository
{
    Task<MessageModel> CreateModel(int userId, string? description, DateTime createdAt);
}