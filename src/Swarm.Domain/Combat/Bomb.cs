using Swarm.Domain.Time;

namespace Swarm.Domain.Combat;

public sealed class Bomb(
    string identifier,
    Cooldown cooldown
)
{
    public string Identifier { get; } = identifier;
    public Cooldown Cooldown { get; private set; } = cooldown;
    public void Tick(DeltaTime dt) => Cooldown = Cooldown.Tick(dt);
    public void Start() => Cooldown.Start();
    public bool IsReady => Cooldown.IsReady;

}
