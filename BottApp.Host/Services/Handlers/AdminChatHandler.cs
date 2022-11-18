using System.Globalization;
using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers
{
    public class AdminChatHandler
    {
        private readonly IDatabaseContainer _databaseContainer;

        public AdminChatHandler(IDatabaseContainer databaseContainer)
        {
            _databaseContainer = databaseContainer;
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
          
            var findUserByUid = await _databaseContainer.User.FindOneByUid(approveId);
            await _databaseContainer.User.UpdateUserPhone(findUserByUid, approvePhone);
            await _databaseContainer.User.ChangeOnState(findUserByUid, OnState.Menu);
            
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