namespace Swarm.Application.DTOs;

public record class HudDTO(
    int Score,
    int HP,
    string Timer,
    int NumberOfEnemiesAlive,
    string GameLevel,
    string WeaponString
    
    );