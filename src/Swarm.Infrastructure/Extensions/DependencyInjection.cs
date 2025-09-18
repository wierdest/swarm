using Microsoft.Extensions.DependencyInjection;
using Swarm.Application.Contracts;
using Swarm.Infrastructure.Persistence;

namespace Swarm.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGameSnapshotJsonSaving(this IServiceCollection services)
    {
        services.AddScoped<IGameSnapshotRepository, JsonGameSnapshotRepository>();
        return services;
    }
} 