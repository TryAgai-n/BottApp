using System.Text;
using BottApp.Database;
using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using BottApp.Host.Migrations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Int32;

namespace BottApp.Host.Services.Handlers.AdminChat
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

        public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }

        public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var prepString = message.Text.ToLower();
            
            if (prepString.Contains("/start"))
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: $"Поздравялю, {message.From.FirstName}, вы - админ\n"+
                    "Для вывода всех активных команд нажимайте /help",
                    cancellationToken: cancellationToken
                );
            }
            
            if (prepString.Contains("/view_statistic_"))
            {
                await GetVoteStatistic(botClient, message, cancellationToken, prepString);
            }
            
            if (prepString.Contains("/like_statistic_"))
            {
                await GetVoteStatistic(botClient, message, cancellationToken, prepString, false);
            }
            
            if (prepString.Contains("/send_document_"))
            {
                await SendDocument(botClient, message, cancellationToken, prepString);
            }
            
            if (prepString.Contains("/send_top_by_nomination"))
            {
                await SendTopDocument(botClient, message, cancellationToken, prepString);
            }
            
            if (prepString.Contains("/help"))
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "/start\n" +
                          "/view_statistic_[value]\n" +
                          "/like_statistic_[value]\n" +
                          "/send_document_[value]\n"+
                          "/send_top_by_nomination",
                cancellationToken: cancellationToken
                );
            }
            
            
        }

        public async Task SendTopDocument(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken, string prepString)
        {
            var documents = await _documentRepository.ListMostDocumentInVote(1, true);

            var groupByNomination = documents.ToLookup(s => s.DocumentNomination);
            
                
            foreach (var nomination in groupByNomination)
            {
                foreach (var item in documents.Where(x=> x.DocumentNomination == nomination.Key))
                {
                    await using FileStream fileStream = new(item.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat.Id,
                        photo: new InputOnlineFile(fileStream, "Document" + item.DocumentExtension),
                        caption: $"Nomination: {item.DocumentNomination}\nViewCount: {item.DocumentStatisticModel.ViewCount}\nLikeCount: {item.DocumentStatisticModel.LikeCount}\nCaption: {item.Caption}",
                        cancellationToken: cancellationToken); 
                }
               
            }
            
        }
        
        public async Task SendDocument(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken, string prepString)
        {
            try
            {
                TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
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
                TryParse(string.Join("", prepString.Where(char.IsDigit)), out var id);
                var document = await _documentRepository.GetOneByDocumentId(id);
                
                await using FileStream fileStream = new(document.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            
                await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, "Document" + document.DocumentExtension),
                    caption: $"{document.Caption}",
                    cancellationToken: cancellationToken); 
            }
            

          
        }

        public async Task GetVoteStatistic(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string prepString, bool isByView = true)
        {
            TryParse(string.Join("", prepString.Where(char.IsDigit)), out var take);
                
            if (take > 20)
                take = 20;
                 
            var listTopByNomination = await _documentRepository.ListMostDocumentInVote(take, isByView);

            var groupByNomination = listTopByNomination.ToLookup(s => s.DocumentNomination);
                
            var sb = new StringBuilder($"For {take} Candidates: \n\n");
                
            var i = 1;
                
            foreach (var nomination in groupByNomination)
            {
                sb.Append($"{nomination.Key}.\n");
                foreach (var item in listTopByNomination.Where(x=> x.DocumentNomination == nomination.Key))
                {
                    sb.Append($"{i++}." +
                              $"  ID  {item.Id}" +
                              $"  View {item.DocumentStatisticModel.ViewCount}" +
                              $"  Like {item.DocumentStatisticModel.LikeCount}\n");
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
            CancellationToken cancellationToken
        )
        {
            // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
            
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


        public async Task ApproveDocument(
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
        
        
        public async Task DeclineDocument(
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

        public async Task Approve(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
               
            var subs = callbackQuery.Message.Caption.Split('|');
            var approveId = Convert.ToInt64(subs[3]);
            var approvePhone = subs[5]; 

         
            var user = await _userRepository.FindOneByUid(approveId);
            await _userRepository.UpdateUserPhone(user, approvePhone);
            await _userRepository.ChangeOnState(user, OnState.Menu);
            
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Заявка на регистрацию Пользователя UID {user.UId} FirstName {user.FirstName} LastName {user.LastName} Phone {user.Phone}\nПРИНЯТА",
                cancellationToken: cancellationToken
            );
            
            await _messageService.DeleteMessages(botClient, AdminSettings.AdminChatId, callbackQuery.Message.MessageId);
            
            await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync
            (
                chatId: approveId,
                text: "Вы авторизованы!",
                replyMarkup: Keyboard.MainKeyboard,
                cancellationToken: cancellationToken
            ));
           

        }
        public async Task<Message> Decline(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            
              var subs = callbackQuery.Message.Caption.Split('|');
              var declineId = Convert.ToInt64(subs[3]);
              
              var user = await _userRepository.FindOneByUid(declineId);
              
              await botClient.SendTextMessageAsync
              (
                  chatId: callbackQuery.Message.Chat.Id,
                  text: $"Заявка на регистрацию Пользователя UID {user.UId} FirstName {user.FirstName} LastName {user.LastName} Phone {user.Phone}\nОТКЛОНЕНА",
                  cancellationToken: cancellationToken
              );
              
              await _messageService.DeleteMessages(botClient, AdminSettings.AdminChatId, callbackQuery.Message.MessageId);
              
              return await botClient.SendTextMessageAsync
            (
                chatId: declineId,
                text: "Вы не авторизованы в системе, попробуйте позже!",
                cancellationToken: cancellationToken
            );
        }
    }
}