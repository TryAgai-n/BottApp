using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Services.Handlers.MainMenu;

public class MainMenuHandler : IMainMenuHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;
    private readonly StateService _stateService;
    

    public MainMenuHandler(IUserRepository userRepository, IDocumentService documentService, IMessageService messageService,StateService stateService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _stateService = stateService;
        _messageService = messageService;
    }
    
    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Главное меню", replyMarkup: Keyboard.MainKeyboard
        );
    }
   //Todo: убрать рерализации вспомогательных методов редактирования сообщений в MessageManager
    public string GetTimeEmooji()
    {
        string[] emooji = {"🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛", "🕐 ", "🕑 ",};
        var rand = new Random();
        var preparedString = emooji[rand.Next(0, emooji.Length)];
        return preparedString;
    }
    
    public async Task<Message> TryEditMessage(
        ITelegramBotClient? botClient,
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
                return await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, text: editText,
                    replyMarkup: Keyboard.MainKeyboard, cancellationToken: cancellationToken
                );
            }
            catch
            {
                editText = viewText + GetTimeEmooji();

                return await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, text: editText,
                    replyMarkup: Keyboard.MainKeyboard, cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id, text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboard, cancellationToken: cancellationToken
            );
        }
    }


    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken, UserModel user)
    {
        Enum.TryParse<MenuButton>(callbackQuery.Data, out var result);
        var startup = result switch
        {
            MenuButton.Votes => _stateService.StartState(user, OnState.Votes, botClient, callbackQuery.Message),
            MenuButton.Help =>  _stateService.StartState(user, OnState.Help, botClient, callbackQuery.Message),
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    public async Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        await _messageService.MarkMessageToDelete(message);
        // if (message.Document != null)
        // {
        //     await _documentService.UploadFile(message, botClient);
        // }

        await Usage(botClient, message, cancellationToken, user);

         async Task Usage(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken,
            UserModel user
        )
        {
            await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
            await _messageService.MarkMessageToDelete( await botClient.SendTextMessageAsync(
                chatId: user.UId, text: "Главное Меню", replyMarkup: Keyboard.MainKeyboard,
                cancellationToken: cancellationToken
            ));
        }
    }



    public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    public async Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}