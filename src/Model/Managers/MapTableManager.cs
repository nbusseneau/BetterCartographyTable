using System;
using System.Collections.Generic;
using System.Linq;
using BetterCartographyTable.Extensions;
using BetterCartographyTable.Model.MapTableZDOValues;
using BetterCartographyTable.UI;
using UnityEngine;

namespace BetterCartographyTable.Model.Managers;

/// <summary>
/// Wrapper-manager class enhancing MapTable objects with our own behaviour.
/// </summary>
public class MapTableManager : IEquatable<MapTable>
{
  public static MapTableManager CurrentTable { get; private set; }
  public static bool IsTableInUse => CurrentTable is not null;

  public static void TryOpenCurrentTable(MapTableManager table, Humanoid user)
  {
    if (IsTableInUse) return;
    if (!table.CheckAccess()) user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$piece_noaccess"), 0, null);
    else if (Plugin.IsModifierKeyPressed && GuildsManager.IsEnabled) table.TryToggleMode(user);
    else
    {
      CurrentTable = table;
      CurrentTable.Open();
    }
  }

  public static void TryCloseCurrentTable()
  {
    if (!IsTableInUse) return;
    CurrentTable.Close();
    CurrentTable = null;
  }

  private readonly MapTable _mapTable;
  private ZNetView NView => this._mapTable.m_nview;
  private Vector3 Position => this._mapTable.transform.position;

  private readonly MapTableZDOPins _pins;
  public IEnumerable<SharablePinData> Pins { get => this._pins.Value; private set => this._pins.Value = value; }

  private readonly MapTableZDOSharingMode _sharingMode;
  public SharingMode SharingMode { get => this._sharingMode.Value; private set => this._sharingMode.Value = value; }
  public bool IsPublic => this.SharingMode == SharingMode.Public;
  public bool IsGuild => this.SharingMode == SharingMode.Guild;

  private readonly MapTableZDOOwner _owner;
  public string Owner { get => this._owner.Value; private set => this._owner.Value = value; }

  public MapTableManager(MapTable mapTable)
  {
    this._mapTable = mapTable;
    this._pins = new(this._mapTable);
    this._sharingMode = new(this._mapTable);
    this._owner = new(this._mapTable);
    this.NView.Register<ZPackage, int>(OnPinEventRPC, this.RPC_OnPinEvent);
  }

  public bool IsInUseDistance(Player player) => Vector3.Distance(player.transform.position, this.Position) <= player.m_maxInteractDistance + 1.75f;

  private bool CheckAccess()
  {
    var guildName = this.Owner;
    var isPlayerMemberOfGuild = !string.IsNullOrEmpty(guildName) && GuildsManager.CurrentGuildName == guildName;
    return this.IsPublic || this.IsGuild && isPlayerMemberOfGuild;
  }

  public string GetHoverText()
  {
    var hoverText = this._mapTable.m_name + "\n";
    hoverText += this.IsGuild ? $"$MapTable_HoverText_RestrictedToGuild: {this.Owner}" : $"$MapTable_HoverText_Public";
    if (!this.CheckAccess()) return hoverText;

    hoverText += "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
    if (!GuildsManager.IsEnabled) return hoverText;

    var toggleTo = this.IsGuild ? $"$MapTable_HoverText_MakePublic" : $"$MapTable_HoverText_RestrictToGuild";
    return hoverText + $"\n[<b><color=yellow>{Plugin.ModifierKey}</color> + <color=yellow>$KEY_Use</color></b>] {toggleTo}";
  }

  private void TryToggleMode(Humanoid user)
  {
    if (this.IsPublic && string.IsNullOrEmpty(GuildsManager.CurrentGuildName))
    {
      user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$MapTable_ToggleMode_GuildRequired"), 0, null);
      return;
    }
    if (this.Pins.Any()) MapTableToggleModeWarningPopup.Show();
    else this.ToggleMode();
  }

  private void ToggleMode()
  {
    if (this.IsPublic) this.RestrictToGuild(GuildsManager.CurrentGuildName);
    else if (this.IsGuild) this.MakePublic();
  }

  private void MakePublic()
  {
    MinimapManager.RemovePins(this.Pins);
    this.Pins = [];
    this.SharingMode = SharingMode.Public;
    this.Owner = string.Empty;
  }

