using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Microsoft.IdentityModel.Tokens;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace BottApp.Host.Handlers.Auth
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
            

            if (message.Text == "/start" && user.ViewMessageId == 0)
            {
                var msg1 = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Привет! Я чат бот - Чатик☘️️", ParseMode.Markdown, cancellationToken: cancellationToken
                    
                );
                
                await _messageService.TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);

                await _userRepository.ChangeViewMessageId(user, msg1.MessageId);

                await Task.Delay(1500);
                
                msg1 = await botClient.EditMessageTextAsync(
                    chatId: msg1.Chat.Id, msg1.MessageId, text: $"{msg1.Text}\n\nДавайте знакомиться!",
                    cancellationToken: cancellationToken
                );

                await Task.Delay(1500);
                
                var msg2 = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: $"Для начала поделитесь телефоном, чтобы я мог идентифицировать вас.",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken
                );
                
                await _messageService.TryDeleteMessage(msg1.Chat.Id, msg1.MessageId, botClient);
                
                await _userRepository.ChangeViewMessageId(user, msg2.MessageId);
                return;
            }

            try
            {
                Message? msg;
                switch (user)
                {
                    case {Phone : null} when message.Contact is not null:
                        await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
                        await Task.Delay(500);
                        msg = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправьте свое имя",
                            cancellationToken: cancellationToken
                        );
                        
                       
                       
                        await _userRepository.UpdateUserPhone(user, message.Contact.PhoneNumber);
                        await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                        return;

                    case {Phone : null} when message.Contact is null:
                        
                        await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
                        await Task.Delay(500);
                        msg = await botClient.SendTextMessageAsync(
                            message.Chat.Id, "Отправьте телефон по кнопке *\"Поделиться контактом\"*",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: Keyboard.RequestLocationAndContactKeyboard
                        );
                        
                       
                      
                        await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                        return;

                    case {FirstName: null} when message.Text is not null:
                        
                        await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
                        await Task.Delay(500);
                        msg = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: $"Cпасибо!\nТеперь отправьте фамилию"
                        );
                        
                      
                        
                        await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                        user.FirstName = message.Text;
                        return;

                    case {FirstName: null} when message.Text is null:
                        
                        await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
                        await Task.Delay(500);
                        msg = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "Отправьте имя *в виде текста*",
                            ParseMode.Markdown
                        );
                       
                        await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                        return;

                    case {LastName: null} when message.Text is not null:
                        user.LastName = message.Text;
                        var profile = new Profile(user.FirstName, user.LastName);
                        
                        await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
                        await Task.Delay(500);
                      
                        await _userRepository.UpdateUserFullName(user, profile);

                        await SendUserFormToAdmin(botClient, message, user);

                        msg = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Отлично!\nПередал заявку на модерацию.\nОжидайте уведомление :)"
                        );
                        
                        await botClient.DeleteMessageAsync(user.UId, message.MessageId);
                        
                        await Task.Delay(3000);
                        
                        await  _messageService.TryDeleteMessage(message.Chat.Id, msg.MessageId, botClient);
                        return;

                    case {LastName: null} when message.Text is null:
                        await botClient.DeleteMessageAsync(user.UId, user.ViewMessageId);
                        await Task.Delay(500);
                        msg = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "Отправьте фамилию *в виде текста*",
                            ParseMode.Markdown
                            
                        );

                        await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                        return;

                    default:
                        msg = await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "Ваши данные на проверке, не переживайте!"
                        );

                        await Task.Delay(3000);
                        await _messageService.TryDeleteMessage(message.Chat.Id, msg.MessageId, botClient);
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                await botClient.DeleteMessageAsync(user.UId, message.MessageId);
            }
        }


        protected virtual async Task<FileStream?> GetDefaultUserAvatar()
        {
            const string filePath = @"Files/BOT_NO_IMAGE.jpg";
            await using FileStream? fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fileStream.Close();
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
                $"ID {user.Id} UID {user.UId} \n" + 
                $"Пользователь @{message.Chat.Username ?? "Нет публичного имени"} \n" +
                $"Имя: {user.FirstName} \n"+
                $"Фамилия: {user.LastName} \n" +
                $"Моб.тел. {user.Phone} \n" +
                $"Хочет* авторизоваться в системе*",
                ParseMode.Markdown,
                replyMarkup: Keyboard.ApproveDeclineKeyboard
            );
        }


        public Task HandlePollingErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            throw new NotImplementedException();
        }


        public Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}