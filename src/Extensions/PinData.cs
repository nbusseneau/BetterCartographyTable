using BetterCartographyTable.Model;
using static Minimap;

namespace BetterCartographyTable.Extensions;

public static class PinDataExtensions
{
  public static bool IsPrivate(this PinData pin) => (pin is SharablePinData sharablePin && sharablePin.IsPrivate) || pin is not SharablePinData;
  public static bool IsSharable(this PinData pin) => SharablePinData.IsSharable(pin);
  public static void ToggleChecked(this PinData pin)
  {
    pin.m_checked = !pin.m_checked;
    instance.m_pinUpdateRequired = true;
  }
}
