using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.Auth
{
    public class AuthHandler : IAuthHandler
    {
        private bool _isSendPhone = false;
        private bool _isSendLastName = false;
        private bool _isSendFirstName = false;
        private bool _isAllDataGrip;
        private List<Message> _messageToRemoveList = new ();

        private readonly IUserRepository _userRepository;


        public AuthHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
            _messageToRemoveList.Add(message);
            
            if (message.Text == "/start" && !_isAllDataGrip)
            {
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
                await RemoveMessageInList(botClient);
                
                _messageToRemoveList.Add(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, 
                        text: "Ваши данные на проверке, не переживайте!"
                    )
                );
            }

            if ((message.Contact != null && !_isSendPhone))
            {
                await RemoveMessageInList(botClient);

                _messageToRemoveList.Add(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, 
                        text: "Cпасибо!\nТеперь отправь свое имя",
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
                    await RemoveMessageInList(botClient);
                    
                    _messageToRemoveList.Add(
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id, 
                            text: "Cпасибо!\nТеперь отправь фамилию"
                        )
                    );
                    
                    await _userRepository.UpdateUserFirstName(user, message.Text);
                
                    _isSendFirstName = true;
                    
                    return;
                }
                
                await Task.Delay(1000);
                
                await RemoveMessageInList(botClient);

                _messageToRemoveList.Add(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, 
                        text: "Отправьте имя в виде текста"
                    ));
                return;
            }

            if (!_isSendLastName && _isSendPhone)
            {
                if (message.Text != null)
                {
                    await RemoveMessageInList(botClient);

                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, text: "Отлично!\nПередал заявку на модерацию.\nОжидай уведомление :)"
                    );

                    await _userRepository.UpdateUserLastName(user, message.Text);

                    _isSendLastName = true;
                    _isAllDataGrip = true;

                    await SendUserFormToAdmin(botClient, message, user, AdminChatID);
                    return;
                }

                await Task.Delay(1000);

                await RemoveMessageInList(botClient);

                _messageToRemoveList.Add(
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id, 
                        text: "Отправьте фамилию в виде текста"
                    )
                );
            }
        }


        private async Task RemoveMessageInList(ITelegramBotClient botClient)
        {
            foreach (var message in _messageToRemoveList)
            {
                await botClient.DeleteMessageAsync(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId);
            }
            _messageToRemoveList.Clear();
        }


        public async Task RequestContactAndLocation(
            ITelegramBotClient botClient,
            Message? message,
            CancellationToken cancellationToken
        )
        {
            _messageToRemoveList.Add(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Привет! Мне необходимо собрать некоторую информацию, чтобы я мог идентифицировать тебя.",
                    cancellationToken: cancellationToken
                )
            );
            
            await Task.Delay(1500);
            
            _messageToRemoveList.Add(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text:
                    "Не переживай! Какие-либо данные не передаются третьим лицам и хранятся на безопасном сервере.",
                    cancellationToken: cancellationToken
                )
            );
            
            await Task.Delay(1500);
            
            _messageToRemoveList.Add(
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id, text: "Для начала нажми на кнопку\n 'Поделиться контактом'",
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
           var photo = getPhotoAsync.Result.Photos[0];

            await botClient.SendPhotoAsync(
                AdminChatID, photo[0].FileId,
                $" Пользователь |{user.TelegramFirstName}|\n" + 
                       $" @{message.From.Username} |{user.UId}|\n" +
                       $" Моб.тел. |{user.Phone}|\n" +
                       $" Фамилия {user.LastName}, имя {user.FirstName}\n" +
                       $" Хочет авторизоваться в системе",
                replyMarkup: Keyboard.ApproveDeclineKeyboardMarkup
            );
        }
    }
}