using BottApp.Database.User;
using BottApp.Host.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Services.OnStateStart;

public class StateStart
{
 private  readonly IUserRepository _userRepository;
 public  StateStart(IUserRepository userRepository)
 {
  _userRepository = userRepository;
 }
 public async Task Startup(UserModel user, OnState state, ITelegramBotClient bot, Message message)
     {
      await _userRepository.ChangeOnState(user, state);
      
      var start = state switch
      {
       OnState.Menu =>  MenuStart(bot,message),
       OnState.Votes => VotesStart(bot,message),
       OnState.UploadCandidate =>  UploadCandidateStart(bot,message),
       _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
      };
     }

     private async Task<Message> VotesStart(ITelegramBotClient botClient, Message message)
    {
     await botClient.DeleteMessageAsync(
       chatId: message.Chat.Id,
       messageId: message.MessageId);
     
      await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

      return await botClient.SendTextMessageAsync(
       chatId: message.Chat.Id, 
       text: "Меню: Голосование", 
       replyMarkup: Keyboard.MainVotesKeyboardMarkup);
    }
     private async Task<Message> MenuStart(ITelegramBotClient botClient, Message message)
     {
      
      await botClient.DeleteMessageAsync(
        chatId: message.Chat.Id,
        messageId: message.MessageId);
      
      await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);
      
       return await botClient.SendTextMessageAsync(
        chatId: message.Chat.Id,
        text: "Главное меню",
        replyMarkup: Keyboard.MainKeyboardMarkup);
     }
     private async Task<Message> UploadCandidateStart(ITelegramBotClient botClient, Message message)
     {
      return await botClient.SendTextMessageAsync
      (
       chatId: message.Chat.Id,
       text: "Отправь мне фотографию своего кандидата"
      );
     }

}