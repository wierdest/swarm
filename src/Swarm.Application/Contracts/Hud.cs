namespace Swarm.Application.Contracts;

public record class Hud(
    int Kills,
    int TargetKills,
    int HP,
    int NumberOfPlayerRespawns,
    string Timer,
    int NumberOfEnemiesAlive,
    string LevelString,
    string WeaponString,
    string BombString,
    int NumberOfHealthyAlive,
    int Casualties,
    int NumberOfHealthySaved
)
{
    public string ToDisplayString()
    {
        var killsText = $"Kill Count: {Kills} / {TargetKills}";
        var hpText = $"HP: {HP}";
        var playerDeathsText = $"Deaths: {NumberOfPlayerRespawns}";
        var enemiesText = $"Enemies: {NumberOfEnemiesAlive}";
        var weaponText = WeaponString;
        var timerText = $"Timer: {Timer}";
        var healthyAliveText = $"Healthy Alive: {NumberOfHealthyAlive}";
        var casualtiesText = $"Casualties: {Casualties}";
        var healthySavedText = $"Saved: {NumberOfHealthySaved}";

        return $"{hpText} {playerDeathsText} {weaponText} {killsText} {enemiesText} {timerText}" +
                $"{healthyAliveText} {casualtiesText} {healthySavedText} {timerText}";
    }
}