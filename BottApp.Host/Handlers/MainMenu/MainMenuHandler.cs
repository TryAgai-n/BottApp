using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Handlers.MainMenu;

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
        _messageService = messageService;
        _stateService = stateService;
    }
    
    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Главное меню", replyMarkup: Keyboard.MainKeyboard
        );
    }

    public async Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken, UserModel user)
    {
        Enum.TryParse<MenuButton>(callbackQuery.Data, out var result);
        var button = result switch
        {
            MenuButton.Votes => _stateService.StartState(user, OnState.Votes, botClient),
            MenuButton.Help =>  _stateService.StartState(user, OnState.Help, botClient),
            _ => _stateService.StartState(user, OnState.Menu, botClient)
        };

        await button;
    }


    public async Task BotOnMessageReceived(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken,
        UserModel user
    )
    {
        if (message.Text is not { } messageText)
            return;
        
        Message? msg;
        
        if (message.Text.Contains("/restart"))
        {
             msg = await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, text: "Перезагружено\nМеню: Голосование", replyMarkup: Keyboard.MainKeyboard
            );

            await _userRepository.ChangeViewMessageId(user, msg.MessageId);
            return;
        }
        
        msg = await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Используй вирутальные кнопки", cancellationToken: cancellationToken
        );
        await Task.Delay(1000);
        await botClient.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
        // await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
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