namespace Swarm.Application.Contracts;

public record class HudData(
    int Kills,
    int TargetKills,
    int HP,
    int NumberOfPlayerRespawns,
    string Timer,
    int NumberOfEnemiesAlive, // zombies, shooters, infected
    bool LevelCompleted,
    bool HasReachedTargetKills,
    string WeaponName,
    int AmmoStock,
    int CurrentAmmo,
    int MaxAmmo,
    int BombCount,
    int NumberOfHealthyAlive, // healthy and saved
    int Casualties,
    int NumberOfHealthySaved,
    int Infected
)
{
    
}