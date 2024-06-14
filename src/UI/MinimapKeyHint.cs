using BetterCartographyTable.Extensions;
using UnityEngine;

namespace BetterCartographyTable.UI;

public class MinimapKeyHint(GameObject keyHint)
{
  private readonly GameObject _gameObject = keyHint;
  public string Text { set => this._gameObject.SetText(value); }

  public static MinimapKeyHint Clone(MinimapKeyHint original, string name, string key)
  {
    // clone original MinimapKeyHint and set clone as its first sibling
    var clone = Object.Instantiate(original._gameObject, original._gameObject.transform.parent);
    clone.name = name;
    clone.transform.SetAsFirstSibling();

    // remove mouse image key hint
    Object.Destroy(clone.transform.Find("keyboard_hint").gameObject);

    // add key string key hint
    var keyBkg = KeyHints.m_instance.m_buildHints.transform.Find("Keyboard/AltPlace/key_bkg").gameObject;
    var keyBkgClone = Object.Instantiate(keyBkg, clone.transform);
    keyBkgClone.name = "key_bkg";
    keyBkgClone.SetText(key);

    // hide clone by default
    clone.SetActive(false);
    return new(clone);
  }

  public void Show(string text = null)
  {
    this._gameObject.SetActive(true);
    if (!string.IsNullOrEmpty(text)) this.Text = text;
  }
  public void Hide() => this._gameObject.SetActive(false);
}
