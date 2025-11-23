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

    private static string KillMissionHint(int amount) => $"Kill {amount} enemies!";
    private static string SaveMissionHint(int amount) => $"Save {amount} people!";

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

    public static string BuildTopLine(HudData hud)
    {
        return string.Join(" | ",
            BuildHpText(hud.HP),
            BuildDeathsText(hud.NumberOfPlayerRespawns),
            BuildWeaponText(hud.WeaponName, hud.CurrentAmmo, hud.MaxAmmo, hud.AmmoStock),
            BuildKillsText(hud.Kills, hud.TargetKills),
            BuildEnemiesText(hud.NumberOfEnemiesAlive),
            BuildHealthyAliveText(hud.NumberOfHealthyAlive),
            BuildCasualtiesText(hud.Casualties),
            BuildHealthySavedText(hud.NumberOfHealthySaved),
            BuildTimerText(hud.Timer)
        );
    }

    public static string BuildMissionText(HudData hud)
    {
        if (hud.LevelCompleted)
            return _successHint;

        if (hud.HasReachedTargetKills)
            return _levelMissionHint;

        return KillMissionHint(hud.TargetKills);
    }

    public static string BuildSaveGameString(HudData hud)
{
    var killsText = $"Kill Count: {hud.Kills} / {hud.TargetKills}";
    var hpText = $"HP: {hud.HP}";
    var playerDeathsText = $"Deaths: {hud.NumberOfPlayerRespawns}";
    var enemiesText = $"Enemies: {hud.NumberOfEnemiesAlive}";
    var weaponText = hud.WeaponName;
    var timerText = $"Timer: {hud.Timer}";
    var healthyAliveText = $"Healthy Alive: {hud.NumberOfHealthyAlive}";
    var casualtiesText = $"Casualties: {hud.Casualties}";
    var healthySavedText = $"Saved: {hud.NumberOfHealthySaved}";

    return $"{hpText} {playerDeathsText} {weaponText} {killsText} " +
           $"{enemiesText} {timerText} {healthyAliveText} " +
           $"{casualtiesText} {healthySavedText} {timerText}";
}

}
