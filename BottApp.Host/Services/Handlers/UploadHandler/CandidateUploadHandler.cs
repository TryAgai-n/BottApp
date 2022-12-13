using BottApp.Database.Document;
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
    
    
    private bool _isSendDocument;
    private bool _isSendCaption;
    private bool _isSendNomination;
    private InNomination nomination;
    private string _caption;
    
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
        switch (callbackQuery.Data)
        {
            case nameof(NominationButton.Biggest):
                
                nomination = InNomination.ㅤ;
                _isSendNomination = true;
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );
                
                return;
            
            case nameof(NominationButton.Smaller):
                nomination = InNomination.ㅤㅤㅤ;
                _isSendNomination = true;
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );

                return;
            
            case nameof(NominationButton.Fastest):
                nomination = InNomination.ㅤㅤ;
                _isSendNomination = true;
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );

                return;
            
            case nameof(VotesButton.ToVotes):
          
                _isSendNomination = false;
                _isSendCaption = false;
                _isSendDocument = false;
                await _messageService.DeleteMessages(botClient, user);
                await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                return;
        }
    }

    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        await _messageService.MarkMessageToDelete(message);
        
        if (!_isSendNomination)
        {
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Выберете номинацию", replyMarkup: Keyboard.NominationKeyboard, cancellationToken: cancellationToken
                )
            );
            
            return;
        }
        
        if (_isSendNomination && !_isSendCaption)
        {
            if (message.Text != null)
            {
                _caption = message.Text;
                _isSendCaption = true;
                await _messageService.DeleteMessages(botClient, user);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Наконец загрузите фотографию в виде документа :)"));
                return;
            }
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(message.Chat.Id, "Внесите описание в виде текста"));
            return;
        }

        if (_isSendCaption && !_isSendDocument)
        {
            if (message.Document!=null)
            {
                await _documentService.UploadVoteFile(message, botClient, nomination,
                    _caption);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Все сохранил, спасибо!"));
                await Task.Delay(1000);
                await _messageService.DeleteMessages(botClient, user);
                
                _isSendNomination = false;
                _isSendCaption = false;
                _isSendDocument = false;
                _caption = "";
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