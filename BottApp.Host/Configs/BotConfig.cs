namespace BottApp.Host.Configs;

public sealed class BotConfig
{
    public string Token { get; set; }

    public string HostAddress { get; init; } = default!;

    public string Route { get; init; } = default!;
    
    public string SecretToken { get; init; } = default!;
}