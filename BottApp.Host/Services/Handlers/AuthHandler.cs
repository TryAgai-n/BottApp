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
    public class AuthHandler
    {
        private async Task BotOnMessageReceivedVotes(SimpleFSM FSM, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await RequestContactAndLocation(botClient, message, cancellationToken);
        }

        public async Task BotOnMessageReceived(SimpleFSM FSM, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, IDatabaseContainer _dbContainer, long AdminChatID)
        {
            if ((message.Contact != null))
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "Ваши данные проходят модерацию",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken
                );

                var getPhotoAsync = botClient.GetUserProfilePhotosAsync(message.Chat.Id);
                var photo = getPhotoAsync.Result.Photos[1];
                
                await botClient.SendPhotoAsync(
                    chatId: AdminChatID,
                    photo: photo[0].FileId ,
                    caption: $" Пользователь {message.Chat.FirstName}\n {message.From}\n Моб.тел. {message.Contact.PhoneNumber}\n Хочет авторизоваться в системе " +
                             $"{message.Caption}",
                    replyMarkup: Keyboard.ApproveDeclineKeyboardMarkup,
                    cancellationToken: cancellationToken);
                return;
            }
            if(message.Text == "/start")
            {
                await RequestContactAndLocation(botClient, message, cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync
                (
                    chatId: message.Chat.Id,
                    text: "Что-то пошло не так :( Давай попробуем еще раз?",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard,
                    cancellationToken: cancellationToken
                );

                await Task.Delay(2000);
                
                await RequestContactAndLocation(botClient, message, cancellationToken);
            }
        }
        
        public static async Task RequestContactAndLocation( ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Привет! Мне необходим твой номер телефона, чтобы я мог идентифицировать тебя.",
                cancellationToken: cancellationToken
            );

            await Task.Delay(750);

            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Не переживай! Твои данные не передаются третьим лицам и хранятся на безопасном сервере =)",
                cancellationToken: cancellationToken
            );

            await Task.Delay(1500);

            await botClient.SendTextMessageAsync
            (
                chatId: message.Chat.Id,
                text: "Нажми на кнопку 'Поделиться контактом' ниже",
                replyMarkup: Keyboard.RequestLocationAndContactKeyboard,
                cancellationToken: cancellationToken
            );
        }
    }
}