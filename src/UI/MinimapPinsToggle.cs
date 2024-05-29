using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BetterCartographyTable.UI;

public class MinimapPinsToggle
{
  private static readonly List<GameObject> s_gameObjects = [];
  private static int s_lastCheckCount = -1;
  private static GameObject s_sharedExplorationToggle;
  private static float s_togglesLineSpacing;

  public static void Prepare()
  {
    s_gameObjects.Add(Minimap.instance.m_publicPosition.gameObject);
    s_gameObjects.Add(Minimap.instance.m_sharedMapHint);
    s_sharedExplorationToggle = Minimap.instance.m_sharedMapHint;
    s_togglesLineSpacing = Minimap.instance.m_sharedMapHint.transform.position.y - Minimap.instance.m_publicPosition.transform.position.y;
  }

  public static void UpdatePositions()
  {
    var activeToggles = s_gameObjects.Where(gameObject => gameObject.activeInHierarchy).ToList();
    var noChangeSinceLastCheck = s_lastCheckCount == activeToggles.Count;
    if (!activeToggles.Any() || noChangeSinceLastCheck) return;
    s_lastCheckCount = activeToggles.Count;

    var firstPosition = activeToggles.First().transform.position;
    var counter = 1;
    foreach (var gameObject in activeToggles.Skip(1))
    {
      var position = gameObject.transform.position;
      gameObject.transform.position = new Vector3(position.x, firstPosition.y + s_togglesLineSpacing * counter, position.z);
      counter++;
    }
  }

  private readonly Toggle _toggle;
  private readonly GameObject _gameObject;
  public bool IsOn { get; private set; } = true; // show by default on game start

  public MinimapPinsToggle(string namePrefix, string labelText)
  {
    // clone original GameObject from shared exploration toggle, and adjust name / label
    _gameObject = Object.Instantiate(s_sharedExplorationToggle, s_sharedExplorationToggle.transform.parent);
    _gameObject.name = namePrefix + "Panel";
    var clonePosition = _gameObject.transform.Find("PublicPosition");
    clonePosition.name = namePrefix + "Position";
    _gameObject.SetText(labelText);

    // disable default Toggle listener and hook up our own
    _toggle = _gameObject.GetComponentInChildren<Toggle>();
    _toggle.onValueChanged.SetPersistentListenerState(0, UnityEventCallState.Off);
    _toggle.onValueChanged.AddListener(this.OnToggle);

    // enable by default
    this.ForceToggleOn();
    this.Show();

    // register
    s_gameObjects.Add(_gameObject);
  }

  private void OnToggle(bool isOn)
  {
    this.IsOn = isOn;
    Minimap.instance.m_pinUpdateRequired = true;
  }

  public void ForceToggleOn()
  {
    _toggle.isOn = true;
    this.OnToggle(_toggle.isOn);
  }

  public void Show() => _gameObject.SetActive(true);
  public void Hide() => _gameObject.SetActive(false);
}
