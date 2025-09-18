using Swarm.Application.Common;

namespace Swarm.Application.Primitives;

public sealed record class SaveName
{
    public readonly string Value;

    public SaveName(string value)
    {
        Guard.ValidSaveName(value, nameof(SaveName));
        Value = value;
    }

    // implicit conversion
    public static implicit operator string(SaveName saveName) => saveName.Value;
}
