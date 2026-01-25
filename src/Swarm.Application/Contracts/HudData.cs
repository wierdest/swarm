namespace Swarm.Application.Contracts;

public record class HudData(
    int Kills,
    int HP,
    int NumberOfPlayerRespawns,
    string Timer,
    int NumberOfEnemiesAlive, // zombies, shooters, infected
    bool LevelCompleted,
    bool HasReachedTargetGoal,
    string WeaponName,
    int AmmoStock,
    int CurrentAmmo,
    int MaxAmmo,
    int BombCount,
    int NumberOfHealthyAlive, // healthy and saved
    int Casualties,
    int NumberOfHealthySaved,
    int Infected,
    string GoalDescription
);