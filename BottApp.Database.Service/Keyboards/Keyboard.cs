using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Database.Service.Keyboards;

public class Keyboard
{

    public static InlineKeyboardMarkup MainKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Голосование 🗳 ",nameof(MenuButton.ToVotes)),
                InlineKeyboardButton.WithCallbackData("Сказать привет ✋ ", nameof(MenuButton.Hi)),
                
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Help❕ ", nameof(MenuButton.ToHelp)),
            },
        });
    
    public static InlineKeyboardMarkup MainVotesKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Выбрать номинацию", nameof(MainVoteButton.ToChooseNomination)),
                
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Добавить своего кандидата",nameof(MainVoteButton.AddCandidate)),
            },
            // third row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Back", nameof(MainVoteButton.Back)),
                InlineKeyboardButton.WithCallbackData("Help", nameof(MainVoteButton.ToHelp)),
            },
        });
    
    public static InlineKeyboardMarkup VotesKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("< ", nameof(VotesButton.Left)),
                InlineKeyboardButton.WithCallbackData(" >", nameof(VotesButton.Right)),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Like",nameof(VotesButton.Like)),
            },
            // third row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Back", nameof(VotesButton.ToVotes)),
                InlineKeyboardButton.WithCallbackData("Help", nameof(VotesButton.ToHelp)),
            },
        });
    
    public static InlineKeyboardMarkup ApproveDeclineKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Approve",nameof(AdminButton.Approve)),
                InlineKeyboardButton.WithCallbackData("Decline", nameof(AdminButton.Decline)),
            },
        });


    public static ReplyKeyboardMarkup RequestLocationAndContactKeyboard = new(
        new[]
        {
            KeyboardButton.WithRequestContact("Поделиться контактом")
        });
        
    public static InlineKeyboardMarkup NominationKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Самый забавный", nameof(NominationButton.ToFirstNomination)),
                InlineKeyboardButton.WithCallbackData("Самый круглый", nameof(NominationButton.ToSecondNomination)),
                InlineKeyboardButton.WithCallbackData("Самый пушистый", nameof(NominationButton.ToThirdNomination)),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Back", nameof(VotesButton.ToVotes)),
            },
        });

    //
    // {
    //     ResizeKeyboard = true
    //     //OneTimeKeyboard = true
    // };

    public const string usage = "Usage:\n" +
                                "/votes       - send votes keyboard\n" +
                                "/keyboard    - send custom keyboard\n" +
                                "/remove      - remove custom keyboard\n" +
                                "/photo       - send a photo\n" +
                                "/request     - request location or contact\n" +
                                "/inline_mode - send keyboard with Inline Query\n" +
                                "/help        - Раздел в разработке";
}