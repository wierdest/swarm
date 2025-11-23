namespace Swarm.Application.Contracts;

public record class HudData(
    int Kills,
    int TargetKills,
    int HP,
    int NumberOfPlayerRespawns,
    string Timer,
    int NumberOfEnemiesAlive,
    bool LevelCompleted,
    bool HasReachedTargetKills,
    string WeaponName,
    int AmmoStock,
    int CurrentAmmo,
    int MaxAmmo,
    int BombCount,
    int NumberOfHealthyAlive,
    int Casualties,
    int NumberOfHealthySaved
)
{
    
}