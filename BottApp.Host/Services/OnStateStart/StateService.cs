using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Services.OnStateStart;

public class StateService
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageService _messageService;


    public StateService(IUserRepository userRepository, IMessageService messageService)
    {
        _userRepository = userRepository;
        _messageService = messageService;
    }


    public async Task Startup(UserModel user, OnState state, ITelegramBotClient bot, Message message)
    {
         _messageService.DeleteMessages(bot);
        await _userRepository.ChangeOnState(user, state);

        var start = state switch
        {
            OnState.Menu => MenuStart(bot, message),
            OnState.Votes => VotesStart(bot, message),
            OnState.UploadCandidate => UploadCandidateStart(bot, message),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
    
    private async Task VotesStart(ITelegramBotClient botClient, Message message)
    {
        // await botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId);

        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

           await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Меню: Голосование", replyMarkup: Keyboard.MainVotesKeyboard
        ));
    }


    private async Task MenuStart(ITelegramBotClient botClient, Message message)
    {
        // await botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId);

        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

        await _messageService.MarkMessageToDelete( await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Главное меню", replyMarkup: Keyboard.MainKeyboard
        ));
    }
    private async Task UploadCandidateStart(ITelegramBotClient botClient, Message message)
    {
        // await botClient.DeleteMessageAsync(chatId: message.Chat.Id, messageId: message.MessageId);

        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);
        
         await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Выберете номинацию", replyMarkup: Keyboard.NominationKeyboard
        ));
    }
}