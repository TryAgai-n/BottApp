using System.Text;
using BottApp.Database.Document;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.AdminChat
{
    public class AdminChatHandler : IAdminChatHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageService _messageService;
        private readonly IDocumentService _documentService;

        private readonly IDocumentRepository _documentRepository;

        private string _exampleTop10 = "Top 10:\n" + "1. ID 23 View 31 Like 14\n" + "2. ID 4  View 40 Like 9 \n" +
                                      "3. ID 9  View 22 Like 7 \n" + "4. ID 2  View 34 Like 4 \n" +
                                      "5. ID 16 View 18 Like 3 \n" + "6. ID 11 View 10 Like 1 \n";

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
                    text: "Поздравялю вы - админ",
                    cancellationToken: cancellationToken
                );
            }
            
            if (prepString.Contains("/view_top_10"))
            {

                var listTop = await _documentRepository.ListMostViewedDocuments();
                var sb = new StringBuilder("Most View Candidate: \n");

                var i = 1;
                foreach (var item in listTop)
                {
                    sb.Append(
                        $"{i++}.  ID {item.Id}  View {item.DocumentStatisticModel.ViewCount}  Like {item.DocumentStatisticModel.LikeCount}\n"
                    );
                }

                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: sb.ToString(),
                    cancellationToken: cancellationToken
                );
            }
            
            if (prepString.Contains("/help"))
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "/start\n" +
                          "/view_Top_10\n",
                    cancellationToken: cancellationToken
                );
            }
            
            
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
            //TodO: Принята кем
            await botClient.EditMessageCaptionAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                caption: $"{callbackQuery.Message.Caption}\n\nПРИНЯТА @{callbackQuery.From.Username}",
                replyMarkup:  Keyboard.Ok,
                cancellationToken: cancellationToken
            );
           await _documentRepository.SetModerate(documentApproveId, true);
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
            
            await _messageService.DeleteMessages(botClient, -1001897483007);
            
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
              
              await _messageService.DeleteMessages(botClient, -1001897483007);
              
              return await botClient.SendTextMessageAsync
            (
                chatId: declineId,
                text: "Вы не авторизованы в системе, попробуйте позже!",
                cancellationToken: cancellationToken
            );
        }
    }
}