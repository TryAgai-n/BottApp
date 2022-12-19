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

    private readonly StateService _stateService;

    private List<LocalUser> _localUsers = new();


    public CandidateUploadHandler(IUserRepository userRepository, IDocumentService documentService, StateService stateService, IMessageService messageService)
    {
        _userRepository = userRepository;
        _documentService = documentService;
        _stateService = stateService;
        _messageService = messageService;
    }
    public async void Add_User_To_List(int userId, InNomination nomination)
    {
         _localUsers.Add(new LocalUser(userId, nomination));
         var user = _localUsers.FirstOrDefault(x => x.Id == userId);
    }
    
    public async void Remove_User_InTo_List(int userId)
    { 
        var user = _localUsers.FirstOrDefault(x => x.Id == userId);
        _localUsers.Remove(user);
    }
    
    public async Task<LocalUser> Get_One_User(int userId)
    {
        return _localUsers.FirstOrDefault(x => x.Id == userId);
    }

    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
         await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Отправь мне фотографию своего кандидата"
        );
    }


    public async Task BotOnCallbackQueryReceived(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken, UserModel user)
    {
        LocalUser? localUser;
        switch (callbackQuery.Data)
        {
            case nameof(NominationButton.Biggest):
                
                Add_User_To_List(user.Id, InNomination.First);
                localUser = await Get_One_User(user.Id);
                localUser.IsSendNomination = true;
                
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );
                
                return;
            
            case nameof(NominationButton.Smaller):
                
                Add_User_To_List(user.Id, InNomination.Third);
                localUser = await Get_One_User(user.Id);
                localUser.IsSendNomination = true;
                
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );

                return;
            
            case nameof(NominationButton.Fastest):
                
                Add_User_To_List(user.Id, InNomination.Second);
                localUser = await Get_One_User(user.Id);
                localUser.IsSendNomination = true;
                
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );

                return;
            
            case nameof(VotesButton.ToVotes):
           
                Remove_User_InTo_List(user.Id);
                
                await _messageService.DeleteMessages(botClient, user);
                await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                return;
        }
    }

    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        var localUser = await Get_One_User(user.Id);
        
        await _messageService.MarkMessageToDelete(message);
        
        switch (localUser)
        {


            case { IsSendNomination : false }:
            {
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Выберете номинацию", replyMarkup: Keyboard.NominationKeyboard,
                        cancellationToken: cancellationToken
                    )
                );

                return;
            }
            
            case { IsSendNomination: true, IsSendCaption: false } when message.Text != null:
                
                localUser.DocumentCaption = message.Text;

                localUser.IsSendCaption = true;

                await _messageService.DeleteMessages(botClient, user);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "Отправьте фото кандидата в виде документа :)"));
                return;
            
            
            case { IsSendNomination: true, IsSendCaption: false }:
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Внесите описание в виде текста"));
                return;
            
            
            case { IsSendCaption: true, IsSendDocument: false }:
            {
                if (message.Document != null)
                {
                    await _documentService.UploadVoteFile(message, botClient, localUser.Nomination,
                        localUser.DocumentCaption);
                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Все сохранил, спасибо!"));
                    await Task.Delay(1000, cancellationToken);
                    await _messageService.DeleteMessages(botClient, user);

                    Remove_User_InTo_List(user.Id);

                    await _stateService.Startup(user, OnState.Votes, botClient, message);
                }
                break;
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