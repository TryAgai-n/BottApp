using BottApp.Database;
using BottApp.Database.User;
using BottApp.Host.Keyboards;
using BottApp.Host.SimpleStateMachine;
using Telegram.Bot;
using Telegram.Bot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Host.Services.Handlers.AdminChat
{
    public class AdminChatHandler : IAdminChatHandler
    {
        private readonly IUserRepository _userRepository;

        public AdminChatHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
            SimpleFSM FSM,
            ITelegramBotClient? botClient,
            CallbackQuery callbackQuery,
            CancellationToken cancellationToken
        )
        {
            // _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
            // await MessageManager.SaveInlineMessage(_dbContainer, callbackQuery);


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

            string s = callbackQuery.Message.Caption;

            string[] subs = s.Split('|');
            string[] DotSubs = s.Split('.');

            foreach (var sub in subs)
            {
                Console.WriteLine($"Substring: {sub}");
            }
            
            long approveID = Convert.ToInt64(subs[3]); 
            var approveFirstName = subs[1]; 
            var approvePhone = subs[5]; 
          
            var findUserByUid = await _userRepository.FindOneByUid(approveID);
            await _userRepository.UpdateUserPhone(findUserByUid, approvePhone);
            await _userRepository.ChangeOnState(findUserByUid, OnState.Menu);
            
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
                cancellationToken: cancellationToken
            );
        }
        
        public async Task AuthComplete(SimpleFSM FSM, ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken)
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