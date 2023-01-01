using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Types;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Services.Handlers.UploadHandler;

public class CandidateUploadHandler : ICandidateUploadHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;

    private readonly StateService _stateService;

  


    public CandidateUploadHandler(IUserRepository userRepository, IDocumentRepository documentRepository, IDocumentService documentService, StateService stateService, IMessageService messageService)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _documentService = documentService;
        _stateService = stateService;
        _messageService = messageService;
       
    }
    
    

    public async Task OnStart(ITelegramBotClient botClient, Message message)
    {
         await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Отправьте мне фотографию своего кандидата"
        );
    }


    public async Task BotOnCallbackQueryReceived(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken, UserModel user)
    {
        LocalUser? localUser;
        switch (callbackQuery.Data)
        {
            case nameof(MenuButton.BiggestNomination):
                
                if (await _documentRepository.CheckSingleDocumentInNominationByUser(user, InNomination.First))
                {
                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id, text: "Вы уже добавляли кандидата в текущую номинацию!",
                            cancellationToken: cancellationToken
                        )
                    );
                    await Task.Delay(2000, cancellationToken);
                    await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                    await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                    
                    return;
                }
                
                await UserService.Add_User_To_List(user.Id);
                localUser = await UserService.Get_One_User(user.Id);
                localUser.Nomination = InNomination.First;
                localUser.IsSendNomination = true;
                
                ////////////
                await _documentService.CreateEmptyDocumentForVotes(user.Id, InNomination.First);

                await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );
                
                return;
            
            case nameof(MenuButton.SmallerNomination):
                
                if (await _documentRepository.CheckSingleDocumentInNominationByUser(user, InNomination.Third))
                {
                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id, text: "Вы уже добавляли кандидата в текущую номинацию!",
                            cancellationToken: cancellationToken
                        )
                    );
                    await Task.Delay(2000, cancellationToken);
                    await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                    await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                    
                    return;
                }
                
                await UserService.Add_User_To_List(user.Id);
                localUser = await UserService.Get_One_User(user.Id);
                localUser.Nomination = InNomination.Third;
                localUser.IsSendNomination = true;
                await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );
                
                return;
            
            case nameof(MenuButton.FastestNomination):
                
                if (await _documentRepository.CheckSingleDocumentInNominationByUser(user, InNomination.Second))
                {
                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id, text: "Вы уже добавляли кандидата в текущую номинацию!",
                            cancellationToken: cancellationToken
                        )
                    );
                    
                    await Task.Delay(2000, cancellationToken);
                    await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                    await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                    
                    return;
                }
                
                await UserService.Add_User_To_List(user.Id);
                localUser = await UserService.Get_One_User(user.Id);
                localUser.Nomination = InNomination.Second;
                localUser.IsSendNomination = true;
                await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: callbackQuery.Message.Chat.Id, text: "Внесите краткое описание кандидата.", cancellationToken: cancellationToken
                    )
                );

                return;
            
            
            
            case nameof(MenuButton.Votes):
           
                await UserService.Remove_User_InTo_List(user.Id);
                
                await _messageService.DeleteMessages(botClient, user.UId, callbackQuery.Message.MessageId);
                await _stateService.Startup(user, OnState.Votes, botClient, callbackQuery.Message);
                return;
        }
    }

    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        var localUser = await UserService.Get_One_User(user.Id);
        
        await _messageService.MarkMessageToDelete(message);
        
        switch (localUser)
        {
            case { IsSendNomination: true, IsSendCaption: false } when message.Text != null:
                
                localUser.DocumentCaption = message.Text;

                localUser.IsSendCaption = true;

                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "Отправьте фото кандидата", cancellationToken: cancellationToken));
                return;
            
            
            case { IsSendNomination: true, IsSendCaption: false }:
                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Внесите описание в виде текста", cancellationToken: cancellationToken));
                return;

            case { IsSendCaption: true, IsSendDocument: false } when message.Photo == null:
            {
                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
                
                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Отправьте фото как обычно. Не в виде документа", cancellationToken: cancellationToken));

                break;
            }
            
            case { IsSendCaption: true, IsSendDocument: false }:
            {
                await _documentService.UploadVoteFile(message, botClient, localUser.Nomination,
                    localUser.DocumentCaption);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Сохранил, спасибо!\nПосле модерации ваш кандидат появится в соответсвующей номинации, ожидайте! :)", cancellationToken: cancellationToken));

                await Task.Delay(2000, cancellationToken);

                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                await UserService.Remove_User_InTo_List(user.Id);

                await _stateService.Startup(user, OnState.Votes, botClient, message);

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
                text: "Отправьте фото кандидата",
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