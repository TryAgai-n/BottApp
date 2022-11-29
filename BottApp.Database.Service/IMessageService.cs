using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Database.Service;

public interface IMessageService
{
    Task SaveMessage(Telegram.Bot.Types.Message message);
    Task SaveInlineMessage(CallbackQuery callbackQuery);

    Task<Telegram.Bot.Types.Message> TryEditInlineMessage(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );
    
    Task MarkMessageToDelete(Telegram.Bot.Types.Message message);
    void DeleteMessages(ITelegramBotClient botClient);
}