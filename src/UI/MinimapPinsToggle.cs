using BetterCartographyTable.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BetterCartographyTable.UI;

public class MinimapPinsToggle
{
  private readonly Toggle _toggle;
  public GameObject GameObject { get; private set; }
  public bool IsOn { get; private set; } = true; // show by default on game start

  public MinimapPinsToggle(string namePrefix, string labelText)
  {
    // clone original GameObject from shared exploration toggle, and adjust name / label
    this.GameObject = Object.Instantiate(Minimap.instance.m_sharedMapHint, Minimap.instance.m_sharedMapHint.transform.parent);
    this.GameObject.name = namePrefix + "Panel";
    var clonePosition = GameObject.transform.Find("PublicPosition");
    clonePosition.name = namePrefix + "Position";
    this.GameObject.SetText(labelText);

    // disable default Toggle listener and hook up our own
    this._toggle = this.GameObject.GetComponentInChildren<Toggle>();
    this._toggle.onValueChanged.SetPersistentListenerState(0, UnityEventCallState.Off);
    this._toggle.onValueChanged.AddListener(this.OnToggle);

    // enable by default
    this.ForceToggleOn();
    this.Show();
  }

  private void OnToggle(bool isOn)
  {
    this.IsOn = isOn;
    Minimap.instance.m_pinUpdateRequired = true;
  }

  public void ForceToggleOn()
  {
    this._toggle.isOn = true;
    this.OnToggle(this._toggle.isOn);
  }

  public void Show() => this.GameObject.SetActive(true);
  public void Hide() => this.GameObject.SetActive(false);
}
