using BetterCartographyTable.Model.Managers;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(MapTable))]
public static class MapTablePatches
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(MapTable.Start))]
  private static void RegisterMapTable(MapTable __instance) => MapTableManager.Add(__instance);

  /// <summary>
  /// Replace default read/write actions on MapTable with our own.
  /// </summary>
  [HarmonyPrefix]
  [HarmonyPatch(nameof(MapTable.OnRead), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData)])]
  [HarmonyPatch(nameof(MapTable.OnRead), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData), typeof(bool)])]
  [HarmonyPatch(nameof(MapTable.OnWrite), [typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData)])]
  private static void OnUse(MapTable __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    var isItemInteraction = item is not null;
    var isInvalidZNetView = !__instance.m_nview.IsValid();
    if (isItemInteraction || isInvalidZNetView)
    {
      __result = false;
      return;
    }

    var isProtectedByWard = !PrivateArea.CheckAccess(__instance.transform.position);
    if (isProtectedByWard)
    {
      __result = true;
      return;
    }

    MapTableManager.TryOpen(__instance, user);
    __result = true;
  }

  /// <summary>
  /// Replace default read/write hover text on MapTable with our own.
  /// </summary>
  [HarmonyPrefix]
  [HarmonyPatch(nameof(MapTable.GetReadHoverText))]
  [HarmonyPatch(nameof(MapTable.GetWriteHoverText))]
  private static void GetHoverText(MapTable __instance, ref string __result, ref bool __runOriginal)
  {
    __runOriginal = false;
    var isProtectedByWard = !PrivateArea.CheckAccess(__instance.transform.position, flash: false);
    if (isProtectedByWard)
    {
      __result = Localization.instance.Localize(__instance.m_name + "\n$piece_noaccess"); ;
      return;
    }

    var hoverText = MapTableManager.GetHoverText(__instance);
    __result = Localization.instance.Localize(hoverText);
  }
}
