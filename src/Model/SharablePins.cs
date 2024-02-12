using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using static Minimap;

namespace BetterCartographyTable.Model;

/// <summary>
/// <c>List&lt;SharablePinData&gt;</c> intended to replace <c>Minimap.instance.m_pins</c> with an
/// <c>HashSet</c>-like (no duplicates) implementation. It extends <c>List&lt;PinData&gt;</c> so as
/// to replace <c>Minimap.instance.m_pins</c>, but actually should only hold <c>SharablePinData</c>
/// objects thanks to a Harmony patch making sure calls to <c>Minimap.AddPin(...)</c> always add
/// <c>SharablePinData</c> pins.
/// </summary>
public class SharablePins : List<PinData>
{
  public IEnumerable<SharablePinData> AsSharable => this.Cast<SharablePinData>();
  public IEnumerable<SharablePinData> SharedPins => this.AsSharable.Where(p => p.IsShared);
  public IEnumerable<SharablePinData> PublicPins => this.AsSharable.Where(p => p.IsPublic);
  public IEnumerable<SharablePinData> GuildPins => this.AsSharable.Where(p => p.IsGuild);

  public ZPackage ToZPackage()
  {
    return this.AsSharable.ToZPackage();
  }

  /// <summary>
  /// Adds a pin to the end of the list if the list does not already contain this pin, and forcibly
  /// set pin as if it was created locally. Since we can't <c>override</c> the base <c>Add(...)</c>
  /// operation, we must combine using <c>new</c> and a Harmony patch to make sure calls to
  /// <c>Minimap.AddPin(...)</c> always use our implementation.
  /// </summary>
  /// <param name="pin"></param>
  public new void Add(PinData pin)
  {
    if (this.Contains(pin)) return;
    pin.m_ownerID = 0L; // always set pin as if it was created locally using ownerID = 0L, since we replace the sharing mechanism entirely
    base.Add(pin);
  }
}

public static class IEnumerableSharablePinDataExtensions
{
  public static ZPackage ToZPackage(this IEnumerable<SharablePinData> pins)
  {
    ZPackage zPackage = new();
    zPackage.Write(pins.Count());
    var currentPlayerID = Player.m_localPlayer.GetPlayerID();
    var networkUserId = PrivilegeManager.GetNetworkUserId();
    foreach (var pin in pins) zPackage.Write(pin, currentPlayerID, networkUserId);
    return zPackage.GetCompressed();
  }
}
