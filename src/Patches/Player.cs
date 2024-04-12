using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

public static class PlayerPatches
{
  [HarmonyPatch(typeof(Player), nameof(Player.Save))]
  public static class SaveSharedPinsOnPlayerSave
  {
    private static void Prefix() => PlayerSaveManager.OnSave();
  }

  [HarmonyPatch(typeof(Player), nameof(Player.Load))]
  public static class LoadSharedPinsOnPlayerLoad
  {
    private static void Postfix() => PlayerSaveManager.OnLoad();
  }

  [HarmonyPatch(typeof(Player), nameof(Player.FixedUpdate))]
  public static class CloseMapWhenTooFarFromTable
  {
    private static void Postfix(Player __instance) => InteractionManager.CloseMapWhenTooFarFromTable(__instance);
  }
}
