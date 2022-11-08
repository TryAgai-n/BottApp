namespace BottApp.Host.Example.Configs;

public sealed class BotConfig
{
    public string Token { get; set; }
    
    public string HostAddress { get; init; } = default!;
}