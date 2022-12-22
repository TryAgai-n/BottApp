using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Database.UserMessage;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public class MessageService : IMessageService
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private List<Message> _messageList = new();


    public MessageService(IUserRepository userRepository, IMessageRepository messageRepository)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }


    public async Task SaveMessage(Message message)
    {
        var user = await _userRepository.FindOneByUid((int)message.Chat.Id);
        string type = message.Type.ToString();
        await _messageRepository.CreateModel(user.Id, message.Text, type, DateTime.Now);
    }

    public async Task SaveInlineMessage(CallbackQuery callbackQuery)
    {
        var user = await _userRepository.FindOneByUid((int)callbackQuery.Message.Chat.Id);
        string type = callbackQuery.GetType().ToString();
        await _messageRepository.CreateModel(user.Id, callbackQuery.Data, type, DateTime.Now);
    }

    public async Task<Message> TryEditInlineMessage(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    )
    {
        var viewText = "Такой команды еще нет ";
        var viewExceptionText = "Все сломаделось : ";
        var editText = viewText + GetTimeEmooji();

        try
        {
            try
            {
                return await botClient.EditMessageTextAsync
                (
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: editText,
                    replyMarkup: Keyboard.MainKeyboard,
                    cancellationToken: cancellationToken
                );
            }
            catch
            {
                editText = viewText + GetTimeEmooji();

                return await botClient.EditMessageTextAsync
                (
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: editText,
                    replyMarkup: Keyboard.MainKeyboard,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboard,
                cancellationToken: cancellationToken
            );
        }
    }


    private string GetTimeEmooji()
    {
        string[] emooji = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ", };
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }
    
    public async Task MarkMessageToDelete(Message message)
    {
         _messageList.Add(message);
    }

    public async Task<bool> TryDeleteAfterReboot(ITelegramBotClient botClient, long UId, int messageId)
    {
        var lastIndex = messageId - 1;

        await botClient.DeleteMessageAsync(
            chatId: UId,
            messageId: lastIndex);

        return true;
    }


    public async Task DeleteMessages(ITelegramBotClient botClient, long UId, int messageId)
    {
    

    
         
        

        var temp = _messageList.Where(x => x.Chat.Id == UId).ToList();
        if (temp.Count > 0)
        {
            foreach (var message in temp)
            {
                await botClient.DeleteMessageAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId);
                _messageList.Remove(message);
            }
            temp.Clear();
        }
        else
        {
            try
            {
                for (var i = 3; i > -3; i--)
                {
                    try
                    {
                         botClient.DeleteMessageAsync(
                            chatId: UId,
                            messageId: messageId+i);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("delete message not found" + e);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Global delete message Exception" + e);
            }
          
        }
    }
}