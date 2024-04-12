using System.Collections.Generic;
using System.Reflection;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

public static class MapTablePatches
{
  [HarmonyPatch(typeof(MapTable), nameof(MapTable.Start))]
  private class RegisterCustomZDODataRPCsOnMapTableStart
  {
    private static void Postfix(MapTable __instance) => __instance.OnStart();
  }

  /// <summary>
  /// Replace default read/write actions on Cartography Table with our own.
  /// </summary>
  [HarmonyPatch]
  private class StartInteractionOnMapTableUse
  {
    private static IEnumerable<MethodInfo> TargetMethods() =>
    [
      AccessTools.DeclaredMethod(typeof(MapTable), nameof(MapTable.OnRead), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData)]),
      AccessTools.DeclaredMethod(typeof(MapTable), nameof(MapTable.OnRead), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData), typeof(bool)]),
      AccessTools.DeclaredMethod(typeof(MapTable), nameof(MapTable.OnWrite)),
    ];

    private static bool Prefix(MapTable __instance, Switch caller, Humanoid user, ItemDrop.ItemData item, ref bool __result)
    {
      var shouldRunOriginalMethod = false;
      var isItemInteraction = item is not null;
      var isInvalidZNetView = !__instance.m_nview.IsValid();
      if (isItemInteraction || isInvalidZNetView)
      {
        __result = false;
        return shouldRunOriginalMethod;
      }

      var isProtectedByWard = !PrivateArea.CheckAccess(__instance.transform.position, 0f, true, false);
      if (isProtectedByWard)
      {
        __result = true;
        return shouldRunOriginalMethod;
      }

      if (__instance is not null)
      {
        InteractionManager.OnMapTableUse(__instance, user);
      }
      __result = true;
      return shouldRunOriginalMethod;
    }
  }

  /// <summary>
  /// Replace default read/write hover text on Cartography Table with our own.
  /// </summary>
  [HarmonyPatch]
  private class ReplaceMapTableHoverText
  {
    private static IEnumerable<MethodInfo> TargetMethods() =>
    [
      AccessTools.DeclaredMethod(typeof(MapTable), nameof(MapTable.GetReadHoverText)),
      AccessTools.DeclaredMethod(typeof(MapTable), nameof(MapTable.GetWriteHoverText)),
    ];

    private static void Postfix(MapTable __instance, ref string __result)
    {
      if (__instance is null) return;
      var hoverText = InteractionManager.GetHoverText(__instance);
      __result = Localization.instance.Localize(hoverText);
    }
  }
}
