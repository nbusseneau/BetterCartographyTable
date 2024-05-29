using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Model;

namespace BetterCartographyTable.Extensions;

public static class IEnumerableSharablePinDataExtensions
{
  public static ZPackage ToCompressedZPackage(this IEnumerable<SharablePinData> pins)
  {
    ZPackage zPackage = new();
    zPackage.Write(pins.Count());
    var currentPlayerID = Player.m_localPlayer.GetPlayerID();
    var networkUserId = PrivilegeManager.GetNetworkUserId();
    foreach (var pin in pins) zPackage.Write(pin, currentPlayerID, networkUserId);
    return zPackage.Compress();
  }
}
