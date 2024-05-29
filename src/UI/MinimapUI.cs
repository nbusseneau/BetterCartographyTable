using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.Managers;
using UnityEngine.PlayerLoop;
using static Minimap;

namespace BetterCartographyTable.UI;

public static class MinimapUI
{
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

  public static void PrepareTogglesAndKeyHints()
  {
    // replace Cartography Table toggle label with our own, since it's only a shared exploration toggle now
    instance.m_sharedMapHint.SetText("$PinsToggle_SharedExploration");

    // create additional toggles for public and guild pins
    MinimapPinsToggle.Prepare();
    Toggles.PublicPins = new("PublicPins", "$PinsToggle_PublicPins");
    Toggles.GuildPins = new("GuildPins", "$PinsToggle_GuildPins");

    // set up key hints
    var keyHints = instance.m_hints[0].transform;
    KeyHints.DoubleClick = new(keyHints.Find("keyboard_hints/AddPin").gameObject);
    KeyHints.LeftClick = new(keyHints.Find("keyboard_hints/CrossOffPin").gameObject);
    KeyHints.RightClick = new(keyHints.Find("keyboard_hints/RemovePin").gameObject);
    KeyHints.ModifierKey = MinimapKeyHint.Clone(KeyHints.DoubleClick, "ModifierKey", Plugin.ModifierKey);
  }

  public static void ShowTableUI()
  {
    KeyHints.ModifierKey.Show();
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
  }

  public static void HideTableUI()
  {
    KeyHints.ModifierKey.Hide();
    if (!MinimapManager.PublicPins.Any()) Toggles.PublicPins.Hide();
    if (!MinimapManager.GuildPins.Any()) Toggles.GuildPins.Hide();
  }

  public static void UpdateKeyHints()
  {
    MinimapPinsToggle.UpdatePositions();
    if (!MapTableManager.IsTableInUse)
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
