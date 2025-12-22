using Swarm.Domain.Common;

namespace Swarm.Domain.Entities.Missions;

public readonly record struct Difficulty
{
    public readonly int Value;

    public Difficulty(int value)
    {
        Guard.True(value >= 0 && value <= 2, "Difficulty must be 0 (Easy), 1 (Normal), or 2 (Hard).");
        Value = value;
    }

    public static Difficulty Easy => new(0);
    public static Difficulty Normal => new(1);
    public static Difficulty Hard => new(2);

    public override string ToString() => Value switch
    {
        0 => "Easy",
        1 => "Normal",
        2 => "Hard",
        _ => "Unknown"
    };
}
