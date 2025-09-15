using Swarm.Domain.Common;

namespace Swarm.Domain.GameObjects.Spawners;

public static class SpawnObjectTypesExtensions
{
    public static SpawnObjectTypes Parse(string value)
    {
        var success = Enum.TryParse<SpawnObjectTypes>(value, ignoreCase: true, out var result)
                        && Enum.IsDefined(typeof(SpawnObjectTypes), result);

        Guard.True(success, $"Invalid spawn object type: {value}");

        return result;
    }
}