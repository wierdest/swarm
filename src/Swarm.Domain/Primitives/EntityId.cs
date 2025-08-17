using System.Diagnostics.CodeAnalysis;

namespace Swarm.Domain.Primitives;

public readonly record struct EntityId(Guid Value)
{
    public static EntityId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static bool TryParse(string text, [MaybeNullWhen(false)] out EntityId id)
    {
        if (Guid.TryParse(text, out var g) && g != Guid.Empty)
        {
            id = new EntityId(g);
            return true;
        }

        id = default;
        return false;
    }
}
