using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Services.Handlers.Help;

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
        
        switch (callbackQuery.Data)
        {
            case nameof(HelpButton.ToMainMenu):
                await _messageService.DeleteMessages(botClient, user.UId,callbackQuery.Message.MessageId);
                await _stateService.Startup(user, OnState.Menu, botClient, callbackQuery.Message);
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
        await _messageService.DeleteMessages(botClient, user.UId,message.MessageId);
        await _messageService.MarkMessageToDelete(message);
        await _messageService.DeleteMessages(botClient, user.UId,message.MessageId);
        
        await botClient.SendTextMessageAsync(
            AdminSettings.AdminChatId,
            $"ВОПРОС от Пользователя {user.FirstName}\n@{message.Chat.Username ?? "Нет публичного имени"} UID {user.UId}\n" +
            $"Моб.тел. {user.Phone}\n\n" +
            $"{message.Text}"
        );
      

        
        await _messageService.MarkMessageToDelete( await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Ваш вопрос успешно передан в службу поддержки, вы будете возвращены в главное меню"));
        
        await Task.Delay(5000, cancellationToken);
        
        await _messageService.DeleteMessages(botClient, user.UId,message.MessageId);
        
        await _stateService.Startup(user, OnState.Menu, botClient, message);
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

        // _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}