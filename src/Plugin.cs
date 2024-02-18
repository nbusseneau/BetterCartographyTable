using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LocalizationManager;
using UnityEngine;

namespace BetterCartographyTable;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency("org.bepinex.plugins.guilds", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
  internal const string ModGUID = "nbusseneau.BetterCartographyTable";
  private const string ModName = "BetterCartographyTable";
  private const string ModVersion = "0.1.1";

  private static ConfigEntry<KeyboardShortcut> modifierKey;
  private static ConfigEntry<Color> publicPinsColor;

  public static new ManualLogSource Logger;
  public static KeyboardShortcut ModifierKey => modifierKey.Value;
  public static bool IsModifierKeyPushed => Input.GetKey(modifierKey.Value.MainKey) && modifierKey.Value.Modifiers.All(Input.GetKey);
  public static Color PublicPinsColor => publicPinsColor.Value;

  public void Awake()
  {
    Localizer.Load();
    Logger = base.Logger;
    modifierKey = Config.Bind("Keys", "Modifier key", new KeyboardShortcut(KeyCode.LeftShift), "Modifier key to use for interacting with public or guild pins on the cartography table.");
    publicPinsColor = Config.Bind("UI", "Public pins color", Color.green, "Color to use for public pins.");
    SetUpConfigWatcher();

    var assembly = Assembly.GetExecutingAssembly();
    Harmony harmony = new(ModGUID);
    harmony.PatchAll(assembly);
  }

  public void OnDestroy() => Config.Save();

  private void SetUpConfigWatcher()
  {
    FileSystemWatcher watcher = new(Paths.ConfigPath, Path.GetFileName(Config.ConfigFilePath));
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
