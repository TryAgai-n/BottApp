using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.Auth
{
    public class AuthHandler : IAuthHandler
    {
        private bool _isSendPhone = false;
        private bool _isSendLastName = false;
        private bool _isSendFirstName = false;
        private bool _isAllDataGrip;

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

            if (message.Text == "/start" && !_isAllDataGrip)
            {
                await SendMainPicture(botClient, message);
                await RequestContactAndLocation(botClient, message, cancellationToken);
                return;
            }

            if (message.Text == "/secretRestart")
            {
                await RequestContactAndLocation(botClient, message, cancellationToken);
                _isSendLastName = false;
                _isSendFirstName = false;
                _isSendPhone = false;
                _isAllDataGrip = false;
                return;
            }

            if (message.Text != null && _isAllDataGrip)
            {
                await _messageService.DeleteMessages(botClient, user);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Ваши данные на проверке, не переживайте!"
                    )
                );
                await Task.Delay(10000);
               await _messageService.DeleteMessages(botClient, user);
            }

            if ((message.Contact != null && !_isSendPhone))
            {
                await _messageService.DeleteMessages(botClient, user);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправь свое имя",
                        replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken
                    )
                );

                await _userRepository.UpdateUserPhone(user, message.Contact.PhoneNumber);

                _isSendPhone = true;
                return;
            }

            if (!_isSendFirstName && _isSendPhone)
            {
                if (message.Text != null)
                {
                    await _messageService.DeleteMessages(botClient, user);

                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, text: "Cпасибо!\nТеперь отправь фамилию"
                        )
                    );

                    var profile = new Profile(message.Text, null);
                    await _userRepository.UpdateUserFullName(user, profile);

                    _isSendFirstName = true;

                    return;
                }

                await Task.Delay(1000);

                await _messageService.DeleteMessages(botClient, user);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: "Отправьте имя в виде текста")
                );
                return;
            }

            if (!_isSendLastName && _isSendPhone)
            {
                if (message.Text != null)
                {
                    await _messageService.DeleteMessages(botClient, user);
                    
                    
                    var profile = new Profile(null, message.Text);

                    await _userRepository.UpdateUserFullName(user, profile);

                    _isSendLastName = true;
                    _isAllDataGrip = true;

                    await SendUserFormToAdmin(botClient, message, user, AdminChatID);
                    
                    await _messageService.MarkMessageToDelete(
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Отлично!\nПередал заявку на модерацию.\nОжидай уведомление :)"
                        )
                    );
                    await Task.Delay(10000);
                    await _messageService.DeleteMessages(botClient, user);
                    return;
                }

                await Task.Delay(1000);

                 await _messageService.DeleteMessages(botClient, user);

                await _messageService.MarkMessageToDelete(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Отправьте фамилию в виде текста"
                    )
                );
            }
            
            await _messageService.DeleteMessages(botClient, user);

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
                    "Для начала поделись телефоном, чтобы я мог идентифицировать тебя.",
                    cancellationToken: cancellationToken
                )
            );
            
            await Task.Delay(1500);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text:
                    "Не переживай! Твои данные не передаются третьим лицам и хранятся на безопасном сервере.",
                    cancellationToken: cancellationToken
                )
            );

            await Task.Delay(1500);
            
            await _messageService.MarkMessageToDelete(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Чтобы отправить телефон нажми на кнопку\n 'Поделиться контактом'",
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
               // var photo = getPhotoAsync.Result.Photos[0];
               // _messageService.MarkMessageToDelete(
               // await botClient.SendPhotoAsync(
               //     AdminChatID, photo[0].FileId,
               //     $" Пользователь |{user.TelegramFirstName}|\n" + 
               //     $" @{message.From.Username}    |{user.UId}|\n" +
               //     $" Моб.тел. |{user.Phone}|\n" +
               //     $" Фамилия {user.LastName}, имя {user.FirstName}\n" +
               //     $" Хочет авторизоваться в системе",
               //     replyMarkup: Keyboard.ApproveDeclineKeyboard
               // ));
           }
           else
           {
               // var filePath = @"Files/BOT_MAIN_PICTURE1.png";
               // await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
               // _messageService.MarkMessageToDelete(
               // await botClient.SendPhotoAsync(
               //     AdminChatID, fileStream,
               //     $" Пользователь |{user.TelegramFirstName}|\n" +
               //     $" @{message.From.Username}    |{user.UId}|\n" +
               //     $" Моб.тел. |{user.Phone}|\n" +
               //     $" Фамилия {user.LastName}, имя {user.FirstName}\n" +
               //     $" Хочет авторизоваться в системе",
               //     replyMarkup: Keyboard.ApproveDeclineKeyboard));
               //     fileStream.Close();
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