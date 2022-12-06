using BottApp.Client.Bot;
using BottApp.Database;
using BottApp.Host.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BottApp.Host.Controllers.Client;

public class TestConfig : AbstractClientController<TestConfig>
{
    // private readonly BotConfig _botConfig;

    private readonly IDatabaseContainer _databaseContainer;
    
    public TestConfig(ILogger<TestConfig> logger, 
                      // BotConfig botConfig,
                      IDatabaseContainer databaseContainer) : base(logger)
    {
        // _botConfig = botConfig;
        _databaseContainer = databaseContainer;
    }

    [HttpGet]
    public void GetBotConfig()
    {
        // Console.WriteLine(_botConfig.Token);
    }


    [HttpGet]
    [ProducesResponseType(typeof(BotUpdate.Response), 200)]
    public async Task<BotUpdate.Response> CreateUser([FromBody] object request)
    {
        // var user = await _databaseContainer.User.FindOneByUid((int) request.Message.Chat.Id);
        
        
        
        return new BotUpdate.Response();
    }
    
    [HttpGet("/slowtest")]
    public async Task<string> Get(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Starting to do slow work");

        // slow async action, e.g. call external api
        await Task.Delay(10_000, cancellationToken);

        var message = "Finished slow delay of 10 seconds.";

        Logger.LogInformation(message);

        return message;
    }
    
        
    [HttpGet("/slownobrake")]
    public async Task<string> GetTest()
    {
        Logger.LogInformation("Starting to do super slow work");

        // slow async action, e.g. call external api
        await Task.Delay(10_000);

        var message = "Finished super slow delay of 10 seconds.";

        Logger.LogInformation(message);

        return message;
    }

    // [HttpGet]
    // [ProducesResponseType(typeof(GetOne.Response), 200)]
    // public async Task<GetOne.Response> CreateUser([FromBody] GetOne request)
    // {
        
        // var user = await _databaseContainer.User.FindOneByUid((int) request.Update.Message.Chat.Id);
        //
        // return new GetOne.Response(user);
    // }
}