using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Model;
using Splatform;

namespace BetterCartographyTable.Extensions;

public static class IEnumerableSharablePinDataExtensions
{
  public static ZPackage ToCompressedZPackage(this IEnumerable<SharablePinData> pins)
  {
    ZPackage zPackage = new();
    zPackage.Write(pins.Count());
    var currentPlayerID = Player.m_localPlayer.GetPlayerID();
    var platformUserID = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
    foreach (var pin in pins) zPackage.Write(pin, currentPlayerID, platformUserID);
    return zPackage.Compress();
  }
}
