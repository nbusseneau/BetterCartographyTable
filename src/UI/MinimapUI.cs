using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.Managers;
using UnityEngine;
using static Minimap;

namespace BetterCartographyTable.UI;

public static class MinimapUI
{
  private static List<GameObject> s_toggles;
  private static class Toggles
  {
    public static MinimapPinsToggle PublicPins { get; set; }
    public static MinimapPinsToggle GuildPins { get; set; }
  }

  private static class KeyHints
  {
    public static MinimapKeyHint DoubleClick { get; set; }
    public static MinimapKeyHint LeftClick { get; set; }
    public static MinimapKeyHint RightClick { get; set; }
    public static MinimapKeyHint ModifierKey { get; set; }
  }

  public static void PrepareToggles()
  {
    // create additional toggles for public and guild pins
    Toggles.PublicPins = new("PublicPins", "$PinsToggle_PublicPins");
    Toggles.GuildPins = new("GuildPins", "$PinsToggle_GuildPins");

    // register all toggles
    s_toggles = [
      instance.m_publicPosition.gameObject,
      instance.m_sharedMapHint,
      Toggles.PublicPins.GameObject,
      Toggles.GuildPins.GameObject,
    ];
  }

  public static void PrepareKeyHints()
  {
    // set up key hints
    var keyHints = instance.m_hints[0].transform;
    KeyHints.DoubleClick = new(keyHints.Find("keyboard_hints/AddPin").gameObject);
    KeyHints.LeftClick = new(keyHints.Find("keyboard_hints/CrossOffPin").gameObject);
    KeyHints.RightClick = new(keyHints.Find("keyboard_hints/RemovePin").gameObject);
    KeyHints.ModifierKey = MinimapKeyHint.Clone(KeyHints.DoubleClick, "ModifierKey", Plugin.ModifierKey.ToKeyHintString());
  }

  public static void ShowTableUI()
  {
    if (MapTableManager.CurrentTable.IsPublic)
    {
      Toggles.PublicPins.ForceToggleOn();
      Toggles.PublicPins.Show();
    }
    else if (MapTableManager.CurrentTable.IsGuild)
    {
      Toggles.GuildPins.ForceToggleOn();
      Toggles.GuildPins.Show();
    }
    KeyHints.ModifierKey.Show();
  }

  public static void HideTableUI()
  {
    if (!MinimapManager.PublicPins.Any()) Toggles.PublicPins.Hide();
    if (!MinimapManager.GuildPins.Any()) Toggles.GuildPins.Hide();
    KeyHints.ModifierKey.Hide();
  }

  private static int s_togglesLastCheckActiveCount = -1;
  public static void UpdateToggles()
  {
    // update toggles positions based on active status
    var activeToggles = s_toggles.Where(gameObject => gameObject.activeInHierarchy);
    var activeCount = activeToggles.Count();
    if (!activeToggles.Any() || s_togglesLastCheckActiveCount == activeCount) return;
    s_togglesLastCheckActiveCount = activeCount;

    var firstPosition = activeToggles.First().transform.position;
    var counter = 1;
    var lineSpacing = instance.m_sharedMapHint.transform.position.y - instance.m_publicPosition.transform.position.y;
    foreach (var gameObject in activeToggles.Skip(1))
    {
      var position = gameObject.transform.position;
      gameObject.transform.position = new Vector3(position.x, firstPosition.y + lineSpacing * counter, position.z);
      counter++;
    }
  }

  public static void UpdateKeyHints()
  {
    if (!MapTableManager.IsTableValid)
    {
      KeyHints.DoubleClick.Show("$hud_addpin");
      KeyHints.LeftClick.Text = "$hud_crossoffpin";
      KeyHints.RightClick.Text = "$hud_removepin";
    }
    else
    {
      if (!Plugin.IsModifierKeyPressed)
      {
        KeyHints.DoubleClick.Show("$KeyHint_AddPrivatePin");
        KeyHints.LeftClick.Text = "$hud_crossoffpin";
        KeyHints.RightClick.Text = "$KeyHint_RemovePrivatePin";
      }
      else
      {
        KeyHints.DoubleClick.Hide();
        KeyHints.LeftClick.Text = "$KeyHint_ToggleSharing";
        KeyHints.RightClick.Text = MapTableManager.CurrentTable.IsGuild ? "$KeyHint_RemoveGuildPin" : "$KeyHint_RemovePublicPin";
      }
      KeyHints.ModifierKey.Text = MapTableManager.CurrentTable.IsGuild ? "$KeyHint_GuildModifierKey" : "$KeyHint_PublicModifierKey";
    }
  }

  public static void UpdateSharedPins()
  {
    foreach (var pin in MinimapManager.PublicPins)
    {
      pin.SetVisibility(Toggles.PublicPins.IsOn);
      if (Toggles.PublicPins.IsOn) pin.SetColor(Plugin.PublicPinsColor);
    }

    foreach (var pin in MinimapManager.GuildPins)
    {
      pin.SetVisibility(Toggles.GuildPins.IsOn);
      if (Toggles.GuildPins.IsOn) pin.SetColor(GuildsManager.CurrentGuildColor);
    }
  }
}
