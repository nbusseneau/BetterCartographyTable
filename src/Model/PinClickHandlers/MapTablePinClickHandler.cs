using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.Managers;
using static Minimap;

namespace BetterCartographyTable.Model.PinClickHandlers;

public class MapTablePinClickHandler
{
  /// <summary>
  /// Handle clicks on table pins. Caller is expected to check that the pin is
  /// actually a table pin beforehand.
  /// </summary>
  public static void Dispatch(ClickType clickType, SharablePinData pin)
  {
    if (clickType == ClickType.LeftClick) LeftClick(pin);
    else if (clickType == ClickType.LeftClickPlusModifier) LeftClickPlusModifier(pin);
    // do nothing for ClickType.RightClick: safeguard against accidental deletions
    else if (clickType == ClickType.RightClickPlusModifier) RightClickPlusModifier(pin);
  }

  /// <summary>
  /// Toggle checked status on table pin.
  /// </summary>
  public static void LeftClick(SharablePinData pin)
  {
    pin.ToggleChecked();
    MapTableManager.CurrentTable.SendPinEvent(pin, PinEvent.ToggleChecked);
  }

  /// <summary>
  /// Unshare pin from table.
  /// </summary>
  public static void LeftClickPlusModifier(SharablePinData pin)
  {
    pin.SharingMode = SharingMode.Private;
    instance.m_pinUpdateRequired = true;
    MapTableManager.CurrentTable.SendPinEvent(pin, PinEvent.Remove);
  }

  /// <summary>
  /// Remove table pin.
  /// </summary>
  public static void RightClickPlusModifier(SharablePinData pin)
  {
    instance.RemovePin(pin);
    MapTableManager.CurrentTable.SendPinEvent(pin, PinEvent.Remove);
  }
}
