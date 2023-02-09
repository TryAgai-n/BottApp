using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Handlers.Help;

public class HelpHandler : IHelpHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;
    private readonly StateService _stateService;
    

    public HelpHandler(IUserRepository userRepository, IDocumentService documentService, IMessageService messageService,StateService stateService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _stateService = stateService;
        _messageService = messageService;
    }
    
    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Раздел помощи", replyMarkup: Keyboard.MainKeyboard
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
            MenuButton.MainMenu =>  _stateService.StartState(user, OnState.Menu, botClient),
            _ => _stateService.StartState(user, OnState.Help, botClient)
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
        await Task.Delay(1500, cancellationToken);
        
        var msg = await botClient.EditMessageTextAsync(
            chatId:  message.Chat.Id,
            messageId:  user.ViewMessageId, 
            text: "Ваш вопрос успешно передан в службу поддержки, вы будете возвращены в главное меню", 
            replyMarkup: InlineKeyboardMarkup.Empty(), 
            cancellationToken: cancellationToken);

        await botClient.SendTextMessageAsync(
            AdminSettings.AdminChatId,
            $"ВОПРОС от @{message.Chat.Username ?? "Нет публичного имени"}\nID {user.Id} UID {user.UId}\n" +
            $"Моб.тел. {user.Phone}\n\n" +
            $"\"{message.Text}\"", cancellationToken: cancellationToken
        );
        
       await _messageService.TryDeleteMessage(user.UId, message.MessageId, botClient);
       
       await Task.Delay(5000, cancellationToken);
       
       await _stateService.StartState(user, OnState.Menu, botClient);
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