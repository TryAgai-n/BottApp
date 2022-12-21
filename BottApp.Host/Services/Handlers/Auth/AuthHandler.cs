using BottApp.Database;
using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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


        public async Task BotOnMessageReceivedVotes(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken
        )
        {
            await RequestContactAndLocation(botClient, message, cancellationToken);
        }


        public async Task BotOnMessageReceived(
            ITelegramBotClient botClient,
            Message message,
            CancellationToken cancellationToken,
            UserModel user,
            long AdminChatID
        )
        {
      
            await _messageService.MarkMessageToDelete(message);
            
            var localUser = await UserService.Get_One_User(user.Id);
            if (localUser == null)
            {
                await UserService.Add_User_To_List(user.Id);
                localUser = await UserService.Get_One_User(user.Id);
            }
            
            

             if (message.Text == "/start" && !localUser.IsAllDataGrip)
            {
                await SendMainPicture(botClient, message);
                await RequestContactAndLocation(botClient, message, cancellationToken);
                return;
            }
            
          
            
            if (message.Text == "/secretRestart")
            {
                await RequestContactAndLocation(botClient, message, cancellationToken);
                localUser.IsSendFirstName = false;
                localUser.IsSendLastName = false;
                localUser.IsSendPhone = false;
                localUser.IsAllDataGrip = false;
                return;
            }

            if (message.Text != null && localUser.IsAllDataGrip)
            {
                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Ваши данные на проверке, не переживайте!"
                    )
                );
                await Task.Delay(10000);
               await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
            }

            if ((message.Contact != null && !localUser.IsSendPhone))
            {
                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправь свое имя",
                        replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                    )
                );

                await _userRepository.UpdateUserPhone(user, message.Contact.PhoneNumber);

                localUser.IsSendPhone = true;
                return;
            }

            if (!localUser.IsSendFirstName && localUser.IsSendPhone)
            {
                if (message.Text != null)
                {
                    await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправьте фамилию"
                        )
                    );

                  
                    localUser.FirstName = message.Text;
                    localUser.IsSendFirstName = true;

                    return;
                }

                await Task.Delay(1000);

                await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Отправьте имя в виде текста")
                );
                return;
            }

            if (!localUser.IsSendLastName && localUser.IsSendPhone)
            {
                if (message.Text != null)
                {
                    await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                    localUser.LasttName = message.Text;
                    
                    var profile = new Profile(localUser.FirstName, localUser.LasttName);

                    await _userRepository.UpdateUserFullName(user, profile);

                    localUser.IsSendLastName = true;
                    localUser.IsAllDataGrip = true;

                    await SendUserFormToAdmin(botClient, message, user, AdminChatID);
                    
                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Отлично!\nПередал заявку на модерацию.\nОжидайте уведомление :)"
                        )
                    );
                    await Task.Delay(2000, cancellationToken);
                    await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);
                    return;
                }

                await Task.Delay(1000);

                 await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Отправьте фамилию в виде текста"
                    )
                );
            }
            
            await _messageService.DeleteMessages(botClient, user.UId, message.MessageId);

            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Если все сломалось - тыкай /start"
                )
            );
        }


        public async Task RequestContactAndLocation(
            ITelegramBotClient botClient,
            Message? message,
            CancellationToken cancellationToken
        )
        {
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет! Я чат бот - Чатик",
                    cancellationToken: cancellationToken
                )
            );
            
            await Task.Delay(1500);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Давай знакомиться!",
                    cancellationToken: cancellationToken
                )
            );
            
            await Task.Delay(1500);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text:
                    "Для начала поделитесь телефоном, чтобы я мог идентифицировать вас.",
                    cancellationToken: cancellationToken
                )
            );
            
            await Task.Delay(1500);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text:
                    "Не переживайте! Ваши данные не передаются третьим лицам и хранятся на безопасном сервере.",
                    cancellationToken: cancellationToken
                )
            );

            await Task.Delay(1500);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Чтобы отправить телефон нажмите на кнопку\n 'Поделиться контактом'",
                    replyMarkup: Keyboard.RequestLocationAndContactKeyboard, cancellationToken: cancellationToken
                )
            );
            
            
        }
        
        public async Task SendUserFormToAdmin(
            ITelegramBotClient botClient,
            Message? message,
            UserModel user,
            long AdminChatID
        )
        {
            //TODO: Фотография может быть null, тогда неоходимо вставлять фото-заглушку
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
               var filePath = @"Files/BOT_MAIN_PICTURE1.png";
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
            var filePath = @"Files/BOT_MAIN_PICTURE1.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await botClient.SendPhotoAsync(chatId: message.Chat.Id, new InputOnlineFile(fileStream));
        }
                
    }
}