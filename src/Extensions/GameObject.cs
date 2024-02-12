using TMPro;
using UnityEngine;

namespace BetterCartographyTable.Extensions;

public static class GameObjectExtensions
{
  public static void SetText(this GameObject gameObject, string text)
  {
    var textMeshProUGUI = gameObject.GetComponentInChildren<TextMeshProUGUI>();
    var localizedText = Localization.instance.Localize(text);
    textMeshProUGUI.SetText(localizedText);
  }
}
