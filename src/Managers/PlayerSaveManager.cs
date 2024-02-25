using System.Collections.Generic;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model;

namespace BetterCartographyTable.Managers;

public static class PlayerSaveManager
{
  private const string pinsKey = $"{Plugin.ModGUID}.Pins";
  private static string PinsKey => $"{pinsKey}.{ZNet.instance.GetWorldUID()}";

  public static void OnSave()
  {
    if (Player.m_localPlayer is null) return;
    StorePinsInPlayerSavefile(MinimapManager.SharedPins);
  }

  /// <summary>
  /// Store pins offline in the local player's save custom data.
  /// </summary>
  private static void StorePinsInPlayerSavefile(IEnumerable<SharablePinData> pins)
  {
    var zPackage = pins.ToZPackage();
    var base64Data = zPackage.GetBase64();
    Player.m_localPlayer.m_customData[PinsKey] = base64Data;
  }

  public static void OnLoad()
  {
    if (Player.m_localPlayer is null) return;
    var sharedPins = RetrievePinsFromPlayerSavefile();
    MinimapManager.AddPins(sharedPins);
  }

  /// <summary>
  /// Retrieve pins stored offline in the local player's save custom data.
  /// </summary>
  private static IEnumerable<SharablePinData> RetrievePinsFromPlayerSavefile()
  {
    if (!Player.m_localPlayer.m_customData.ContainsKey(PinsKey)) return [];
    var base64Data = Player.m_localPlayer.m_customData[PinsKey];
    var zPackage = new ZPackage(base64Data);
    zPackage = zPackage.GetDecompressed();
    return zPackage.ReadSharablePinList();
  }
}
