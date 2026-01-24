using Microsoft.Extensions.DependencyInjection;
using Swarm.Application.Contracts;
using Swarm.Infrastructure.Loader;

namespace Swarm.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddLoader(this IServiceCollection services)
    {
        services.AddSingleton<IGameSessionConfigLoader, GameSessionConfigLoader>();
        return services;
    }
} 
