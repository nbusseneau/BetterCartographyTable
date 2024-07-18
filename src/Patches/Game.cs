using BetterCartographyTable.Model.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Game))]
public static class GamePatches
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Game.RequestRespawn))]
  private static void InitializeGuildsManager() => GuildsManager.Initialize();
}
