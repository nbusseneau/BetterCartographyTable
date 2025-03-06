using BetterCartographyTable.Model;
using Splatform;
using UnityEngine;
using static Minimap;

namespace BetterCartographyTable.Extensions;

public static class MinimapExtensions
{
  public static SharablePinData AddPin(this Minimap minimap, SharablePinData pin) => minimap.AddPin(pin.m_pos, pin.m_type, pin.m_name, true, pin.m_checked, pin.m_ownerID, pin.m_author, pin.SharingMode);

  private static SharablePinData AddPin(this Minimap minimap, Vector3 pos, PinType type, string name, bool save, bool isChecked, long ownerID = 0L, PlatformUserID author = default, SharingMode sharingMode = SharingMode.Private)
  {
    var pin = (SharablePinData)minimap.AddPin(pos, type, name, save, isChecked, ownerID, author);
    pin.SharingMode = sharingMode;
    return pin;
  }
}
