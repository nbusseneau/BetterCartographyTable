using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Player))]
public static class PlayerPatches
{
  [HarmonyPrefix]
  [HarmonyPatch(nameof(Player.Save))]
  private static void SaveSharedPinsOnPlayerSave() => PlayerSaveManager.OnSave();

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Player.Load))]
  private static void LoadSharedPinsOnPlayerLoad() => PlayerSaveManager.OnLoad();

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Player.FixedUpdate))]
  private static void CloseMapWhenTooFarFromTable(Player __instance) => InteractionManager.CloseMapWhenTooFarFromTable(__instance);
}
