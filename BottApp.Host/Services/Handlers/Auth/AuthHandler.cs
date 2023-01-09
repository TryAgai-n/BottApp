using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.Auth
{
    public class AuthHandler : IAuthHandler
    {

        private readonly IUserRepository _userRepository;
        private readonly IMessageService _messageService;


        public AuthHandler(IUserRepository userRepository, IMessageService messageService)
        {
            _userRepository = userRepository;
            _messageService = messageService;
        }


        public async Task BotOnCallbackQueryReceived(
            ITelegramBotClient? botClient,
            CallbackQuery callbackQuery,
            CancellationToken cancellationToken,
            UserModel user
        )
        {
            if (callbackQuery.Data == "SendPhoneRequest")
            {
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id, text: " ",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken
                );
            }
        }




        public async Task BotOnMessageReceived(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken,
            UserModel user
        )
        {
            if (message.Text == "/start" && user.ViewDocumentId<0)
            {
                _messageService.TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);

                var msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Привет! Я чат бот - Чатик",
                    cancellationToken: cancellationToken);

                await Task.Delay(1500);

                msg = await botClient.EditMessageTextAsync(
                    chatId: msg.Chat.Id, msg.MessageId, text: $"{msg.Text}\n\nДавайте знакомиться!",
                    cancellationToken: cancellationToken);

                await Task.Delay(2000);

                _messageService.TryDeleteMessage(msg.Chat.Id, msg.MessageId, botClient);

                msg = await botClient.SendTextMessageAsync(
                    chatId: msg.Chat.Id, text: $"Для начала поделитесь телефоном, чтобы я мог идентифицировать вас.",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken);

                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;
            }

            if (user.Phone.IsNullOrEmpty())
            {
                Message msg;
                _messageService.TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                _messageService.TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);


                if (message.Contact != null)
                {
                    msg = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправьте свое имя",
                        cancellationToken: cancellationToken
                    );

                    await _userRepository.UpdateUserPhone(user, message.Contact.PhoneNumber);

                    await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                    return;
                }

                msg = await botClient.SendTextMessageAsync(
                    message.Chat.Id, "Отправьте телефон по кнопке 'Поделиться контактом'",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard
                );

                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;

            }

            if (user.FirstName.IsNullOrEmpty() && !user.Phone.IsNullOrEmpty())
            {
                Message msg;
                _messageService.TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                _messageService.TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);

                if (message.Text != null)
                {
                    msg = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: $"Cпасибо!\nТеперь отправьте фамилию{message.MessageId}"
                    );

                    await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                    user.FirstName = message.Text;
                    return;
                }

                msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Отправьте имя в виде текста"
                );

                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;
            }

            if (user.LastName.IsNullOrEmpty() && !user.FirstName.IsNullOrEmpty())
            {
                Message msg;
                _messageService.TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                _messageService.TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);

                if (message.Text != null)
                {
                    user.LastName = message.Text;

                    var profile = new Profile(user.FirstName, user.LastName);

                    await _userRepository.UpdateUserFullName(user, profile);

                    await SendUserFormToAdmin(botClient, message, user);

                    msg = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Отлично!\nПередал заявку на модерацию.\nОжидайте уведомление :)"
                    );

                    await _userRepository.ChangeViewMessageId(user, message.MessageId);

                    await Task.Delay(3000);
                    _messageService.TryDeleteMessage(message.Chat.Id, msg.MessageId, botClient);
                    return;
                }

                msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Отправьте фамилию в виде текста"
                );

                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;
            }



            if (message.Text != null && !user.LastName.IsNullOrEmpty() && !user.FirstName.IsNullOrEmpty() &&
                !user.Phone.IsNullOrEmpty())
            {
                _messageService.TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                _messageService.TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);

                var msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Ваши данные на проверке, не переживайте!"
                );

                await Task.Delay(3000);
                _messageService.TryDeleteMessage(message.Chat.Id, msg.MessageId, botClient);
                return;
            }

            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Если все сломалось - тыкай /start");
        }

        protected virtual async Task<FileStream?> GetDefaultUserAvatar()
        {
            const string filePath = @"Files/BOT_NO_IMAGE.jpg";
            await using FileStream? fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return fileStream;
        }


        private async Task SendUserFormToAdmin(ITelegramBotClient botClient, Message? message, UserModel user)
        {
            InputOnlineFile photo;

            var getPhotoAsync = botClient.GetUserProfilePhotosAsync(message.Chat.Id);
            if (getPhotoAsync.Result.TotalCount > 0)
                photo = getPhotoAsync.Result.Photos[0][0].FileId;
            else
                photo = await GetDefaultUserAvatar();

            await botClient.SendPhotoAsync(
                AdminSettings.AdminChatId, photo,
                $" Пользователь {user.TelegramFirstName}\n" +
                $" @{message.Chat.Username ?? "Нет публичного имени"} {user.UId}\n" + $" Моб.тел. {user.Phone}\n" +
                $" Фамилия {user.LastName}, имя {user.FirstName}\n" + $" Хочет авторизоваться в системе",
                replyMarkup: Keyboard.ApproveDeclineKeyboard
            );
        }
        
        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


        public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}