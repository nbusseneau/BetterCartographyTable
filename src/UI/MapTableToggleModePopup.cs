namespace BetterCartographyTable.UI;

public class MapTableWarningPopup : WarningPopup
{
  private MapTableWarningPopup(string header, string text) : base(header, text, UnifiedPopup.Pop)
  {
    UnifiedPopup.Push(this);
  }

  public static void Show(string header, string text) => new MapTableWarningPopup(header, text);
}
