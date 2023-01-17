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
    

    public async Task StartState(UserModel user, OnState state, ITelegramBotClient bot)
    {
        await _userRepository.ChangeOnState(user, state);
        var result = state switch
        {
            OnState.Menu => MenuStart(bot, user),
            OnState.Help => HelpStart(bot, user),
            OnState.Votes => VotesStart(bot, user),
            OnState.UploadCandidate => UploadCandidateStart(bot, user)
        };
        await result;
    }


    private async Task MenuStart(ITelegramBotClient botClient, UserModel user)
    {
        botClient.SendChatActionAsync(chatId: user.UId, chatAction: ChatAction.Typing);

        await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: user.UId,
            text: "Главное меню",
            replyMarkup: Keyboard.MainKeyboard
        );
    }
    private async Task VotesStart(ITelegramBotClient botClient, UserModel user)
    {
        botClient.SendChatActionAsync(chatId: user.UId, chatAction: ChatAction.Typing);

        await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: user.UId,
            text: "Меню: Голосование",
            replyMarkup: Keyboard.MainVotesKeyboard
        );
    }
    private async Task HelpStart(ITelegramBotClient botClient,UserModel user)
    {
        botClient.SendChatActionAsync(chatId: user.UId, chatAction: ChatAction.Typing);

         await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: user.UId,
            text: "Меню: Помощь\n\nВы можете обратиться напрямую к администратору - @diedpie\n\n" +
                  "Либо подробно опишите, что у вас произошло. Вопрос будет передан в службу поддержки и вам ответит первый свободный специалист",
            replyMarkup: Keyboard.ToMainMenu
        );
    }
  
    private async Task UploadCandidateStart(ITelegramBotClient botClient,UserModel user)
    {
         botClient.SendChatActionAsync(chatId: user.UId, chatAction: ChatAction.Typing);
        
        await botClient.EditMessageTextAsync(
            messageId: user.ViewMessageId,
            chatId: user.UId,
            text: "Меню: Выбор номинации для загрузки кандидата",
            replyMarkup: Keyboard.NominationKeyboard
        );
    }
}