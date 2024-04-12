using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

public static class GamePatches
{
  public static bool IsNoMapModeEnabled { get; private set; }

  [HarmonyPatch(typeof(Game), nameof(Game.Start))]
  private class RegisterGuildOnGameStart
  {
    private static void Postfix() => GuildsManager.OnGameStart();
  }

  [HarmonyPatch(typeof(Game), nameof(Game.UpdateNoMap))]
  private class RegisterNoMapModeOnUpdateNoMap
  {
    private static void Postfix() => IsNoMapModeEnabled = Game.m_noMap;
  }
}
