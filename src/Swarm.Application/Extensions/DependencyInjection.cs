using Microsoft.Extensions.DependencyInjection;
using Swarm.Application.Contracts;
using Swarm.Application.Services;

namespace Swarm.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, ISaveConfig saveConfig)
    {
        services.AddSingleton(saveConfig);

        services.AddScoped<IGameSessionService, GameSessionService>();

        return services;
    }
}
