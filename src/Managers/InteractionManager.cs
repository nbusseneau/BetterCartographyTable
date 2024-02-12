using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model;
using BetterCartographyTable.UI;

namespace BetterCartographyTable.Managers;

public static class InteractionManager

{
  private static MapTable currentMapTable;
  private static MapTable CurrentMapTable
  {
    get => currentMapTable;
    set
    {
      currentMapTable = value;
      CurrentMapTableMode = value?.RetrieveModeFromZDO();
    }
  }
  public static bool IsInteracting => currentMapTable is not null;
  public static SharingMode? CurrentMapTableMode { get; private set; }

  #region MapTable interactions

  public static void OnMapTableUse(MapTable mapTable, Humanoid user)
  {
    if (IsInteracting) return;

    var hasAccess = HasAccess(mapTable);
    if (!hasAccess) user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_noaccess"), 0, null);
    else if (Plugin.IsModifierKeyPushed && GuildsManager.IsEnabled) TryToggleMode(mapTable, user);
    else
    {
      CurrentMapTable = mapTable;
      CurrentMapTable.SyncExploredMap();
      MinimapManager.ReplaceMinimapPinsWithTablePins(CurrentMapTable);
      CurrentMapTable.StartListeningForPinEvents();
      MinimapManager.OpenMap(mapTable);
    }
  }

  private static bool HasAccess(MapTable mapTable)
  {
    var hasAccess = false;
    var (isPublic, isGuild) = mapTable.GetModes();
    var isPlayerMemberOfGuild = mapTable.RetrieveOwnerFromZDO() is { } guildName && GuildsManager.CurrentGuildName == guildName;

    if (isPublic || isGuild && isPlayerMemberOfGuild) hasAccess = true;
    return hasAccess;
  }

  private static void TryToggleMode(MapTable mapTable, Humanoid user)
  {
    var pins = mapTable.RetrievePinsFromZDO();
    if (pins.Any()) InteractionManagerUI.ShowToggleModePopup(() => DoToggleMode(mapTable, user));
    else DoToggleMode(mapTable, user);
  }

  private static void DoToggleMode(MapTable mapTable, Humanoid user)
  {
    var (isPublic, isGuild) = mapTable.GetModes();
    if (isPublic)
    {
      if (!string.IsNullOrEmpty(GuildsManager.CurrentGuildName)) mapTable.RestrictToGuild(GuildsManager.CurrentGuildName);
      else user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$MapTableToggleMode_GuildRequired"), 0, null);
    }
    else if (isGuild) mapTable.MakePublic();
  }

  public static string GetHoverText(MapTable mapTable)
  {
    var mode = mapTable.RetrieveModeFromZDO();
    var isGuild = mode == SharingMode.Guild;
    var owner = mapTable.RetrieveOwnerFromZDO();

    var hoverText = mapTable.m_name + "\n";
    hoverText += isGuild ? $"$MapTableHoverText_RestrictedToGuild: {owner}" : $"$MapTableHoverText_Public";
    var hasAccess = HasAccess(mapTable);
    if (!hasAccess) return hoverText;

    hoverText += "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
    if (!GuildsManager.IsEnabled) return hoverText;

    var toggleTo = isGuild ? $"$MapTableHoverText_MakePublic" : $"$MapTableHoverText_RestrictToGuild";
    return hoverText + $"\n[<b><color=yellow>{Plugin.ModifierKey}</color> + <color=yellow>$KEY_Use</color></b>] {toggleTo}";
  }

  #endregion

  #region Minimap interactions

  public static void OnMapClose()
  {
    if (!IsInteracting) return;
    CurrentMapTable.SyncExploredMap();
    CurrentMapTable.ReplaceTablePinsWithMinimapPins();
    CurrentMapTable.StopListeningForPinEvents();
    MinimapManager.OnMapClose();
    CurrentMapTable = null;
  }

  public static bool OnMapDblClick()
  {
    var shouldRunOriginalMethod = true;
    // to prevent accidental pin addition while cycling pins with modifier key + left click,
    // do nothing if using modifier key + double-clicking
    if (Plugin.IsModifierKeyPushed) shouldRunOriginalMethod = false;
    return shouldRunOriginalMethod;
  }

  public static void OnMapLeftClick()
  {
    MinimapManager.HidePinTextInput();
    var closestPin = MinimapManager.GetClosestPinToMouse();
    if (closestPin is null) return;

    // left-clicking a private pin without using the modifier key toggles the pin's checked status
    var leftClickingPrivatePin = !Plugin.IsModifierKeyPushed && closestPin.IsPrivate;
    if (leftClickingPrivatePin)
    {
      closestPin.ToggleChecked();
      return;
    }

    // all other actions require interacting with a map table
    if (!IsInteracting) return;
    var isTablePin = closestPin.IsMapTablePin(CurrentMapTable);

    var modifierPlusLeftClickingPrivatePin = Plugin.IsModifierKeyPushed && closestPin.IsPrivate;
    var leftClickingTablePin = !Plugin.IsModifierKeyPushed && isTablePin;
    var modifierPlusLeftClickingTablePin = Plugin.IsModifierKeyPushed && isTablePin;

    // using modifier key + left-clicking a private pin adds it to the table, becoming a table pin
    if (modifierPlusLeftClickingPrivatePin)
    {
      closestPin.SharingMode = CurrentMapTableMode.Value;
      CurrentMapTable.SendPinEvent(closestPin, PinEvent.Add);
    }

    // left-clicking a table pin without using the modifier key toggles the pin's checked status
    else if (leftClickingTablePin)
    {
      closestPin.ToggleChecked();
      CurrentMapTable.SendPinEvent(closestPin, PinEvent.ToggleChecked);
    }

    // using modifier key + left-clicking a table pin removes it from the table, becoming private
    else if (modifierPlusLeftClickingTablePin)
    {
      closestPin.SharingMode = SharingMode.Private;
      CurrentMapTable.SendPinEvent(closestPin, PinEvent.Remove);
    }
  }

  public static void OnMapRightClick()
  {
    MinimapManager.HidePinTextInput();
    var closestPin = MinimapManager.GetClosestPinToMouse();
    if (closestPin is null) return;

    // right-clicking a private pin without using the modifier key removes it from the map
    var rightClickingPrivatePin = !Plugin.IsModifierKeyPushed && closestPin.IsPrivate;
    if (rightClickingPrivatePin)
    {
      MinimapManager.RemovePin(closestPin);
      return;
    }

    // all other actions require interacting with a map table
    if (!IsInteracting) return;
    var isTablePin = closestPin.IsMapTablePin(CurrentMapTable);

    var modifierPlusRightClickingPrivatePin = Plugin.IsModifierKeyPushed && closestPin.IsPrivate;
    var rightClickingTablePin = !Plugin.IsModifierKeyPushed && isTablePin;
    var modifierPlusRightClickingTablePin = Plugin.IsModifierKeyPushed && isTablePin;

    // to prevent accidental deletions, do nothing when:
    // - using modifier key + right-clicking a private pin
    // - right-clicking a table pin without using the modifier key
    if (modifierPlusRightClickingPrivatePin || rightClickingTablePin) return;

    // using modifier key + right-clicking a table pin removes it from the map
    else if (modifierPlusRightClickingTablePin)
    {
      MinimapManager.RemovePin(closestPin);
      CurrentMapTable.SendPinEvent(closestPin, PinEvent.Remove);
    }
  }

  #endregion
}
