using System.Text;
using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace BottApp.Host.Handlers.AdminChat
{
    public class AdminChatHandler : IAdminChatHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageService _messageService;
        private readonly IDocumentService _documentService;
        private readonly IDocumentRepository _documentRepository;

        public AdminChatHandler(IUserRepository userRepository, IMessageService messageService, IDocumentService documentService, IDocumentRepository documentRepository)
        {
            _userRepository = userRepository;
            _messageService = messageService;
            _documentService = documentService;
            _documentRepository = documentRepository;
        }

       
        public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, UserModel user)
        {
            string prepString;
            
            switch (prepString = message.Text.ToLower())
            {
                case not null when prepString.Contains("/start"):
                    await botClient.SendTextMessageAsync
                    (
                        chatId: message.Chat.Id,
                        text: $"Поздравялю, {message.From.FirstName}, вы - админ\n"+
                              "Для вывода всех активных команд нажимайте /help",
                        cancellationToken: cancellationToken
                    );
                    break;
                
                case not null when prepString.Contains("/view_statistic_"):
                    await GetVoteStatistic(botClient, message, cancellationToken, prepString, FindDocumentBy.Views);
                    break;
                
                case not null when prepString.Contains("/like_statistic_" ):
                    await GetVoteStatistic(botClient, message, cancellationToken, prepString, FindDocumentBy.Likes);
                    break; 
                
                case not null when prepString.Contains( "/find_user_by_firstname"):
                    await FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.FirstName);
                    break;
                
                case not null when prepString.Contains( "/find_user_by_lastname"):
                    await FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.LastName);
                    break;
                
                case not null when prepString.Contains( "/find_user_by_id"):
                    await FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.Id);
                    break;
                
                case not null when prepString.Contains( "/find_user_by_uid"):
                    await FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.UId);
                    break;
                
                case not null when prepString.Contains("/send_document_"):
                    await SendDocument(botClient, message, cancellationToken, prepString);
                    break; 
                
                case not null when prepString.Contains("/send_top_by_like" ):
                    await SendTopDocument(botClient, message, cancellationToken, prepString, FindDocumentBy.Likes);
                    break; 
                
                case not null when prepString.Contains("/send_top_by_view") :
                    await SendTopDocument(botClient, message, cancellationToken, prepString, FindDocumentBy.Views);
                    break; 
                
                case not null when prepString.Contains( "/help"):
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "/start\n" +
                              "/view_statistic_3\n" +
                              "/like_statistic_3\n" +
                              "/send_document_1\n" +
                              "/send_top_by_like\n" +
                              "/send_top_by_view\n" +
                              "/find_user_by_firstname_[value]\n"+
                              "/find_user_by_lastname_[value]\n"+
                              "/find_user_by_id_[value]]\n"+
                              "/find_user_by_Uid_[value]]\n",
                        cancellationToken: cancellationToken
                    );
                    break;
            }
            
            
        }
        
        private async Task FindUserByParam(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string prepString, FindUserBy findUserBy)
        {
            switch (findUserBy)
            {
                case FindUserBy.FirstName:
                    break;
                
                case FindUserBy.LastName:
                    break;
                
                case FindUserBy.Phone:
                    break;
                
                
                case FindUserBy.Id:
                    try
                    {
                        int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
                        await _userRepository.FindOneById(id);
                    }
                    catch (Exception e)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "No user by current ID\n", cancellationToken: cancellationToken
                        );
                    }
                    finally
                    {
                        int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
                        var user = await _userRepository.FindOneById(id);
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"{user.FirstName} {user.LastName}\n" +
                                  $"Tel. {user.Phone}. ID {user.Id}. UID {user.UId}",
                            cancellationToken: cancellationToken
                        );
                    }
                    break;
                
                case FindUserBy.UId:
                    try
                    {
                        int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var uid);
                        await _userRepository.FindOneByUid(uid);
                    }
                    catch (Exception e)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "No user by current UID\n", cancellationToken: cancellationToken
                        );
                    }
                    finally
                    {
                        int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var uid);
                        var user = await _userRepository.FindOneByUid(uid);
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, 
                            text: $"{user.FirstName} {user.LastName}\n" +
                                  $"Tel. {user.Phone}. ID {user.Id}. UID {user.UId}",
                            cancellationToken: cancellationToken
                        );
                    }
                    break;
            }
        }
        
        private async Task SendTopDocument(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken, string prepString, FindDocumentBy findDocumentBy)
        {

            const int take = 1;
            var documents = findDocumentBy switch
            {
                FindDocumentBy.Views => await _documentRepository.List_Most_Document_In_Vote_By_Views(take),
                FindDocumentBy.Likes => await _documentRepository.List_Most_Document_In_Vote_By_Likes(take),
            };

            var groupByNomination = documents.ToLookup(s => s.DocumentNomination);
            
            foreach (var nomination in groupByNomination)
            {
                foreach (var item in documents.Where(x=> x.DocumentNomination == nomination.Key))
                {
                    await using FileStream fileStream = new(item.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: new InputOnlineFile(fileStream, "Document" + item.DocumentExtension),
                        caption: $"FindBy {findDocumentBy}\n" +
                                 $"Nomination: {item.DocumentNomination}\n" +
                                 $"ViewCount: {item.DocumentStatisticModel.ViewCount}\n" +
                                 $"LikeCount: {item.DocumentStatisticModel.LikeCount}\n" +
                                 $"Caption: {item.Caption}",
                        cancellationToken: cancellationToken); 
                }
               
            }
            
        }
        
        private async Task SendDocument(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken, string prepString)
        {
            try
            {
                int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
                var document = await _documentRepository.GetOneByDocumentId(id);
            }
            catch (Exception e)
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "No items by current ID\n",
                    cancellationToken: cancellationToken
                );
            }
            finally
            {
                int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
                var document = await _documentRepository.GetOneByDocumentId(id);
                
                await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            
                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, "Document" + document.DocumentExtension),
                    caption: $"{document.Caption}",
                    cancellationToken: cancellationToken); 
            }
        }

        private async Task GetVoteStatistic(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string prepString, FindDocumentBy findDocumentBy)
        {
            int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var take);
            if (take > 20)
                take = 20;

            var listTopByNomination = findDocumentBy switch
            {
                FindDocumentBy.Views => await _documentRepository.List_Most_Document_In_Vote_By_Views(take),
                FindDocumentBy.Likes => await _documentRepository.List_Most_Document_In_Vote_By_Likes(take),
            };

            var groupByNomination = listTopByNomination.ToLookup(s => s.DocumentNomination);

            var sb = new StringBuilder($"Try to find {take} candidates by {findDocumentBy}: \n\n");

            var i = 1;

            foreach (var nomination in groupByNomination)
            {
                sb.Append($"Nomination {nomination.Key}. Find {nomination.Count()}.\n");
                foreach (var item in listTopByNomination.Where(x => x.DocumentNomination == nomination.Key))
                {
                    sb.Append($"{i++}." +
                              $"  ID  {item.Id}" +
                              $"  View {item.DocumentStatisticModel.ViewCount}" +
                              $"  Like {item.DocumentStatisticModel.LikeCount} " +
                              $"/send_document_{item.Id}\n");
                }
                sb.Append('\n');
                i = 1;
            }
                
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: sb.ToString(),
                cancellationToken: cancellationToken
            );
        }

        public async Task BotOnCallbackQueryReceived
        (
            ITelegramBotClient? botClient,
            CallbackQuery callbackQuery,
            CancellationToken cancellationToken,
            UserModel user
        )
        {
            switch (callbackQuery.Data)
            {
                case nameof(AdminButton.Approve):
                    await Approve(botClient, callbackQuery, cancellationToken);
                    break;
                
                case nameof(AdminButton.Decline):
                    await Decline(botClient, callbackQuery, cancellationToken);
                    break;
                
                case nameof(AdminButton.DocumentApprove):
                    await ApproveDocument(botClient, callbackQuery, cancellationToken);
                    break;
                
                case nameof(AdminButton.DocumentDecline):
                    await DeclineDocument(botClient, callbackQuery, cancellationToken);
                    break;
                
                case nameof(AdminButton.SendOk): 
                    break;
                
                default: await _messageService.TryEditInlineMessage(botClient, callbackQuery, cancellationToken);
                    break;
            }
        }
        
        private async Task ApproveDocument(
            ITelegramBotClient botClient,
            CallbackQuery callbackQuery,
            CancellationToken cancellationToken
        )
        {
            var subs = callbackQuery.Message.Caption.Split(' ');
            var documentApproveId = Convert.ToInt32(subs[1]);
            
            await _documentRepository.SetModerate(documentApproveId, true);
            await botClient.EditMessageCaptionAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                caption: $"{callbackQuery.Message.Caption}\n\nПРИНЯТА @{callbackQuery.From.Username}",
                replyMarkup:  Keyboard.Ok,
                cancellationToken: cancellationToken
            );
          
        }
        
        private async Task DeclineDocument(
            ITelegramBotClient botClient,
            CallbackQuery callbackQuery,
            CancellationToken cancellationToken
        )
        {
            await botClient.EditMessageCaptionAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                caption: $"{callbackQuery.Message.Caption}\n\nОТКЛОНЕНА @{callbackQuery.From.Username}",
                replyMarkup:  Keyboard.Ok,
                cancellationToken: cancellationToken
            );
          
        }

        private async Task Approve(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
               
            var subs = callbackQuery.Message.Caption.Split(' ');
            var approveId = Convert.ToInt64(subs[3]);
            var user = await _userRepository.GetOneByUid(approveId);
            await _userRepository.ChangeOnState(user, OnState.Menu);
            
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text:  $"Заявка на регистрацию Пользователя UID {user.UId}\n" +
                       $"Имя {user.FirstName}\n" +
                       $"Фамилия {user.LastName} Phone {user.Phone}\n\n" +
                       $"ПРИНЯТА",
                cancellationToken: cancellationToken
            );
            
            await _messageService.TryDeleteMessage(AdminSettings.AdminChatId, callbackQuery.Message.MessageId, botClient);
            
            var userMsg = await botClient.SendTextMessageAsync
             (
                 chatId: approveId,
                 text: "Вы авторизованы!\nГлавное меню",
                 replyMarkup: Keyboard.MainKeyboard,
                 cancellationToken: cancellationToken
             );

          await _userRepository.ChangeViewMessageId(user, userMsg.MessageId);


        }
        private async Task Decline(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            
            var subs = callbackQuery.Message.Caption.Split(' ');
            var declineId = Convert.ToInt64(subs[3]);
            var user = await _userRepository.GetOneByUid(declineId);
              
              await botClient.SendTextMessageAsync
              (
                  chatId: callbackQuery.Message.Chat.Id,
                  text: $"Заявка на регистрацию Пользователя UID {user.UId}\n" +
                        $"Имя {user.FirstName}\n" +
                        $"Фамилия {user.LastName} Phone {user.Phone}\n\n" +
                        $"ОТКЛОНЕНА",
                  cancellationToken: cancellationToken
              );
              
              await _messageService.TryDeleteMessage(AdminSettings.AdminChatId, callbackQuery.Message.MessageId, botClient);

             var userMsg = await botClient.SendTextMessageAsync(
                  chatId: declineId, 
                  text: "Вы не авторизованы в системе, попробуйте позже!",
                  cancellationToken: cancellationToken
              );
              
              await _userRepository.ChangeViewMessageId(user, userMsg.MessageId);
        }
        
        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
        public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}