namespace Swarm.Application.DTOs;

public record class InputStateDTO(
    float DirX,
    float DirY,
    float MouseX,
    float MouseY,
    float? AimRadians,
    float AimMagnitude,
    bool FirePressed,
    bool FireHeld,
    bool DropBomb,
    bool Reload,
    bool Pause,
    bool Save,
    bool Load,
    bool Restart,
    bool Left,
    bool Right
)
{
    public override string ToString()
    {
        var parts = new List<string>();

        if (DirX != 0f) parts.Add($"DirX={DirX:F2}");
        if (DirY != 0f) parts.Add($"DirY={DirY:F2}");
        if (MouseX != 0f) parts.Add($"MouseX={MouseX:F0}");
        if (MouseY != 0f) parts.Add($"MouseY={MouseY:F0}");
        if (AimRadians.HasValue) parts.Add($"AimRadians={AimRadians.Value:F2}");
        if (AimMagnitude > 0f) parts.Add($"AimMagnitude={AimMagnitude:F2}");
        if (FirePressed) parts.Add("FirePressed");
        if (FireHeld) parts.Add("FireHeld");
        if (DropBomb) parts.Add("DropBomb");
        if (Reload) parts.Add("Reload");
        if (Pause) parts.Add("Pause");
        if (Save) parts.Add("Save");
        if (Load) parts.Add("Load");
        if (Restart) parts.Add("Restart");
        if (Left) parts.Add("Left");
        if (Right) parts.Add("Right");

        return string.Join(", ", parts);
    }
}
