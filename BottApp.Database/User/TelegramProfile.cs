namespace BottApp.Database.User;

public class TelegramProfile
{
    public long UId { get; }

    public string? TelegramFirstName { get; }
    
    public string? TelegramLastName { get; }
    
    public string? Phone { get; }
    
    
    public TelegramProfile(long uId, string? telegramFirstName, string? telegramLastName, string? phone)
    {
        UId = uId;
        TelegramFirstName = telegramFirstName;
        TelegramLastName = telegramLastName;
        Phone = phone;
    }

}