  private void RestrictToGuild(string guildName)
  {
    MinimapManager.RemovePins(this.Pins);
    this.Pins = [];
    this.SharingMode = SharingMode.Guild;
    this.Owner = guildName;
  }

  private void Open()
  {
    this.UpdateMinimapPins();
    this.SyncVanillaSharedMapData();
    MinimapUI.ShowTableUI();
    Minimap.instance.ShowPointOnMap(this.Position);
  }

  private void Close()
  {
    this.SaveTablePins();
    this.SyncVanillaSharedMapData();
    MinimapUI.HideTableUI();
  }

  private void UpdateMinimapPins()
  {
    if (this.IsPublic) MinimapManager.PublicPins = this.Pins;
    else if (this.IsGuild) MinimapManager.GuildPins = this.Pins;
  }

  private void SaveTablePins()
  {
    if (this.IsPublic) this.Pins = MinimapManager.PublicPins;
    else if (this.IsGuild) this.Pins = MinimapManager.GuildPins;
  }

  private const string OnPinEventRPC = $"{Plugin.ModGUID}.{nameof(RPC_OnPinEvent)}";

  public void SendPinEvent(SharablePinData pin, PinEvent pinEvent)
  {
    this.NView.InvokeRPC(ZNetView.Everybody, OnPinEventRPC, pin.ToCompressedZPackage(), (int)pinEvent);
    this.SaveTablePins(); // annoying, but required in case someone else opens the table before we close it
  }

  private void RPC_OnPinEvent(long senderId, ZPackage compressedZPackage, int pinEventInt)
  {
    // ignore events if the table is not open
    if (this != CurrentTable) return;

    // ignore events coming from ourselves, as they're already handled locally
    var currentPlayerPeerID = Player.m_localPlayer.GetZDOID().UserID;
    if (senderId == currentPlayerPeerID) return;

    var pin = compressedZPackage.Decompress().ReadSharablePinData();
    var pinEvent = (PinEvent)pinEventInt;
    var existingPin = MinimapManager.SharablePins.SingleOrDefault(p => pin.Equals(p));
    if (existingPin is null && pinEvent == PinEvent.Add) Minimap.instance.AddPin(pin);
    else if (existingPin is not null)
    {
      if (pinEvent == PinEvent.ToggleChecked) existingPin.ToggleChecked();
      else if (pinEvent == PinEvent.Remove) Minimap.instance.RemovePin(existingPin);
    }
    else
    {
      var error = @$"Invalid minimap state: cannot handle the received PinEvent given the current minimap data.
pin: {pin} | pinEvent: {pinEvent} | isExistingPin: {existingPin}";
      Plugin.Logger.LogError(error);
    }
  }

  private void SyncVanillaSharedMapData()
  {
    this.RetrieveExploredMap();
    this.StoreExploredMapAndPublicPins();
  }

  private void RetrieveExploredMap()
  {
    if (this.NView.GetZDO().GetByteArray(ZDOVars.s_data) is { } array)
    {
      array = Utils.Decompress(array);
      // AddSharedMapData(...) is truncated via a Harmony patch so that it only retrieves explored
      // map data from inbound vanilla shared data, ignoring pins coming from non-modded clients.
      Minimap.instance.AddSharedMapData(array);
    }
  }

  private void StoreExploredMapAndPublicPins()
  {
    var array = this.NView.GetZDO().GetByteArray(ZDOVars.s_data, null);
    if (array is not null) array = Utils.Decompress(array);
    // Underlying call to GetSharedMapData(...) is modified via a Harmony patch so that it stores
    // public pins in outbound vanilla shared data, for limited compatibility with non-modded
    // clients.
    var zPackage = this._mapTable.GetMapData(array);
    this.NView.InvokeRPC("MapData", zPackage);
  }

  public bool Equals(MapTable other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this._mapTable, other)) return true;
    return this.Position.Equals(other.transform.position);
  }
  public override bool Equals(object obj) => this.Equals(obj as MapTable);
  public static bool operator ==(MapTableManager a, MapTable b) => a is not null && (ReferenceEquals(a._mapTable, b) || a.Equals(b));
  public static bool operator !=(MapTableManager a, MapTable b) => !(a == b);
  public override int GetHashCode() => this.Position.GetHashCode();
}
