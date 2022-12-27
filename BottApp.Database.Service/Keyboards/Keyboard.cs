using BottApp.Database.Document;
using Telegram.Bot.Types.ReplyMarkups;

namespace BottApp.Database.Service.Keyboards;

public class Keyboard
{

    public async Task<InlineKeyboardMarkup> GetDynamicVotesKeyboard(int leftButtonOffset, int rightButtonOffset, InNomination? nomination)
    {
         InlineKeyboardMarkup VotesKeyboard = new(
            new[]
            {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData((leftButtonOffset) + " <", nameof(MenuButton.Left)
                    ),
                    InlineKeyboardButton.WithCallbackData("> " + (rightButtonOffset), nameof(MenuButton.Right)
                    ),
                },
                // second row
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Like",nameof(MenuButton.Like)),
                },
                // third row
                new []
                {
                    InlineKeyboardButton.WithCallbackData("◀️ Назад", nameof(MenuButton.Votes)),
                },
            });
         
        return VotesKeyboard;
    }


        public static InlineKeyboardMarkup MainKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Голосование 🗳 ",nameof(MenuButton.Votes)),
               // InlineKeyboardButton.WithCallbackData("Сказать✋администратору  ", nameof(MenuButton.Hi)),
                
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Нужна помощь❕ ", nameof(MenuButton.Help)),
            },
        });
    
    public static InlineKeyboardMarkup MainVotesKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Выбрать номинацию", nameof(MenuButton.ChooseNomination)),
                
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Добавить своего кандидата",nameof(MenuButton.AddCandidate)),
            },
            // third row
            new []
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад", nameof(MenuButton.Back)),
            },
        });
    
    public static InlineKeyboardMarkup VotesKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("< ", nameof(MenuButton.Left)),
                InlineKeyboardButton.WithCallbackData(" >", nameof(MenuButton.Right)),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Like ❤️",nameof(MenuButton.Like)),
            },
            // third row
            new []
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад", nameof(MenuButton.Votes)),
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

    public static InlineKeyboardMarkup ApproveDeclineDocumetKeyboard = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Approve",nameof(AdminButton.DocumentApprove)),
                InlineKeyboardButton.WithCallbackData("Decline", nameof(AdminButton.DocumentDecline)),
            },
        });

    
    public static InlineKeyboardMarkup Ok = new(
        new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData("Ok",nameof(AdminButton.SendOk)),
            },
        });
    
    public static InlineKeyboardMarkup ToMainMenu = new(
        new[]
        {
            // new []
            // {
            //     InlineKeyboardButton.WithCallbackData("Задать вопрос",nameof(HelpButton.TakeAnswer)),
            // },
            
            new []
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад",nameof(MenuButton.MainMenu)),
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
            new []
            {
                InlineKeyboardButton.WithCallbackData("Самый маленький", nameof(MenuButton.BiggestNomination)),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("Самый пушистый", nameof(MenuButton.SmallerNomination)),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("Самый быстрый ковбой", nameof(MenuButton.FastestNomination)),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад", nameof(MenuButton.Votes)),
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