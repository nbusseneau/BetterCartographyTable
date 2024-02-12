using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Managers;
using BetterCartographyTable.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Minimap;

namespace BetterCartographyTable.UI;

public static class MinimapManagerUI
{
  private static float yOffset;
  private static GameObject AddPinKeyHint { get; set; }
  private static GameObject CrossOffPinKeyHint { get; set; }
  private static GameObject RemovePinKeyHint { get; set; }
  private static GameObject ModifierKeyHint { get; set; }
  private static Toggle PublicPinsToggle { get; set; }
  private static Toggle GuildPinsToggle { get; set; }
  private static bool ArePublicPinsDisplayed { get; set; }
  private static bool AreGuildPinsDisplayed { get; set; }

  public static void OnAwake()
  {
    instance.m_sharedMapHint.SetText("$PinsToggle_SharedExploration");
    yOffset = instance.m_sharedMapHint.transform.position.y - instance.m_publicPosition.transform.position.y;
    PublicPinsToggle = CreateNewPinToggle(instance.m_sharedMapHint, "PublicPins", "$PinsToggle_PublicPins", OnPublicPinsToggle, yOffset);
    GuildPinsToggle = CreateNewPinToggle(instance.m_sharedMapHint, "GuildPins", "$PinsToggle_GuildPins", OnGuildPinsToggle, 2 * yOffset);

    var keyHints = instance.m_hints[0].transform;
    AddPinKeyHint = keyHints.Find("keyboard_hints/AddPin").gameObject;
    CrossOffPinKeyHint = keyHints.Find("keyboard_hints/CrossOffPin").gameObject;
    RemovePinKeyHint = keyHints.Find("keyboard_hints/RemovePin").gameObject;
    ModifierKeyHint = CreateModifierKeyHint(AddPinKeyHint);
  }

  public static void OnStart()
  {
    ArePublicPinsDisplayed = true;
    AreGuildPinsDisplayed = true;
  }

  public static void ShowModifierKeyHintAndPinToggle(SharingMode mode)
  {
    ModifierKeyHint.SetActive(true);
    Toggle toggle = null;
    if (mode == SharingMode.Public) toggle = PublicPinsToggle;
    else if (mode == SharingMode.Guild) toggle = GuildPinsToggle;
    toggle.isOn = true;
    toggle.transform.parent.gameObject.SetActive(true);
  }

  public static void HideModifierKeyHintAndPinToggles()
  {
    ModifierKeyHint.SetActive(false);
    if (!MinimapManager.PublicPins.Any()) PublicPinsToggle.transform.parent.gameObject.SetActive(false);
    if (!MinimapManager.GuildPins.Any()) GuildPinsToggle.transform.parent.gameObject.SetActive(false);
  }

  public static void OnUpdatePins()
  {
    UpdateKeyHints();
    UpdatePins();
  }

  private static void UpdateKeyHints()
  {
    if (ModifierKeyHint.activeInHierarchy)
    {
      if (InteractionManager.CurrentMapTableMode == SharingMode.Public) ModifierKeyHint.SetText("$KeyHint_PublicModifierKey");
      else if (InteractionManager.CurrentMapTableMode == SharingMode.Guild) ModifierKeyHint.SetText("$KeyHint_GuildModifierKey");
    }

    if (!Plugin.IsModifierKeyPushed)
    {
      AddPinKeyHint.SetActive(true);
      CrossOffPinKeyHint.SetText("$hud_crossoffpin");
      if (InteractionManager.IsInteracting) RemovePinKeyHint.SetText("$KeyHint_RemovePrivatePin");
      else RemovePinKeyHint.SetText("$hud_removepin");
    }
    else if (Plugin.IsModifierKeyPushed && InteractionManager.CurrentMapTableMode == SharingMode.Public)
    {
      AddPinKeyHint.SetActive(false);
      CrossOffPinKeyHint.SetText("$KeyHint_TogglePublic");
      RemovePinKeyHint.SetText("$KeyHint_RemovePublicPin");
    }
    else if (Plugin.IsModifierKeyPushed && InteractionManager.CurrentMapTableMode == SharingMode.Guild)
    {
      AddPinKeyHint.SetActive(false);
      CrossOffPinKeyHint.SetText("$KeyHint_ToggleGuild");
      RemovePinKeyHint.SetText("$KeyHint_RemoveGuildPin");
    }
  }

  private static void UpdatePins()
  {
    Plugin.Logger.LogDebug($"ArePublicPinsDisplayed: {ArePublicPinsDisplayed} | AreGuildPinsDisplayed: {AreGuildPinsDisplayed}");
    foreach (var pin in MinimapManager.PublicPins)
    {
      pin.SetVisibility(ArePublicPinsDisplayed);
      if (ArePublicPinsDisplayed) pin.SetColor(Plugin.PublicPinsColor);
    }

    foreach (var pin in MinimapManager.GuildPins)
    {
      pin.SetVisibility(AreGuildPinsDisplayed);
      if (AreGuildPinsDisplayed) pin.SetColor(GuildsManager.CurrentGuildColor);
    }
  }

  private static void OnPublicPinsToggle(bool isOn) => ArePublicPinsDisplayed = isOn;
  private static void OnGuildPinsToggle(bool isOn) => AreGuildPinsDisplayed = isOn;

  private static Toggle CreateNewPinToggle(GameObject original, string namePrefix, string labelText, UnityAction<bool> callback, float YOffset = 0)
  {
    // clone original GameObject and offset its position
    var clone = Object.Instantiate(original, original.transform.parent);
    clone.transform.position = new Vector3(clone.transform.position.x, clone.transform.position.y + YOffset, clone.transform.position.z);
    clone.name = namePrefix + "Panel";
    var clonePosition = clone.transform.Find("PublicPosition");
    clonePosition.name = namePrefix + "Position";

    // edit label text
    clone.SetText(labelText);

    // disable default listener and hook up our own
    var toggle = clone.GetComponentInChildren<Toggle>();
    toggle.onValueChanged.SetPersistentListenerState(0, UnityEventCallState.Off);
    toggle.onValueChanged.AddListener((value) => callback(value));

    clone.SetActive(true);
    return toggle;
  }

  private static GameObject CreateModifierKeyHint(GameObject original)
  {
    // clone original GameObject and set as first siblig
    var clone = Object.Instantiate(original, original.transform.parent);
    clone.name = "ModifierKey";
    clone.transform.SetAsFirstSibling();

    // remove mouse image key hint
    Object.Destroy(clone.transform.Find("keyboard_hint").gameObject);

    // add key string key hint
    var keyBkg = KeyHints.m_instance.m_buildHints.transform.Find("Keyboard/AltPlace/key_bkg").gameObject;
    var keyBkgClone = Object.Instantiate(keyBkg, clone.transform);
    keyBkgClone.name = "key_bkg";
    var keyString = Plugin.ModifierKey.ToString();
    keyBkgClone.SetText(keyString);

    clone.SetActive(false);
    return clone;
  }
}
