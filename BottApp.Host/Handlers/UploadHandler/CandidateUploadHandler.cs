using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.User;
using BottApp.Host.Services.OnStateStart;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using InputFile = Telegram.Bot.Types.InputFile;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;

namespace BottApp.Host.Handlers.UploadHandler;

public class CandidateUploadHandler : ICandidateUploadHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentService _documentService;
    private readonly IMessageService _messageService;

    private readonly StateService _stateService;

  


    public CandidateUploadHandler(
        IUserRepository userRepository,
        IDocumentRepository documentRepository,
        IDocumentService documentService,
        StateService stateService,
        IMessageService messageService)
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
        Enum.TryParse<MenuButton>(callbackQuery.Data, out var result);

        var button = result switch
        {
            MenuButton.BiggestNomination => UploadCandidate(InNomination.First),
            MenuButton.SmallerNomination => UploadCandidate(InNomination.Second),
            MenuButton.FastestNomination => UploadCandidate(InNomination.Third),
            MenuButton.Votes =>  _stateService.StartState(user, OnState.Votes, botClient),
            _ => throw new ArgumentOutOfRangeException()
        };

        await button;

        async Task UploadCandidate(InNomination nomination)
        {
            Message? msg;
            if (await _documentRepository.CheckSingleDocumentInNominationByUser(user, nomination))
            {
               msg = await botClient.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    user.ViewMessageId,
                    text: "Вы уже добавляли кандидата в текущую номинацию!\n\n" +
                          "Переход в меню \"Голосование\"...",
                    cancellationToken: cancellationToken
                );
                
                await Task.Delay(3000, cancellationToken);
                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                await _stateService.StartState(user, OnState.Votes, botClient);
                
                return;
            }
            
            var document = await _documentService.CreateDocument(user.Id, nomination);
            await _userRepository.ChangeViewDocumentId(user, document.Id);

             msg = await botClient.EditMessageTextAsync(
                 chatId: callbackQuery.Message.Chat.Id,
                 user.ViewMessageId,
                 text: "Внесите краткое описание кандидата.",
                cancellationToken: cancellationToken
            );
            
            await _userRepository.ChangeViewMessageId(user, msg.MessageId);
           
        }
    }

    public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
    {
        Message? msg;
        var document = await _documentRepository.GetOneByDocumentId(user.ViewDocumentId);

        try
        {
            switch (document)
            {
                case {Caption: null} when message.Text is null:
                    
                    await Task.Delay(1000, cancellationToken);
                    
                    await botClient.EditMessageTextAsync(
                        user.UId
                        , user.ViewMessageId,
                        "Внесите описание в виде текста",
                        cancellationToken: cancellationToken
                    );
                    
                    await botClient.DeleteMessageAsync(user.UId, message.MessageId);
                    return;

                case {Caption: null} when message.Text is not null:
                    
                    await Task.Delay(500, cancellationToken);
                    
                    document.Caption = message.Text;
                    await _documentRepository.UpdateDocument(document);

                    await botClient.EditMessageTextAsync(
                        user.UId,
                        user.ViewMessageId,
                        "Отправьте фото кандидата",
                        cancellationToken: cancellationToken);
                    await botClient.DeleteMessageAsync(user.UId, message.MessageId);
                    
                    return;

                case {Caption: not null} when message.Photo is null:
                    
                    await Task.Delay(1000, cancellationToken);
                    
                    await botClient.EditMessageTextAsync(
                        user.UId, user.ViewMessageId, "Отправьте фото как обычно. Не в виде документа",
                        cancellationToken: cancellationToken
                    );
                    
                    await botClient.DeleteMessageAsync(user.UId, message.MessageId);
                    return;

                case {Caption: not null} when message.Photo is not null:

                    await _documentService.UploadVoteFile(user, document, botClient, message);
                    await Task.Delay(1000, cancellationToken);
                    
                    await botClient.DeleteMessageAsync(user.UId, message.MessageId);
                    
                    msg = await botClient.EditMessageTextAsync(
                        user.UId, user.ViewMessageId,
                        "Сохранил, спасибо!\n\n" +
                        "*После модерации* ваш кандидат появится в соответствующей номинаци.\nОжидайте! :)",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                    
                    await Task.Delay(2000, cancellationToken);
                
                    
                    await _userRepository.ChangeViewMessageId(user, msg.MessageId);


                    await _stateService.StartState(user, OnState.Menu, botClient);
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        { 
            await botClient.DeleteMessageAsync(user.UId, message.MessageId);
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