using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Services;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.AdminChat
{
    public class AdminChatHandler : IAdminChatHandler
    {
        private readonly IUserRepository _userRepository;

        public AdminChatHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
            
            var action = callbackQuery.Data.Split(' ')[0] switch
            {
                "ButtonApprove" => await Approve(botClient, callbackQuery, cancellationToken),
                "ButtonDecline" => await Decline(botClient, callbackQuery, cancellationToken),
                
                _ => await MessageManager.TryEditInlineMessage(botClient, callbackQuery, cancellationToken, new Keyboard())
            };
            

        }

        public async Task<Message> Approve(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Заявка принята",
                cancellationToken: cancellationToken
            );
            
            var subs = callbackQuery.Message.Caption.Split('|');
            var approveId = Convert.ToInt64(subs[3]);
            var approvePhone = subs[5]; 
          
            var findUserByUid = await _userRepository.FindOneByUid(approveId);
            await _userRepository.UpdateUserPhone(findUserByUid, approvePhone);
            await _userRepository.ChangeOnState(findUserByUid, OnState.Menu);
            
            return await botClient.SendTextMessageAsync
            (
                chatId: approveId,
                text: "Вы авторизованы!",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
            
        }
        public async Task<Message> Decline(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
              await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Заявка отклонена",
                cancellationToken: cancellationToken
            );
              
              var subs = callbackQuery.Message.Caption.Split('|');
              var declineId = Convert.ToInt64(subs[3]);
            
            return await botClient.SendTextMessageAsync
            (
                chatId: declineId,
                text: "Вы не авторизованы в системе, попробуйте позже!",
                cancellationToken: cancellationToken
            );
        }
    }
}