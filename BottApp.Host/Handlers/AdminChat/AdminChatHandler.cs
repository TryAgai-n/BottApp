using System.Text;
using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Handlers.Votes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MenuButton = BottApp.Database.Service.Keyboards.MenuButton;
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
            var prepString = message.Text.ToLower();

            var task = prepString switch
            {
                {} when prepString.Contains("/start") => SendStart(botClient, message),
                {} when prepString.Contains("/help") => SendHelp(botClient, message),
                {} when prepString.Contains("/view_statistic_") => GetVoteStatistic(botClient, message, cancellationToken, prepString, FindDocumentBy.Views),
                {} when prepString.Contains("/like_statistic_") => GetVoteStatistic(botClient, message, cancellationToken, prepString, FindDocumentBy.Likes),
                {} when prepString.Contains("/find_user_by_firstname") => FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.FirstName),
                {} when prepString.Contains("/find_user_by_lastname") => FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.LastName),
                {} when prepString.Contains("/find_user_by_id") => FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.Id),
                {} when prepString.Contains("/find_user_by_uid") => FindUserByParam(botClient, message, cancellationToken, prepString, FindUserBy.UId),
                {} when prepString.Contains("/send_document_") => SendDocument(botClient, message, cancellationToken, prepString),
                {} when prepString.Contains("/send_top_by_like") => SendTopDocument(botClient, message, cancellationToken, prepString, FindDocumentBy.Likes),
                {} when prepString.Contains("/send_top_by_view") => SendTopDocument(botClient, message, cancellationToken, prepString, FindDocumentBy.Views),
                // "/add_to_vote_on" => VoteTurnSwitch.UploadCandidateIsOn = true,
                // "/add_to_vote_off" => VoteTurnSwitch.UploadCandidateIsOn = false,
                // "/vote_on" => VoteTurnSwitch.VoteIsOn = true,
                // "/vote_off" => VoteTurnSwitch.VoteIsOn = false,
                }; 
            await task;
        }

        async Task<Message> SendStart(ITelegramBotClient botClient, Message message)
        {
            return await botClient.SendTextMessageAsync
            (chatId: message.Chat.Id,
                text: $"Поздравялю, {message.From.FirstName}, вы - админ\n" +
                      "Для вывода всех активных команд нажимайте /help");
        }  
        async Task<Message> SendHelp(ITelegramBotClient botClient, Message message)
        {
         return await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "/start\n\n" +
                              "/view_statistic_3\n" +
                              "Отправляет статистику просмотров по кандидатам во всех существующих номинациях. Значение по-умолчанию \"3\"\n\n" +
                              
                              "/like_statistic_3\n" +
                              "Отправляет статистику лайков по кандидатам во всех существующих номинациях. Значение по-умолчанию \"3\"\n\n" +
                              
                              "/send_document_[value]\n" +
                              "Отправляет изображение и описание кандидата по ID\n\n" +
                              
                              "/send_top_by_like\n" +
                              "Отправляет изображение кандидата с максимальным кол-вом лайков по каждой номинации\n\n" +
                              
                              "/send_top_by_view\n" +
                              "Отправляет изображение кандидата с максимальным кол-вом просмотров по каждой номинации\n\n" +
                              
                              "/find_user_by_firstname_[value]\n"+
                              "Ищет пользователя по имени\n\n" +
                              
                              "/find_user_by_lastname_[value]\n"+
                              "Ищет пользователя по фамилии\n\n" +
                              
                              "/find_user_by_id_[value]\n"+
                              "Ищет пользователя по ID\n\n" +
                              
                              "/find_user_by_Uid_[value]\n"+
                              "Ищет пользователя по UID\n\n"+
                              
                              "/add_to_vote_on\n"+
                              "Включает возможность загружать кандидатов\n\n"+
                              
                              "/add_to_vote_off\n"+
                              "Выключает возможность загружать кандидатов\n\n"+
                              
                              "/vote_on\n"+
                              "Включает возможность голосования\n\n"+
                              
                              "/vote_off\n"+
                              "Выключает возможность голосования\n\n"
                              );
        }
        
        private async Task FindUserByParam(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string prepString, FindUserBy findUserBy)
        {
            UserModel? user;

            switch (findUserBy)
            {
                case FindUserBy.FirstName:
                    break;
                
                case FindUserBy.LastName:
                    break;
                
                case FindUserBy.Phone:
                    break;
                
                
                case FindUserBy.Id:
                  
                        int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
                        user = await _userRepository.FindOneById(id);

                        if (user is null)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id, text: "No user by current ID\n", cancellationToken: cancellationToken
                            );
                            return;
                        }
                      
                    
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"{user.FirstName} {user.LastName}\n" + $"Tel. {user.Phone}. ID {user.Id}. UID {user.UId}",
                            cancellationToken: cancellationToken
                        );
                        
                        break;
                
                case FindUserBy.UId:
                    
                        int.TryParse(string.Join("", prepString.Where(char.IsDigit)), out var uid);
                         user = await _userRepository.FindOneByUid(uid);
                   
                        if (user is null)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id, text: "No user by current UID\n", cancellationToken: cancellationToken
                            );
                            return;
                        }

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"{user.FirstName} {user.LastName}\n" + $"Tel. {user.Phone}. ID {user.Id}. UID {user.UId}",
                            cancellationToken: cancellationToken
                        );
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
                        photo: new InputFile(fileStream, "Document" + item.DocumentExtension),
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
                var filePath = Directory.GetCurrentDirectory() + document.Path;
                await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputFile(fileStream, "Document" + document.DocumentExtension),
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
                sb.Append($"Nomination {nomination.Key} Find {nomination.Count()}\n");
                foreach (var item in listTopByNomination.Where(x => x.DocumentNomination == nomination.Key))
                {
                    sb.Append($"{i++}" +
                              $"  ID  {item.Id}" +
                              $"  View {item.DocumentStatisticModel.ViewCount}" +
                              $"  Like {item.DocumentStatisticModel.LikeCount}\n" +
                              $"||/send\\_document\\_{item.Id}||\n");
                }
                sb.Append('\n');
                i = 1;
            }
                
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: sb.ToString(),
                parseMode: ParseMode.MarkdownV2,
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
            Enum.TryParse<AdminButton>(callbackQuery.Data, out var result);
            var button = result switch
            {
                AdminButton.Approve => Approve(botClient, callbackQuery, cancellationToken),
                AdminButton.Decline => Decline(botClient, callbackQuery, cancellationToken),
                AdminButton.DocumentApprove => ApproveDocument(botClient, callbackQuery, cancellationToken),
                AdminButton.DocumentDecline => DeclineDocument(botClient, callbackQuery, cancellationToken),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            await button;
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
                replyMarkup: Keyboard.Ok,
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
                caption: $"{callbackQuery.Message.Caption}\n\n*ОТКЛОНЕНА* @{callbackQuery.From.Username}",
                ParseMode.Markdown,
                replyMarkup:  Keyboard.Ok,
                cancellationToken: cancellationToken
            );
          
        }

        private async Task Approve(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
               
            var subs = callbackQuery.Message.Caption.Split(' ');
            var approveUid = Convert.ToInt64(subs[3]);
            var user = await _userRepository.GetOneByUid(approveUid);
            
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: $"ID {user.Id} UID {user.UId} \n" +
                      $"Имя: {user.FirstName} \n"+
                      $"Фамилия: {user.LastName} \n" +
                      $"Моб.тел. {user.Phone} \n\n" +
                      $"Заявка на авторизацию ПРИНЯТА",
                replyMarkup:  Keyboard.Ok,
                cancellationToken: cancellationToken
            );
            user.isAuthorized = true;
            await _userRepository.UpdateUser(user);
            await _userRepository.ChangeOnState(user, OnState.Menu);
            await _messageService.TryDeleteMessage(AdminSettings.AdminChatId, callbackQuery.Message.MessageId, botClient);
            
            var userMsg = await botClient.SendTextMessageAsync
             (
                 chatId: approveUid,
                 text: "Вы авторизованы!\n\nГлавное меню",
                 replyMarkup: Keyboard.MainKeyboard,
                 cancellationToken: cancellationToken
             );

          await _userRepository.ChangeViewMessageId(user, userMsg.MessageId);


        }
        private async Task Decline(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            
            var subs = callbackQuery.Message.Caption.Split(' ');
            var declineUid = Convert.ToInt64(subs[3]);
            var user = await _userRepository.GetOneByUid(declineUid);
              
              await botClient.SendTextMessageAsync
              (
                  chatId: callbackQuery.Message.Chat.Id,
                  text:  $"ID {user.Id} UID {user.UId} \n" +
                         $"Имя: {user.FirstName} \n"+
                         $"Фамилия: {user.LastName} \n" +
                         $"Моб.тел. {user.Phone} \n\n" +
                         $"Заявка на авторизацию ОТКЛОНЕНА",
                  replyMarkup:  Keyboard.Ok,
                  cancellationToken: cancellationToken
              );
              
              await _messageService.TryDeleteMessage(AdminSettings.AdminChatId, callbackQuery.Message.MessageId, botClient);

             var userMsg = await botClient.SendTextMessageAsync(
                  chatId: declineUid, 
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