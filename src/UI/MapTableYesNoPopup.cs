using System;

namespace BetterCartographyTable.UI;

public class MapTableYesNoPopup : YesNoPopup
{
  protected MapTableYesNoPopup(string header, string text, PopupButtonCallback onYesCallback) : base(header, text, onYesCallback, UnifiedPopup.Pop)
  {
    UnifiedPopup.Push(this);
  }

  public static void Show(string header, string text, Action onYesCallback) => new MapTableYesNoPopup(header, text, () =>
  {
    onYesCallback();
    UnifiedPopup.Pop();
  });
}
