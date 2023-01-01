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

        public async Task BotOnCallbackQueryReceived(ITelegramBotClient? botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken, UserModel user)
        {
            if (callbackQuery.Data == "SendPhoneRequest")
            {
                await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id,
                    text: " ",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard,
                    cancellationToken: cancellationToken);
            }
        }


        private async void TryDeleteMessage(long userUid, int messageId, ITelegramBotClient botClient)
        {
            try
            {
                await botClient.DeleteMessageAsync(userUid, messageId);
            }
            catch
            {
                // ignored
            }
        }
        
        public async Task BotOnMessageReceived(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken,
            UserModel user
        )
        {
            if (message.Text == "/start" & user.Phone.IsNullOrEmpty())
            {
                TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);

               var msg = await botClient.SendTextMessageAsync(
                   chatId: message.Chat.Id,
                   text: "Привет! Я чат бот - Чатик",
                   cancellationToken: cancellationToken
               );

               // await Task.Delay(1500);

               msg = await botClient.EditMessageTextAsync(
                   chatId: msg.Chat.Id, msg.MessageId, text: $"{msg.Text}\n\nДавайте знакомиться!",
                   cancellationToken: cancellationToken
               );

               // await Task.Delay(2000);

               TryDeleteMessage(msg.Chat.Id, msg.MessageId, botClient);
               
               msg = await botClient.SendTextMessageAsync(
                   chatId: msg.Chat.Id,
                   text: $"Для начала поделитесь телефоном, чтобы я мог идентифицировать вас.",
                   replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken
               );

               await _userRepository.ChangeViewMessageId(user, msg.MessageId);
               return;
            }
            
            if (user.Phone.IsNullOrEmpty())
            {
                Message msg;
                TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);
              
                
                if (message.Contact != null)
                {
                    msg = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Cпасибо!\nТеперь отправьте свое имя",
                        cancellationToken: cancellationToken);
                    
                    await _userRepository.UpdateUserPhone(user, message.Contact.PhoneNumber);
                    
                    await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                    return;
                }

                msg = await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    "Отправьте телефон по кнопке 'Поделиться контактом'",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard);
                
                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;
                 
            }

            if (user.FirstName.IsNullOrEmpty() && !user.Phone.IsNullOrEmpty())
            {
                Message msg;
                TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);
                
                if (message.Text != null)
                {
                    msg = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"Cпасибо!\nТеперь отправьте фамилию{message.MessageId}");
                    
                    await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                    user.FirstName = message.Text;
                    return;
                }

                msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Отправьте имя в виде текста");
                
                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;
            }

            if (user.LastName.IsNullOrEmpty() && !user.FirstName.IsNullOrEmpty())
            {
                Message msg;
                TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);
                
                if (message.Text != null)
                {
                    user.LastName = message.Text;
                    
                    var profile = new Profile(user.FirstName, user.LastName);

                    await _userRepository.UpdateUserFullName(user, profile);

                    await SendUserFormToAdmin(botClient, message, user);

                    msg = await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Отлично!\nПередал заявку на модерацию.\nОжидайте уведомление :)");
                    
                    await _userRepository.ChangeViewMessageId(user, message.MessageId);
                    
                    await Task.Delay(3000);
                    TryDeleteMessage(message.Chat.Id, msg.MessageId, botClient);
                    return;
                }

                msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Отправьте фамилию в виде текста"
                );
             
                await _userRepository.ChangeViewMessageId(user, msg.MessageId);
                return;
            }
            
          
            
            if (message.Text != null && !user.LastName.IsNullOrEmpty() && !user.FirstName.IsNullOrEmpty() && !user.Phone.IsNullOrEmpty())
            {
                Message msg;
                TryDeleteMessage(message.Chat.Id, message.MessageId, botClient);
                TryDeleteMessage(message.Chat.Id, user.ViewMessageId, botClient);
                
               msg = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Ваши данные на проверке, не переживайте!");
               
               await Task.Delay(3000);
               TryDeleteMessage(message.Chat.Id, msg.MessageId, botClient);
               return;
            }


            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Если все сломалось - тыкай /start");
        }
        
        
        public async Task SendUserFormToAdmin(
            ITelegramBotClient botClient,
            Message? message,
            UserModel user
        )
        {
            var getPhotoAsync = botClient.GetUserProfilePhotosAsync(message.Chat.Id);
           
           if (getPhotoAsync.Result.TotalCount > 0)
           {
               var photo = getPhotoAsync.Result.Photos[0];
               await _messageService.MarkMessageToDelete(
               await botClient.SendPhotoAsync(
                   AdminSettings.AdminChatId, photo[0].FileId,
                   $" Пользователь |{user.TelegramFirstName}|\n" + 
                   $" @{message.Chat.Username??"Нет публичного имени"}    |{user.UId}|\n" +
                   $" Моб.тел. |{user.Phone}|\n" +
                   $" Фамилия {user.LastName}, имя {user.FirstName}\n" +
                   $" Хочет авторизоваться в системе",
                   replyMarkup: Keyboard.ApproveDeclineKeyboard
               ));
           }
           else
           {
               var filePath = @"Files/BOT_NO_IMAGE.jpg";
               await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
               await _messageService.MarkMessageToDelete(
               await botClient.SendPhotoAsync(
                   AdminSettings.AdminChatId, fileStream,
                   $" Пользователь |{user.TelegramFirstName}|\n" +
                   $" @{message.Chat.Username??"Нет публичного имени"}    |{user.UId}|\n" +
                   $" Моб.тел. |{user.Phone}|\n" +
                   $" Фамилия {user.LastName}, имя {user.FirstName}\n" +
                   $" Хочет авторизоваться в системе",
                   replyMarkup: Keyboard.ApproveDeclineKeyboard));
                   fileStream.Close();
           }
        }
        
        private static async Task SendMainPicture(ITelegramBotClient botClient, Message? message)
        {  
            var filePath = @"Files/BOT_MAIN_WELCOME_PICTURE.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await botClient.SendPhotoAsync(chatId: message.Chat.Id, new InputOnlineFile(fileStream));
        }
                
    }
}