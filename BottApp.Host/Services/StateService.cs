using BottApp.Database.Service;
using BottApp.Database.Service.Keyboards;
using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Services;

public class StateService
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
        await _userRepository.ChangeOnState(user, state);
        var result = state switch
        {
            OnState.Menu => MenuStart(bot, user, message),
            OnState.Help => HelpStart(bot, user,message),
            OnState.Votes => VotesStart(bot, user, message),
            OnState.UploadCandidate => UploadCandidateStart(bot, user, message)
        };
        await result;
    }


    private async Task MenuStart(ITelegramBotClient botClient, UserModel user, Message message)
    {
        botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

        await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: message.Chat.Id,
            text: "Главное меню",
            replyMarkup: Keyboard.MainKeyboard
        );
    }
    private async Task VotesStart(ITelegramBotClient botClient, UserModel user, Message message)
    {
        botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

        await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: message.Chat.Id,
            text: "Меню: Голосование",
            replyMarkup: Keyboard.MainVotesKeyboard
        );
    }
    private async Task HelpStart(ITelegramBotClient botClient,UserModel user, Message message)
    {
        botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);

         await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: message.Chat.Id,
            text: "Меню: Помощь\n\n Подробно опишите, что у вас произошло. Вопрос будет передан в службу поддержки",
            replyMarkup: Keyboard.ToMainMenu
        );
    }
  
    private async Task UploadCandidateStart(ITelegramBotClient botClient,UserModel user, Message message)
    {
         botClient.SendChatActionAsync(chatId: message.Chat.Id, chatAction: ChatAction.Typing);
        
        await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: message.Chat.Id,
            text: "Меню: Выбор номинации для кандидата",
            replyMarkup: Keyboard.NominationKeyboard
        );
    }
}