using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp.Database.Service;

public class StateService : IStateService
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageService _messageService;


    public StateService(IUserRepository userRepository, IMessageService messageService)
    {
        _userRepository = userRepository;
        _messageService = messageService;
    }


    public async Task StartState(UserModel user, OnState state, ITelegramBotClient bot, Message message)
    {
        await _messageService.DeleteMessages(bot, user.UId, message.MessageId);
        await _userRepository.ChangeOnState(user, state);

        switch (state)
        {
            case OnState.Menu:
                await MenuStart(bot, message);
                
                break;
            case OnState.Votes:
                await VotesStart(bot, message);
                break;
            case OnState.UploadCandidate:
                await UploadCandidateStart(bot, message);
                break;
            case OnState.Help:
                await HelpStart(bot, message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
    
    private async Task VotesStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

           await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Меню: Голосование", replyMarkup: Keyboard.MainVotesKeyboard
        ));
    }

    private async Task HelpStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);
        
        await _messageService.MarkMessageToDelete( await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Меню: Помощь", replyMarkup: Keyboard.ToMainMenu));
        
        await _messageService.MarkMessageToDelete( await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Подробно опишите, что у вас произошло. Вопрос будет передан в службу поддержки."));
    }

    private async Task MenuStart(ITelegramBotClient botClient, Message message)
    {

        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

        await _messageService.MarkMessageToDelete( await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Главное меню", replyMarkup: Keyboard.MainKeyboard
        ));
    }
    private async Task UploadCandidateStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);
        
         await _messageService.MarkMessageToDelete(await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, text: "Меню: Выбор номинации для кандидата", replyMarkup: Keyboard.NominationKeyboard
        ));
    }
}