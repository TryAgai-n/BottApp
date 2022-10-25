using System;
using Microsoft.Extensions.Configuration;

namespace BottApp.Host.Configs;

internal class ConfigValidator
{
    private static void ValidateConfig(IConfig config, string configName)
    {
        if (config == null)
        {
            throw new Exception(configName + " <-- This config is null. Update appsettings.json config file");
        }
    }

    public static T GetConfig<T>(IConfiguration configuration, string configName) where T : IConfig
    {
        var config = configuration.GetSection(configName).Get<T>();
        ValidateConfig(config, configName);
        
        return config;
    }
}