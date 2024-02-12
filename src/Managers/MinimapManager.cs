using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model;
using BetterCartographyTable.UI;
using UnityEngine;
using static Minimap;

namespace BetterCartographyTable.Managers;

public static class MinimapManager
{
  private static SharablePins Pins { get; } = [];
  public static IEnumerable<SharablePinData> SharedPins => Pins.SharedPins;
  public static IEnumerable<SharablePinData> PublicPins => Pins.PublicPins;
  public static IEnumerable<SharablePinData> GuildPins => Pins.GuildPins;

  /// <summary>
  /// Inject <c>SharablePins</c> as <c>Minimap.instance.m_pins</c> replacement.
  /// </summary>
  public static void OnAwake() => instance.m_pins = Pins;

  public static IEnumerable<SharablePinData> GetPinsForMode(SharingMode mode)
  {
    if (mode == SharingMode.Public) return PublicPins;
    else if (mode == SharingMode.Guild) return GuildPins;
    else return []; // should never happen lul
  }

  public static void ReplaceMinimapPinsWithTablePins(MapTable mapTable)
  {
    var mode = mapTable.RetrieveModeFromZDO();
    var minimapPins = GetPinsForMode(mode);
    RemovePins(minimapPins);
    var tablePins = mapTable.RetrievePinsFromZDO();
    AddPins(tablePins);
  }

  public static void RemoveTablePins(MapTable mapTable)
  {
    var tablePins = mapTable.RetrievePinsFromZDO();
    RemovePins(tablePins);
  }

  public static void AddPins(IEnumerable<SharablePinData> pins) => pins.ToList().ForEach(pin => instance.AddPin(pin));
  public static void RemovePins(IEnumerable<SharablePinData> pins) => pins.ToList().ForEach(pin => instance.RemovePin(pin));
  public static void RemovePin(SharablePinData pin) => instance.RemovePin(pin);

  public static void HandlePinEvent(ZPackage zPackage, PinEvent pinEvent)
  {
    zPackage = zPackage.GetDecompressed();
    var pin = zPackage.ReadSharablePin();
    var existingPin = Pins.AsSharable.SingleOrDefault(p => pin.Equals(p));
    var isAdd = existingPin is null && pinEvent == PinEvent.Add;
    var isToggleChecked = existingPin is not null && pinEvent == PinEvent.ToggleChecked;
    var isRemove = existingPin is not null && pinEvent == PinEvent.Remove;

    if (isAdd) instance.AddPin(pin);
    else if (isToggleChecked) existingPin.ToggleChecked();
    else if (isRemove) instance.RemovePin(existingPin);
    else
    {
      var error = @$"Invalid minimap state: cannot handle the received PinEvent given the current minimap data.
{nameof(MinimapManager)}.{nameof(HandlePinEvent)} | pin: {pin} | isAdd: {isAdd} | isUpdate: {isToggleChecked} | isRemove: {isRemove}";
      Plugin.Logger.LogError(error);
    }
  }

  public static void OpenMap(MapTable mapTable)
  {
    var mode = mapTable.RetrieveModeFromZDO();
    MinimapManagerUI.ShowModifierKeyHintAndPinToggle(mode);
    instance.ShowPointOnMap(mapTable.transform.position);
  }

  public static void OnMapClose() => MinimapManagerUI.HideModifierKeyHintAndPinToggles();

  public static void HidePinTextInput() => instance.HidePinTextInput(false);

  public static SharablePinData GetClosestPinToMouse()
  {
    var pos = instance.ScreenToWorldPoint(Input.mousePosition);
    var radius = instance.m_removeRadius * (instance.m_largeZoom * 2f);
    return (SharablePinData)instance.GetClosestPin(pos, radius);
  }
}
