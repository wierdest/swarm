namespace Swarm.Application.DTOs;

public readonly record struct HudDTO(int Score, int HP, int NumberOfEnemiesAlive, string GameLevel);