using System;

namespace BetterCartographyTable.UI;

public class MapTableToggleModeWarningPopup : WarningPopup
{
  private MapTableToggleModeWarningPopup() : base("$MapTable_ToggleMode_PopupHeader", "$MapTable_ToggleMode_PopupText", UnifiedPopup.Pop)
  {
    UnifiedPopup.Push(this);
  }

  public static void Show() => new MapTableToggleModeWarningPopup();
}
