using UnityEngine;

namespace BetterCartographyTable.Extensions;

public static class KeyCodeExtensions
{
  public static string ToKeyHintString(this KeyCode keyCode) => ZInput.KeyCodeToDisplayName(keyCode);
}
