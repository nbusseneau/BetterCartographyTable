using System.Collections.Generic;
using BetterCartographyTable.Model;
using Splatform;

namespace BetterCartographyTable.Extensions;

public static class ZPackageExtensions
{
  /// <summary>
  /// Utility function for compressing a ZPackage. Does not edit in place, use the returned instance.
  /// </summary>
  public static ZPackage Compress(this ZPackage zPackage)
  {
    var array = zPackage.GetArray();
    array = Utils.Compress(array);
    return new(array);
  }

  /// <summary>
  /// Utility function for decompressing a ZPackage. Does not edit in place, use the returned instance.
  /// </summary>
  public static ZPackage Decompress(this ZPackage zPackage)
  {
    var array = zPackage.GetArray();
    array = Utils.Decompress(array);
    return new(array);
  }

  public static SharablePinData ReadSharablePinData(this ZPackage zPackage)
  {
    var ownerID = zPackage.ReadLong();
    var name = zPackage.ReadString();
    var pos = zPackage.ReadVector3();
    var type = (Minimap.PinType)zPackage.ReadInt();
    var isChecked = zPackage.ReadBool();
    var author = new PlatformUserID(zPackage.ReadString());
    var sharingMode = (SharingMode)zPackage.ReadInt();
    return new SharablePinData()
    {
      m_name = name,
      m_type = type,
      m_pos = pos,
      m_ownerID = ownerID,
      m_author = author,
      m_checked = isChecked,
      SharingMode = sharingMode,
    };
  }

  public static List<SharablePinData> ReadSharablePinDataList(this ZPackage zPackage)
  {
    List<SharablePinData> pins = [];
    var nbPins = zPackage.ReadInt();
    for (var i = 0; i < nbPins; i++)
    {
      var pin = zPackage.ReadSharablePinData();
      pins.Add(pin);
    }
    return pins;
  }

  public static void Write(this ZPackage zPackage, SharablePinData pin, long currentPlayerID, PlatformUserID platformUserID)
  {
    var ownerID = (pin.m_ownerID != 0L) ? pin.m_ownerID : currentPlayerID;
    var author = (!pin.m_author.IsValid && ownerID == currentPlayerID) ? platformUserID : pin.m_author;
    zPackage.Write(ownerID);
    zPackage.Write(pin.m_name);
    zPackage.Write(pin.m_pos);
    zPackage.Write((int)pin.m_type);
    zPackage.Write(pin.m_checked);
    zPackage.Write(author.ToString());
    zPackage.Write((int)pin.SharingMode);
  }

  public static void Write(this ZPackage zPackage, SharablePinData pin)
  {
    var currentPlayerID = Player.m_localPlayer.GetPlayerID();
    var platformUserID = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
    zPackage.Write(pin, currentPlayerID, platformUserID);
  }
}
