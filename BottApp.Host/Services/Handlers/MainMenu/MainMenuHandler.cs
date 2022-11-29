using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Services.Handlers.MainMenu;

public class MainMenuHandler : IMainMenuHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentService _documentService;
    private readonly StateService _stateService;
    

    public MainMenuHandler(IUserRepository userRepository, IDocumentService documentService, StateService stateService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _stateService = stateService;
    }
    
    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Главное меню", replyMarkup: Keyboard.MainKeyboardMarkup
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
                    replyMarkup: Keyboard.MainKeyboardMarkup, cancellationToken: cancellationToken
                );
            }
            catch
            {
                editText = viewText + GetTimeEmooji();

                return await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id, messageId: callbackQuery.Message.MessageId, text: editText,
                    replyMarkup: Keyboard.MainKeyboardMarkup, cancellationToken: cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            return await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id, text: viewExceptionText + "\n" + e,
                replyMarkup: Keyboard.MainKeyboardMarkup, cancellationToken: cancellationToken
            );
        }
    }


    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken, UserModel user)
    {
        // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        // await MessageManager.SaveInlineMessage(_dbContainer, callbackQuery)
        
        //Todo: Попробовать организовать единый code-style для Switch конструкций
        switch (callbackQuery.Data)
        {
            case nameof(MenuButton.Hi):
                await botClient.SendTextMessageAsync(
                    chatId: -1001824488986,
                    text: user.TelegramFirstName + " говорит привет!", 
                    cancellationToken: cancellationToken);
                break;
            
            case nameof(MenuButton.ToVotes):
                await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                break;
            
            default:
                await TryEditMessage(botClient, callbackQuery, cancellationToken);
                break;
        }
    }


    public async Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        if (message.Document != null)
        {
            await _documentService.UploadFile(message, botClient);
        }

        await Usage(botClient, message, cancellationToken, user);

        static async Task<Message> Usage(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken,
            UserModel user
        )
        {
            return await botClient.SendTextMessageAsync(
                chatId: user.UId, text: "Главное Меню", replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }



    public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
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

        // _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}