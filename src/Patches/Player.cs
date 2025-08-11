using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model;
using BetterCartographyTable.Model.Managers;
using BetterCartographyTable.UI;
using HarmonyLib;
using UnityEngine;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Player))]
public static class PlayerPatches
{
  private const string TablePositionCustomDataKey = $"{Plugin.ModGUID}.TablePosition";
  private static string CurrentWorldPublicTablePositionCustomDataKey => $"Public{TablePositionCustomDataKey}.{ZNet.instance.GetWorldUID()}";
  private static string CurrentWorldGuildTablePositionCustomDataKey => $"Guild{TablePositionCustomDataKey}.{ZNet.instance.GetWorldUID()}";
  private const string PinsCustomDataKey = $"{Plugin.ModGUID}.Pins";
  private static string CurrentWorldPinsCustomDataKey => $"{PinsCustomDataKey}.{ZNet.instance.GetWorldUID()}";

  /// <summary>
  /// Store table positions and shared pins for current world in the player save's custom data.
  /// </summary>
  [HarmonyPrefix]
  [HarmonyPatch(nameof(Player.Save))]
  private static void SaveSharedPins(Player __instance)
  {
    if (__instance != Player.m_localPlayer) return;
    __instance.m_customData[CurrentWorldPublicTablePositionCustomDataKey] = MapTableManager.PublicTablePosition.ToBase64();
    __instance.m_customData[CurrentWorldGuildTablePositionCustomDataKey] = MapTableManager.GuildTablePosition.ToBase64();
    __instance.m_customData[CurrentWorldPinsCustomDataKey] = MinimapManager.SharedPins.ToBase64();
  }

  /// <summary>
  /// Retrieve table positions and shared pins for current world from the player save's custom data.
  /// </summary>
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Player.Load))]
  private static void LoadSharedPins(Player __instance)
  {
    if (__instance != Player.m_localPlayer || !Game.instance.m_firstSpawn) return;

    MapTableManager.PublicTablePosition = PositionFromBase64(__instance, CurrentWorldPublicTablePositionCustomDataKey);
    MapTableManager.GuildTablePosition = PositionFromBase64(__instance, CurrentWorldGuildTablePositionCustomDataKey);
    MinimapManager.AddPins(PinsFromBase64(__instance, CurrentWorldPinsCustomDataKey));
    // hide pins toggles if no pins are available
    MinimapUI.HideTableUI();
  }

  private static Vector3? PositionFromBase64(Player __instance, string key)
  {
    var hasValue = __instance.m_customData.TryGetValue(key, out var base64Data);
    if (!hasValue || base64Data == string.Empty) return null;
    var zPackage = new ZPackage(base64Data).Decompress();
    return zPackage.ReadVector3();
  }

  private static List<SharablePinData> PinsFromBase64(Player __instance, string key)
  {
    var hasValue = __instance.m_customData.TryGetValue(key, out var base64Data);
    if (!hasValue) return [];
    var zPackage = new ZPackage(base64Data).Decompress();
    return zPackage.ReadSharablePinDataList();
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
      MapTableYesNoPopup.Show("$MapTable_CantRemove_PopupHeader", "$MapTable_CantRemove_PopupText", () =>
      {
        s_shouldForceRemoveMapTable = true;
        __instance.RemovePiece();
        MapTableManager.ClearTablePosition(mapTableManager);
      });
    }
  }
}
