using System;
using System.Threading.Tasks;
using BottApp.Client.Bot;
using BottApp.Database.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BottApp.Host.Controllers.Client;

public class UserController : AbstractClientController<UserController>
{
    private bool IsSendContact { get; set; }

    
    public UserController(ILogger<UserController> logger) : base(logger)
    {
        IsSendContact = false;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(BotUpdate.Response), 200)]
    public async Task Update([FromBody] BotUpdate request)
        {
            var message = request.Update.Message;

            if (request.Update.Type == UpdateType.Message)
            {
                try
                {
                    var _text = request.Update.Message.Text;
                    var _id = request.Update.Message.Chat.Id;
                    var _username = request.Update.Message.Chat.FirstName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Возникло исключение! | " + ex);
                    await request.TelegramgBotClient.SendTextMessageAsync(message.Chat.Id, "Ой-ёй, все сломаделось!");
                    return;
                }

                var id = request.Update.Message.Chat.Id;
                var firstName = request.Update.Message.Chat.FirstName;
                var userName = request.Update.Message.Chat.Username;

                if (request.Update.Message.Type == MessageType.Text)
                {

                    var preparedMessage = message.Text.ToLower();

                    if (preparedMessage.Contains("привет") || preparedMessage.Contains("/start"))
                    {
                        if (!IsSendContact)
                        {
                            await request.TelegramgBotClient.SendTextMessageAsync(id, "Привет! Отправьте ваш контакт для связи", replyMarkup: Keyboards.WelcomeKeyboard);
                            return;
                        }
                        else
                        {
                            await request.TelegramgBotClient.SendTextMessageAsync(id, $"Спасибо, {firstName}, но вы уже делились контактом. Все записано, не переживайте!", replyMarkup: Keyboards.DefaultKeyboard);
                            return;
                        }

                    }


                    if (preparedMessage.Contains("контакты"))
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Держи!", replyMarkup: Keyboards.inlineUrlKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("голосование"))
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Выбирай", replyMarkup: Keyboards.VotesKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("главное меню"))
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Выбирай", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("отправить документ"))
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Выберите документы через скрепку и отправьте в чат. Размер одного документа должен быть до 20 Мб.", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }
                    if (preparedMessage.Contains("отладка"))
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Раздел отладки", replyMarkup: Keyboards.debuggingKeyboard);
                        return;
                    }
                    if (preparedMessage.Contains("отправить контакт"))
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Жду!", replyMarkup: Keyboards.WelcomeKeyboard);
                        return;
                    }

                    if (preparedMessage.Contains("/stats"))
                    {
                        try
                        {
                            var _text = request.Update.Message.Text;
                            var _id = request.Update.Message.Chat.Id;
                            var _username = request.Update.Message.Chat.FirstName;
                           // var _phonenumber = userPhone;
                            Console.WriteLine($"{_username} | {_id} | {_text} | {"null"/*_phonenumber*/}");
                        }
                        catch
                        {
                            Console.WriteLine("Возникло исключение!");
                            return;
                        }

                        await request.TelegramgBotClient.SendTextMessageAsync(id, $"Статистика: \nbool - isSendContact: {IsSendContact}, Phone {/*userPhone ?? */"null"}");
                        return;
                    }

                    else
                    {
                        await request.TelegramgBotClient.SendTextMessageAsync(id, "Не совсем понял вас. (возможно раздел еще в разработке)", replyMarkup: Keyboards.DefaultKeyboard);
                        return;
                    }


                }

                if (request.Update.Message.Type == MessageType.Photo)
                {
                    await request.TelegramgBotClient.SendTextMessageAsync(id, "Гружу...");
                    DownloaManager.DownloadDocument(request.TelegramgBotClient, message, request.Update.Message.Type);
                    return;
                }

                if (request.Update.Message.Type == MessageType.Document)
                {
                    DownloaManager.DownloadDocument(request.TelegramgBotClient, message, request.Update.Message.Type);
                    return;
                }

                if (request.Update.Message.Type == MessageType.Voice)
                {
                    DownloaManager.DownloadDocument(request.TelegramgBotClient, message, request.Update.Message.Type);
                    await request.TelegramgBotClient.SendStickerAsync(id, Stikers.stiker1.Stiker_ID);
                    await Task.Delay(500);
                    await request.TelegramgBotClient.SendTextMessageAsync(id, $"{firstName}, я еще не умею обрабатывать такие команды, но уже учусь!");
                    return;
                }

                if (request.Update.Message.Type == MessageType.Sticker)
                {
                    await request.TelegramgBotClient.SendTextMessageAsync(id, $"{firstName}, лови айдишник стика!\n {request.Update.Message.Sticker.FileId}");
                    return;
                }

                if (request.Update.Message.Type == MessageType.Contact) //&& !isSendContact
                {
                    var userPhone = message.Contact.PhoneNumber;

                    IsSendContact = true;
                    
                    await request.TelegramgBotClient.SendTextMessageAsync(id, $"Спасибо, {firstName}, ваш  номер +{userPhone} записан! ");
                    await Task.Delay(1000);
                    await request.TelegramgBotClient.SendTextMessageAsync(id, "Теперь выберите необходимый пункт меню, я постарюсь вам помочь!");
                    await Task.Delay(1000);
                    await request.TelegramgBotClient.SendStickerAsync(id, Stikers.stiker3.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }
                else
                {
                    await request.TelegramgBotClient.SendTextMessageAsync(id, $"Спасибо, {firstName}, но вы уже делились контактом. Все записано, не переживайте!");
                    await request.TelegramgBotClient.SendStickerAsync(id, Stikers.stiker2.Stiker_ID, replyMarkup: Keyboards.DefaultKeyboard);
                    return;
                }

            }
        }


}