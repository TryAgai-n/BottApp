using BottApp.Database.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public interface IMessageService
{
    Task SaveMessage(Message message);
    Task SaveInlineMessage(CallbackQuery callbackQuery);

    Task<Message> TryEditInlineMessage(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );
    public Task TryDeleteMessage(long userUid, int messageId, ITelegramBotClient botClient);

}