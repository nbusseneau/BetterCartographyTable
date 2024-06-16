using System;

namespace BetterCartographyTable.UI;

public class MapTableCantRemovePopup : YesNoPopup
{
  private MapTableCantRemovePopup(PopupButtonCallback onYesCallback) : base("$MapTable_CantRemove_PopupHeader", "$MapTable_CantRemove_PopupText", onYesCallback, UnifiedPopup.Pop)
  {
    UnifiedPopup.Push(this);
  }

  public static void Show(Action onYesCallback) => new MapTableCantRemovePopup(() =>
  {
    onYesCallback();
    UnifiedPopup.Pop();
  });
}
