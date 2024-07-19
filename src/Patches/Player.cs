using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.Managers;
using BetterCartographyTable.UI;
using HarmonyLib;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Player))]
public static class PlayerPatches
{
  private const string PinsCustomDataKey = $"{Plugin.ModGUID}.Pins";
  private static string CurrentWorldPinsCustomDataKey => $"{PinsCustomDataKey}.{ZNet.instance.GetWorldUID()}";

  /// <summary>
  /// Store shared pins for current world in the player save's custom data.
  /// </summary>
  [HarmonyPrefix]
  [HarmonyPatch(nameof(Player.Save))]
  private static void SaveSharedPins(Player __instance)
  {
    if (__instance != Player.m_localPlayer) return;
    var compressedZPackage = MinimapManager.SharedPins.ToCompressedZPackage();
    var base64Data = compressedZPackage.GetBase64();
    __instance.m_customData[CurrentWorldPinsCustomDataKey] = base64Data;
  }

  /// <summary>
  /// Retrieve shared pins for current world from the player save's custom data.
  /// </summary>
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Player.Load))]
  private static void LoadSharedPins(Player __instance)
  {
    if (__instance != Player.m_localPlayer || !Game.instance.m_firstSpawn) return;
    var hasPins = __instance.m_customData.TryGetValue(CurrentWorldPinsCustomDataKey, out var base64Data);
    if (hasPins)
    {
      var zPackage = new ZPackage(base64Data).Decompress();
      var sharedPins = zPackage.ReadSharablePinDataList();
      MinimapManager.AddPins(sharedPins);
    }
    // hide pins toggles if no pins are available
    MinimapUI.HideTableUI();
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Player.UpdateStations))]
  private static void CloseMapWhenTooFarFromTable(Player __instance)
  {
    if (!MapTableManager.IsTableValid) return;
    var isTooFar = !MapTableManager.CurrentTable.IsInUseDistance(__instance);
    if (isTooFar) Minimap.instance.SetMapMode(Minimap.MapMode.Small);
  }

  private static bool s_isRemovingPiece = false;
  [HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
  private class IsRemovingPiece
  {
    private static void Prefix() => s_isRemovingPiece = true;
    private static void Postfix() => s_isRemovingPiece = false;
  }

  private static bool s_shouldForceRemoveMapTable = false;
  [HarmonyPrefix]
  [HarmonyPatch(nameof(Player.CheckCanRemovePiece))]
  private static void CheckCanRemoveMapTable(Player __instance, Piece piece, ref bool __result, ref bool __runOriginal)
  {
    // kludge to check if we're actually removing a piece, because CheckCanRemovePiece is also called when repairing
    if (!s_isRemovingPiece) return;

    // kludge to go through with vanilla checks after user has answered yes to the popup, because popup is a non-waiting
    // call and all remove logic annoyingly runs as part of Update, via raycast-based shenanigans
    if (s_shouldForceRemoveMapTable)
    {
      s_shouldForceRemoveMapTable = false;
      return;
    }

    if (piece.GetComponent<MapTable>() is { } mapTable && MapTableManager.Get(mapTable) is { } mapTableManager && mapTableManager.Pins.Any())
    {
      __runOriginal = false;
      __result = false;
      MapTableCantRemovePopup.Show(() =>
      {
        s_shouldForceRemoveMapTable = true;
        __instance.RemovePiece();
      });
    }
  }
}
