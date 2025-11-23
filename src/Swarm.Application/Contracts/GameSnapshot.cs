using System.Text;
using Swarm.Application.DTOs;

namespace Swarm.Application.Contracts;

public record class GameSnapshot(
    BoundsDTO Stage,
    PlayerDTO Player,
    HudData HudData,
    IReadOnlyList<ProjectileDTO> Projectiles,
    IReadOnlyList<EnemyDTO> Enemies,
    IReadOnlyCollection<DrawableDTO> Walls,
    DrawableDTO PlayerArea,
    DrawableDTO TargetArea,
    bool IsPaused,
    bool IsTimeUp,
    bool IsCompleted,
    bool IsInterrupted,
    bool TargetAreaIsOpenToPlayer,
    float AimPositionX,
    float AimPositionY
)
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Game Snapshot ===");
        sb.AppendLine($"Stage: {Stage}");
        sb.AppendLine($"Player: {Player}");
        sb.AppendLine($"HudData: {HudData}");

        if (Projectiles.Count > 0)
            sb.AppendLine($"Projectiles: {Projectiles.Count}");

        if (Enemies.Count > 0)
            sb.AppendLine($"Enemies: {Enemies.Count}");

        if (Walls.Count > 0)
            sb.AppendLine($"Walls: {Walls.Count}");

        sb.AppendLine($"PlayerArea: {PlayerArea}");
        sb.AppendLine($"TargetArea: {TargetArea}");

        sb.AppendLine("Flags:");
        if (IsPaused) sb.AppendLine("  • Paused");
        if (IsTimeUp) sb.AppendLine("  • TimeUp");
        if (IsCompleted) sb.AppendLine("  • Completed");
        if (IsInterrupted) sb.AppendLine("  • Interrupted");
        if (TargetAreaIsOpenToPlayer) sb.AppendLine("  • Target area is open");

        sb.AppendLine($"Aim: ({AimPositionX:F2}, {AimPositionY:F2})");

        return sb.ToString();
    }
}