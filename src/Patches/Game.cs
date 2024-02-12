using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

public static class GamePatches
{
  [HarmonyPatch(typeof(Game), nameof(Game.Start))]
  private class RegisterGuildOnGameStart
  {
    private static void Postfix() => GuildsManager.OnGameStart();
  }
}
