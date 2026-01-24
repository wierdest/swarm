using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swarm.Application.Config;
using Swarm.Application.Contracts;
using Swarm.Application.Extensions;
using Swarm.Infrastructure.Extensions;

namespace Swarm.Presentation;
class Program
{
    static void Main()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(cfg => cfg.AddConsole());

        var configuration = new ConfigurationBuilder()
                        .AddUserSecrets<Program>() // this works because <UserSecretsId> is in Presentation.csproj
                        .Build();


        string encryptionKey = configuration["EncryptionKey"] 
                               ?? throw new InvalidOperationException("EncryptionKey not found in user secrets.");

        // Save config
        var saveConfig = new SaveConfig(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Swarm",
                "saves"
            ),
            encryptionKey
        );

        
        services.AddApplication(saveConfig);
        services.AddLoader();
        services.AddGamePersistence();

        var provider = services.BuildServiceProvider();

        using var game = new Swarm(
            provider.GetRequiredService<IGameSessionService>(),
            provider.GetRequiredService<ILogger<Swarm>>()
        );
        
        game.Run();
    }
}
