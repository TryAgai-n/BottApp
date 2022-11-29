using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.UploadHandler;

public class CandidateUploadHandler : ICandidateUploadHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;
    
    private readonly StateService _stateService;
    
    public CandidateUploadHandler(IUserRepository userRepository, IDocumentService documentService, StateService stateService, IMessageService messageService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _stateService = stateService;
        _messageService = messageService;
    }


    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
         await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Отправь мне фотографию своего кандидата"
        );
    }


    public async Task BotOnCallbackQueryReceived(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken, UserModel user)
    {
        throw new NotImplementedException();
    }

    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        await _messageService.MarkMessageToDelete(message);
        
        if (message.Document != null)
        {
            if(await _documentService.UploadVoteFile(message, botClient))
            {
               await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync(message.Chat.Id, "Спасибо! Ваш документ загружен в базу данных."));

               await Task.Delay(1000);

               _messageService.DeleteMessages(botClient);

               await _stateService.Startup(user, OnState.Votes, botClient, message);
            }
        }
        
        if (message.Text is not { } messageText)
            return;

        var action = messageText switch
        {
            _ => Usage(botClient, message, cancellationToken)
        };

         async Task Usage(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken)
        {
           await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Отправь мне фотографию своего кандидата в виде документа",
                cancellationToken: cancellationToken
            ));
        }
    }

    public async Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}