namespace UserService.Helper.Telegram;

public class TelegramBotOptions
{
    public static string Position { get { return "TelegramBot"; } }
    public string? ApiToken { get; set; }
    public long ChatId { get; set; }
    
    public string? Username { get; set; }
}