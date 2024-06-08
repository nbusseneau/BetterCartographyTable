using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BetterCartographyTable.Model;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;

namespace BetterCartographyTable;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency("org.bepinex.plugins.guilds", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
[NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
public class Plugin : BaseUnityPlugin
{
  internal const string ModGUID = "nbusseneau.BetterCartographyTable";
  private const string ModName = "BetterCartographyTable";
  private const string ModVersion = "0.5.4";

  private static ConfigEntry<SharingMode> s_mapExplorationSharingMode;
  private static ConfigEntry<KeyCode> s_modifierKey;
  private static ConfigEntry<Color> s_publicPinsColor;

  public static new ManualLogSource Logger;
  public static SharingMode MapExplorationSharingMode => s_mapExplorationSharingMode.Value;
  public static KeyCode ModifierKey => s_modifierKey.Value;
  public static bool IsModifierKeyPressed => Input.GetKey(ModifierKey);
  public static Color PublicPinsColor => s_publicPinsColor.Value;

  public void Awake()
  {
    Logger = base.Logger;
    s_mapExplorationSharingMode = Config.Bind("Behaviour", "Map exploration sharing mode", SharingMode.Public, @"Whether to share your own map exploration to cartography tables you interact with.
No matter your choice, you will always receive other players' map exploration data (if anything was shared to the table).
  - Private: never share your map exploration
  - Public: always share your map exploration with all cartography tables (both public and guild)
  - Guild: only share your map exploration with guild tables");
    s_modifierKey = Config.Bind("Keys", "Modifier key", KeyCode.LeftShift, "Modifier key to use for interacting with public or guild pins on the cartography table.");
    s_publicPinsColor = Config.Bind("UI", "Public pins color", Color.green, "Color to use for public pins.");
    SetUpConfigWatcher();

    var assembly = Assembly.GetExecutingAssembly();
    Harmony harmony = new(ModGUID);
    harmony.PatchAll(assembly);
  }

  public void OnDestroy() => Config.Save();

  private void SetUpConfigWatcher()
  {
    FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, Path.GetFileName(Config.ConfigFilePath));
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Logger.LogDebug("Attempting to reload configuration...");
      Config.Reload();
    }
    catch
    {
      Logger.LogError($"There was an issue loading {Config.ConfigFilePath}");
    }
  }
}
