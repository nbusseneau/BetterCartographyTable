using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterCartographyTable.Extensions;

public static class KeyCodeExtensions
{
  public static string ToKeyHintString(this KeyCode keyCode)
  {
    var (isMouseButton, mouseButton) = ZInput.KeyCodeToMouseButton(keyCode, logWarning: false);
    if (isMouseButton)
    {
      var mouseString = mouseButton switch
      {
        MouseButton.Left => "$button_mouse0",
        MouseButton.Right => "$button_mouse1",
        MouseButton.Middle => "$button_mouse2",
        MouseButton.Forward => Mouse.current?.forwardButton.displayName ?? "Mouse Forward",
        MouseButton.Back => Mouse.current?.backButton.displayName ?? "Mouse Back",
        _ => null,
      };
      if (mouseString is not null) return mouseString;
    }

    var key = ZInput.KeyCodeToKey(keyCode, logWarning: false);
    return key switch
    {
      Key.Comma => ",",
      Key.Period => ".",
      Key.Space => "$button_space",
      Key.LeftShift => "$button_lshift",
      Key.RightShift => "$button_rshift",
      Key.LeftAlt => "$button_lalt",
      Key.RightAlt => "$button_ralt",
      Key.LeftCtrl => "$button_lctrl",
      Key.RightCtrl => "$button_rctrl",
      Key.Enter => "$button_return",
      Key.NumpadEnter => "$button_return",
      Key.None => "$menu_none",
      _ => Keyboard.current?[key].displayName ?? key.ToString(),
    };
  }
}
