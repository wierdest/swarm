namespace Swarm.Application.DTOs;

public readonly record struct HudDTO(int Score, int HP, string Timer, int NumberOfEnemiesAlive, string GameLevel);