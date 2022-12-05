using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.AdminChat
{
    public class AdminChatHandler : IAdminChatHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageService _messageService;

        public AdminChatHandler(IUserRepository userRepository, IMessageService messageService)
        {
            _userRepository = userRepository;
            _messageService = messageService;
          
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
                default: await _messageService.TryEditInlineMessage(botClient, callbackQuery, cancellationToken);
                    break;
            }
        }

        public async Task Approve(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
               
            var subs = callbackQuery.Message.Caption.Split('|');
            var approveId = Convert.ToInt64(subs[3]);
            var approvePhone = subs[5]; 

         
            var findUserByUid = await _userRepository.FindOneByUid(approveId);
            await _userRepository.UpdateUserPhone(findUserByUid, approvePhone);
            await _userRepository.ChangeOnState(findUserByUid, OnState.Menu);
            
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Заявка на регистрацию Пользователя UID {findUserByUid.UId} FirstName {findUserByUid.FirstName} LastName {findUserByUid.LastName} Phone {findUserByUid.Phone}\nПРИНЯТА",
                cancellationToken: cancellationToken
            );
            
            await _messageService.DeleteMessages(botClient, findUserByUid);
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
              
              var findUserByUid = await _userRepository.FindOneByUid(declineId);

              await botClient.SendTextMessageAsync
              (
                  chatId: callbackQuery.Message.Chat.Id,
                  text: $"Заявка на регистрацию Пользователя UID {findUserByUid.UId} FirstName {findUserByUid.FirstName} LastName {findUserByUid.LastName} Phone {findUserByUid.Phone}\nОТКЛОНЕНА",
                  cancellationToken: cancellationToken
              );
              
              await _messageService.DeleteMessages(botClient, findUserByUid);
              
              return await botClient.SendTextMessageAsync
            (
                chatId: declineId,
                text: "Вы не авторизованы в системе, попробуйте позже!",
                cancellationToken: cancellationToken
            );
        }
    }
}