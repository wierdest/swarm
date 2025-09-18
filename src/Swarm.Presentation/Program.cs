using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swarm.Application.Config;
using Swarm.Application.Contracts;
using Swarm.Application.Extensions;
using Swarm.Infrastructure.Extensions;

namespace Swarm.Presentation;
static class Program
{
    static void Main()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(cfg => cfg.AddConsole());

        // Save config
        var saveConfig = new SaveConfig(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Swarm",
                "saves"
            )
        );

        
        services.AddApplication(saveConfig);
        services.AddGameSnapshotJsonSaving();

        var provider = services.BuildServiceProvider();

        using var game = new Swarm(
            provider.GetRequiredService<IGameSessionService>(),
            provider.GetRequiredService<ILogger<Swarm>>()
        );
        
        game.Run();
    }
}
