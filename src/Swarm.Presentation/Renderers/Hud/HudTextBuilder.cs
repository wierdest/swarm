using System.Net;
using Swarm.Application.Contracts;

namespace Swarm.Presentation.Renderers.Hud;

public static class HudTextBuilder
{
    private static readonly string _findAmmoHint = "[Find some ammo!]";
    private static readonly string _reloadHint = "[Press E to reload]";
    private static readonly string _bombsHint = "[Press Q to drop bomb] | Bombs:";
    private static readonly string _successHint = "SUCCESS!";
    private static readonly string _levelMissionHint = "Reach the green area!";
    public static string BuildHpText(int hp) => $"HP: {hp}";
    public static string BuildDeathsText(int respawns) => $"D: {respawns}";
    public static string BuildWeaponText(string weaponName, int currentAmmo, int maxAmmo, int ammoStock)
    {
        if (currentAmmo == 0)
        {
            if (ammoStock == 0)
                return $"{weaponName} {_findAmmoHint}";
            else
                return $"{weaponName} {_reloadHint} | S: {ammoStock}";
        }

        return $"{weaponName} {currentAmmo}/{maxAmmo} | S: {ammoStock}";
    }
    public static string BuildKillsText(int kills, int targetKills) => $"K: {kills}/{targetKills}";
    public static string BuildEnemiesText(int enemiesAlive) => $"E: {enemiesAlive}";
    public static string BuildBombText(int amount) => $"{_bombsHint} {amount}";
    public static string BuildHealthyAliveText(int healthyAlive) => $"H: {healthyAlive}";
    public static string BuildCasualtiesText(int casualties) => $"C: {casualties}";
    public static string BuildHealthySavedText(int saved) => $"S: {saved}";
    public static string BuildTimerText(string timer) => $"T: {timer}";
    public static string BuildInfectedText(int inffected) => $"I: {inffected}";


    public static string BuildTopLine(GameSessionData hud)
    {
        return string.Join(" | ",
            BuildHpText(hud.HP),
            BuildDeathsText(hud.NumberOfPlayerRespawns),
            BuildWeaponText(hud.WeaponName, hud.CurrentAmmo, hud.MaxAmmo, hud.AmmoStock),
            BuildEnemiesText(hud.NumberOfEnemiesAlive),
            BuildHealthyAliveText(hud.NumberOfHealthyAlive),
            BuildCasualtiesText(hud.Casualties),
            BuildHealthySavedText(hud.NumberOfHealthySaved),
            BuildInfectedText(hud.Infected),
            BuildTimerText(hud.Timer)
        );
    }

    public static string BuildMissionText(GameSessionData hud)
    {
        if (hud.LevelCompleted)
            return _successHint;

        if (hud.HasReachedTargetGoal)
            return _levelMissionHint;

        return "TODO Goal Hint here!";
    }

    public static string BuildSaveGameString(GameSessionData hud)
{
    var hpText = $"HP: {hud.HP}";
    var playerDeathsText = $"Deaths: {hud.NumberOfPlayerRespawns}";
    var enemiesText = $"Enemies: {hud.NumberOfEnemiesAlive}";
    var weaponText = hud.WeaponName;
    var timerText = $"Timer: {hud.Timer}";
    var healthyAliveText = $"Healthy Alive: {hud.NumberOfHealthyAlive}";
    var casualtiesText = $"Casualties: {hud.Casualties}";
    var healthySavedText = $"Saved: {hud.NumberOfHealthySaved}";

    return $"{hpText} {playerDeathsText} {weaponText} " +
           $"{enemiesText} {timerText} {healthyAliveText} " +
           $"{casualtiesText} {healthySavedText} {timerText}";
}

}
