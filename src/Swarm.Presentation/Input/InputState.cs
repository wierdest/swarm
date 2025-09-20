namespace Swarm.Presentation.Input;

public readonly record struct InputState(
    float DirX,
    float DirY,
    float MouseX,
    float MouseY,
    bool FirePressed,
    bool FireHeld,
    bool Reload,
    bool Pause,
    bool Save,
    bool Load
);
