using BottApp.Database;
using BottApp.Host.Keyboards;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers
{
    public class AdminChatHandler
    {
        public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            // _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }

        public async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, IDatabaseContainer _dbContainer)
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
            // else
            // {
            //     await botClient.SendTextMessageAsync
            //     (
            //         chatId: message.Chat.Id,
            //         text: "Что-то пошло не так :( Давай попробуем еще раз?. Напишите \n /start",
            //         cancellationToken: cancellationToken
            //     );
            //
            //     await Task.Delay(2000);
            // }
        }

        public async Task BotOnCallbackQueryReceived
        (
            SimpleFSM FSM, ITelegramBotClient? botClient, CallbackQuery callbackQuery,
            CancellationToken cancellationToken,
            IDatabaseContainer _dbContainer
        )
        {
            // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
            // await MessageManager.SaveInlineMessage(_dbContainer, callbackQuery);


            var action = callbackQuery.Data.Split(' ')[0] switch
            {
                "ButtonApprove" => await Approve(botClient, callbackQuery, cancellationToken, _dbContainer),
                "ButtonDecline" => await Decline(botClient, callbackQuery, cancellationToken),
                
                _ => await MessageManager.TryEditInlineMessage(botClient, callbackQuery, cancellationToken, new Keyboard())
            };
            

        }

        public async Task<Message> Approve(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken, IDatabaseContainer _dbContainer)
        {
            await botClient.SendTextMessageAsync
            (
                chatId: callbackQuery.Message.Chat.Id,
                text: "Заявка принята",
                cancellationToken: cancellationToken
            );

            var approveID = 875152571; // ToDo: Нужна логика обработки ID FirstName и Phone юзера из callbackQuery.Message.Caption
            var approveFirstName = " Тест"; 
            var approvePhone = "+7809001923"; 
            _dbContainer.User.CreateUser(approveID, approveFirstName, approvePhone, UserState.Menu.ToString());
            
            
            return await botClient.SendTextMessageAsync
            (
                chatId: approveID,
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
              
            var declineID = 875152571; //ToDo: Нужна логика обработки ID юзера из callbackQuery.Message.Caption
            
            return await botClient.SendTextMessageAsync
            (
                chatId: declineID,
                text: "Вы не авторизованы в системе, попробуйте позже!",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
        
        public static async Task AuthComplete(SimpleFSM FSM, ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
        {
            FSM.SetState(UserState.Menu);
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Вы авторизованы!",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken
            );
                
            await Task.Delay(1000);
                
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Главное Меню",
                replyMarkup: Keyboard.MainKeyboardMarkup,
                cancellationToken: cancellationToken
            );
        }
    }
}