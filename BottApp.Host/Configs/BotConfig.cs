namespace BottApp.Host.Configs;

public sealed class BotConfig
{
    public static readonly string Configuration = "BotConfiguration";
    public string BotToken { get; set; } = "";
    public string HostAddress { get; init; } = default!;
}