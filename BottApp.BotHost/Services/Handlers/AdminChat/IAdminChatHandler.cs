using Telegram.Bot;
using Telegram.Bot.Types;

namespace BottApp.Host.Services.Handlers.AdminChat;

public interface IAdminChatHandler
{
    Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken);

    Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken);

    Task BotOnCallbackQueryReceived(
        ITelegramBotClient? botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );


    Task<Message> Approve(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );


    Task<Message> Decline(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken
    );
}