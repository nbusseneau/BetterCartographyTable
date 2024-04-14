using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Game))]
public static class GamePatches
{
  public static bool IsNoMapModeEnabled { get; private set; }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Game.Start))]
  private static void RegisterGuildOnGameStart() => GuildsManager.OnGameStart();


  [HarmonyPostfix]
  [HarmonyPatch(nameof(Game.UpdateNoMap))]
  private static void RegisterNoMapModeOnUpdateNoMap() => IsNoMapModeEnabled = Game.m_noMap;
}
