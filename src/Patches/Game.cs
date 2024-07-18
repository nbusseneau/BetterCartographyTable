using BetterCartographyTable.Model.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Game))]
public static class GamePatches
{
  public static bool IsNoMapModeEnabled { get; private set; }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Game.UpdateNoMap))]
  private static void CacheNoMapMode() => IsNoMapModeEnabled = Game.m_noMap;

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Game.RequestRespawn))]
  private static void InitializeGuildsManager() => GuildsManager.Initialize();
}
