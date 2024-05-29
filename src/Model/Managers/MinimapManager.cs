using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.PinClickHandlers;
using UnityEngine;
using static Minimap;

namespace BetterCartographyTable.Model.Managers;

public static class MinimapManager
{
  private static List<PinData> AllPins => instance.m_pins;
  public static IEnumerable<SharablePinData> SharablePins => AllPins.OfType<SharablePinData>();
  public static IEnumerable<PinData> PrivatePins => AllPins.Where(p => p is not SharablePinData).Concat(SharablePins.Where(p => p.IsPrivate));
  public static IEnumerable<SharablePinData> SharedPins => SharablePins.Where(p => p.IsShared);
  public static IEnumerable<SharablePinData> PublicPins
  {
    get => SharablePins.Where(p => p.IsPublic);
    set
    {
      RemovePins(PublicPins);
      AddPins(value);
    }
  }
  public static IEnumerable<SharablePinData> GuildPins
  {
    get => SharablePins.Where(p => p.IsGuild);
    set
    {
      RemovePins(GuildPins);
      AddPins(value);
    }
  }

  public static void AddPins(IEnumerable<SharablePinData> pins) => pins.ToList().ForEach(pin => instance.AddPin(pin));
  public static void RemovePins(IEnumerable<SharablePinData> pins) => pins.ToList().ForEach(pin => instance.RemovePin(pin));

  public static void HandleClick(ClickType clickType)
  {
    instance.HidePinTextInput(false);
    var pin = GetClosestPinToMouse();
    if (pin is null) return;

    if (pin.IsPrivate()) PrivatePinClickHandler.Dispatch(clickType, pin);
    else if (MapTableManager.IsTableInUse && pin is SharablePinData sharablePin && sharablePin.SharingMode == MapTableManager.CurrentTable.SharingMode)
    {
      MapTablePinClickHandler.Dispatch(clickType, sharablePin);
    }
  }

  private static PinData GetClosestPinToMouse()
  {
    var pos = instance.ScreenToWorldPoint(Input.mousePosition);
    var radius = instance.m_removeRadius * (instance.m_largeZoom * 2f);
    return instance.GetClosestPin(pos, radius);
  }
}
