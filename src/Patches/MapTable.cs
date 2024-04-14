using BetterCartographyTable.Extensions;
using BetterCartographyTable.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(MapTable))]
public static class MapTablePatches
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(MapTable.Start))]
  private static void RegisterCustomZDODataRPCsOnMapTableStart(MapTable __instance) => __instance.OnStart();

  /// <summary>
  /// Replace default read/write actions on Cartography Table with our own.
  /// </summary>
  [HarmonyPrefix]
  [HarmonyPatch(nameof(MapTable.OnRead), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData)])]
  [HarmonyPatch(nameof(MapTable.OnRead), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData), typeof(bool)])]
  [HarmonyPatch(nameof(MapTable.OnWrite), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData)])]
  private static bool StartInteractionOnMapTableUse(MapTable __instance, Switch caller, Humanoid user, ItemDrop.ItemData item, ref bool __result)
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

  /// <summary>
  /// Replace default read/write hover text on Cartography Table with our own.
  /// </summary>
  [HarmonyPostfix]
  [HarmonyPatch(nameof(MapTable.GetReadHoverText))]
  [HarmonyPatch(nameof(MapTable.GetWriteHoverText))]
  private static void ReplaceMapTableHoverText(MapTable __instance, ref string __result)
  {
    if (__instance is null) return;
    var hoverText = InteractionManager.GetHoverText(__instance);
    __result = Localization.instance.Localize(hoverText);
  }
}
