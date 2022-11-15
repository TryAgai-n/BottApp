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
            if(prepString.Contains("/start"))
            {
               
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "Поздравялю вы админ",
                    cancellationToken: cancellationToken
                );

            }
            else
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "Что-то пошло не так :( Давай попробуем еще раз?. Напишите \n /start",
                    cancellationToken: cancellationToken
                );
                
                await Task.Delay(2000);
            }
        }
    }
}