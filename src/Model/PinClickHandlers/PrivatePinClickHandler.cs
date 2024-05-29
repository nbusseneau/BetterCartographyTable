using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.Managers;
using static Minimap;

namespace BetterCartographyTable.Model.PinClickHandlers;

public class PrivatePinClickHandler
{
  /// <summary>
  /// Handle clicks on private pins. Caller is expected to check that the pin is
  /// actually a private pin beforehand.
  /// </summary>
  public static void Dispatch(ClickType clickType, PinData pin)
  {
    // pins passed to this handler should be sharable pins in most cases but may
    // be vanilla pins if another mod manually adds PinData to the list, so we
    // try to play nice with them
    if (clickType == ClickType.LeftClick) LeftClick(pin);
    else if (clickType == ClickType.RightClick) RightClick(pin);
    else if (clickType == ClickType.LeftClickPlusModifier) LeftClickPlusModifier(pin);
    // do nothing for ClickType.RightClickPlusModifier: safeguard against accidental deletions
  }

  /// <summary>
  /// Toggle checked status on private pin.
  /// </summary>
  private static void LeftClick(PinData pin) => pin.ToggleChecked();

  /// <summary>
  /// Share pin to table.
  /// </summary>
  public static void LeftClickPlusModifier(PinData pin)
  {
    if (!MapTableManager.IsTableInUse || pin is not SharablePinData sharablePin || !sharablePin.IsSharable) return;
    sharablePin.SharingMode = MapTableManager.CurrentTable.SharingMode;
    instance.m_pinUpdateRequired = true;
    MapTableManager.CurrentTable.SendPinEvent(sharablePin, PinEvent.Add);
  }

  /// <summary>
  /// Remove private pin.
  /// </summary>
  public static void RightClick(PinData pin) => instance.RemovePin(pin);
}
