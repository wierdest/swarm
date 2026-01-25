using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        
        services.AddApplication();
        services.AddLoader();

        var provider = services.BuildServiceProvider();

        using var game = new Swarm(
            provider.GetRequiredService<IGameSessionService>()
        );
        
        game.Run();
    }
}
