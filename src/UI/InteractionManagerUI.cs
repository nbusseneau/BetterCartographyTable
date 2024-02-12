using System;

namespace BetterCartographyTable.UI;

public static class InteractionManagerUI
{
  private static string header;
  private static string Header => header ??= Localization.instance.Localize("$MapTableToggleMode_PopupHeader");
  private static string text;
  private static string Text => text ??= Localization.instance.Localize("$MapTableToggleMode_PopupText");

  public static void ShowToggleModePopup(Action onYesCallback)
  {
    var popup = new YesNoPopup(Header, Text, () => OnYes(onYesCallback), UnifiedPopup.Pop);
    UnifiedPopup.Push(popup);
  }

  private static void OnYes(Action onYesCallback)
  {
    onYesCallback();
    UnifiedPopup.Pop();
  }
}
