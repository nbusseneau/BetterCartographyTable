using System.Collections.Generic;
using BetterCartographyTable.Model;

namespace BetterCartographyTable.Extensions;

public static class ZPackageExtensions
{
  public static ZPackage GetCompressed(this ZPackage zPackage)
  {
    var array = zPackage.GetArray();
    array = Utils.Compress(array);
    return new(array);
  }

  public static ZPackage GetDecompressed(this ZPackage zPackage)
  {
    var array = zPackage.GetArray();
    array = Utils.Decompress(array);
    return new(array);
  }

  public static SharablePinData ReadSharablePin(this ZPackage zPackage)
  {
    var ownerID = zPackage.ReadLong();
    var name = zPackage.ReadString();
    var pos = zPackage.ReadVector3();
    var type = (Minimap.PinType)zPackage.ReadInt();
    var isChecked = zPackage.ReadBool();
    var author = zPackage.ReadString();
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

  public static List<SharablePinData> ReadSharablePinList(this ZPackage zPackage)
  {
    List<SharablePinData> pins = [];
    var nbPins = zPackage.ReadInt();
    for (var i = 0; i < nbPins; i++)
    {
      var pin = zPackage.ReadSharablePin();
      pins.Add(pin);
    }
    return pins;
  }

  public static void Write(this ZPackage zPackage, SharablePinData pin, long currentPlayerID, string networkUserId)
  {
    var ownerID = (pin.m_ownerID != 0L) ? pin.m_ownerID : currentPlayerID;
    var author = (string.IsNullOrEmpty(pin.m_author) && ownerID == currentPlayerID) ? networkUserId : pin.m_author;
    zPackage.Write(ownerID);
    zPackage.Write(pin.m_name);
    zPackage.Write(pin.m_pos);
    zPackage.Write((int)pin.m_type);
    zPackage.Write(pin.m_checked);
    zPackage.Write(author);
    zPackage.Write((int)pin.SharingMode);
  }

  public static void Write(this ZPackage zPackage, SharablePinData pin)
  {
    var currentPlayerID = Player.m_localPlayer.GetPlayerID();
    var networkUserId = PrivilegeManager.GetNetworkUserId();
    zPackage.Write(pin, currentPlayerID, networkUserId);
  }
}
