using BottApp.Database.Message;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public class MessageService : IMessageService
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private List<Telegram.Bot.Types.Message> _deleteMessageList = new();


    public MessageService(IUserRepository userRepository, IMessageRepository messageRepository)
    {
        _userRepository = userRepository;
        _messageRepository = messageRepository;
    }


    public async Task SaveMessage(Telegram.Bot.Types.Message message)
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

    public async Task<Telegram.Bot.Types.Message> TryEditInlineMessage(
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
    
    public Task MarkMessageToDelete(Telegram.Bot.Types.Message message)
    {
        _deleteMessageList.Add(message);
        return Task.CompletedTask;
    }

    public void DeleteMessages(ITelegramBotClient? botClient)
    {
        for (int i = _deleteMessageList.Count - 1; i >= 0; i--)
        {
            botClient.DeleteMessageAsync(
                chatId: _deleteMessageList[i].Chat.Id,
                messageId: _deleteMessageList[i].MessageId);

            _deleteMessageList.RemoveAt(i);
        }


        // foreach (var message in _deleteMessageList)
        // { 
        //    
        //     _deleteMessageList.RemoveAt(message.MessageId);
        // }
    }
}