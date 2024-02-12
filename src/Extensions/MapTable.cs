using System.Collections.Generic;
using BetterCartographyTable.Managers;
using BetterCartographyTable.Model;

namespace BetterCartographyTable.Extensions;

public static class MapTableExtensions
{
  public static void OnStart(this MapTable mapTable)
  {
    mapTable.m_nview.Register<ZPackage>(storePinsInZDORPCName, (_, zPackage) => mapTable.RPC_StorePinsInZDO(zPackage));
    mapTable.m_nview.Register<int>(storeModeInZDORPCName, (_, mode) => mapTable.RPC_StoreModeInZDO(mode));
    mapTable.m_nview.Register<string>(storeOwnerInZDORPCName, (_, owner) => mapTable.RPC_StoreOwnerInZDO(owner));
  }

  public static void MakePublic(this MapTable mapTable)
  {
    MinimapManager.RemoveTablePins(mapTable);
    mapTable.StorePinsInZDO([]);
    mapTable.StoreModeInZDO(SharingMode.Public);
    mapTable.StoreOwnerInZDO(string.Empty);
  }

  public static void RestrictToGuild(this MapTable mapTable, string guildName)
  {
    MinimapManager.RemoveTablePins(mapTable);
    mapTable.StorePinsInZDO([]);
    mapTable.StoreModeInZDO(SharingMode.Guild);
    mapTable.StoreOwnerInZDO(guildName);
  }


  #region Pins (custom ZDO key)

  private const string pinsZDOKey = $"{Plugin.ModGUID}.MapTable.Pins";
  private const string storePinsInZDORPCName = $"{Plugin.ModGUID}.{nameof(RPC_StorePinsInZDO)}";

  public static void ReplaceTablePinsWithMinimapPins(this MapTable mapTable)
  {
    var mode = mapTable.RetrieveModeFromZDO();
    var minimapPins = MinimapManager.GetPinsForMode(mode);
    mapTable.StorePinsInZDO(minimapPins);
  }

  private static void StorePinsInZDO(this MapTable mapTable, IEnumerable<SharablePinData> pins)
  {
    var zPackage = pins.ToZPackage();
    if (mapTable.m_nview.IsOwner()) mapTable.m_nview.GetZDO().Set(pinsZDOKey, zPackage.GetArray());
    else mapTable.m_nview.InvokeRPC(storePinsInZDORPCName, zPackage);
  }

  private static void RPC_StorePinsInZDO(this MapTable mapTable, ZPackage zPackage)
  {
    if (!mapTable.m_nview.IsOwner()) return;
    mapTable.m_nview.GetZDO().Set(pinsZDOKey, zPackage.GetArray());
  }

  public static IEnumerable<SharablePinData> RetrievePinsFromZDO(this MapTable mapTable)
  {
    if (mapTable.m_nview.GetZDO().GetByteArray(pinsZDOKey) is { } array)
    {
      array = Utils.Decompress(array);
      ZPackage zPackage = new(array);
      return zPackage.ReadSharablePinList();
    }
    return [];
  }

  #endregion

  #region Mode (custom ZDO key)

  private const string modeZDOKey = $"{Plugin.ModGUID}.MapTable.Mode";
  private const string storeModeInZDORPCName = $"{Plugin.ModGUID}.{nameof(RPC_StoreModeInZDO)}";

  public static (bool isPublic, bool isGuild) GetModes(this MapTable mapTable)
  {
    var mode = mapTable.RetrieveModeFromZDO();
    var isPublic = mode == SharingMode.Public;
    var isGuild = mode == SharingMode.Guild;
    return (isPublic, isGuild);
  }

  private static void StoreModeInZDO(this MapTable mapTable, SharingMode mode)
  {
    if (mapTable.m_nview.IsOwner()) mapTable.m_nview.GetZDO().Set(modeZDOKey, (int)mode);
    else mapTable.m_nview.InvokeRPC(storeModeInZDORPCName, (int)mode);
  }

  private static void RPC_StoreModeInZDO(this MapTable mapTable, int mode)
  {
    if (!mapTable.m_nview.IsOwner()) return;
    mapTable.m_nview.GetZDO().Set(modeZDOKey, mode);
  }

  public static SharingMode RetrieveModeFromZDO(this MapTable mapTable) => (SharingMode)mapTable.m_nview.GetZDO().GetInt(modeZDOKey, (int)SharingMode.Public);

  #endregion

  #region Owner (custom ZDO key)

  private const string ownerZDOKey = $"{Plugin.ModGUID}.MapTable.Owner";
  private const string storeOwnerInZDORPCName = $"{Plugin.ModGUID}.{nameof(RPC_StoreOwnerInZDO)}";

  private static void StoreOwnerInZDO(this MapTable mapTable, string owner)
  {
    if (mapTable.m_nview.IsOwner()) mapTable.m_nview.GetZDO().Set(ownerZDOKey, owner);
    else mapTable.m_nview.InvokeRPC(storeOwnerInZDORPCName, owner);
  }

  private static void RPC_StoreOwnerInZDO(this MapTable mapTable, string owner)
  {
    if (!mapTable.m_nview.IsOwner()) return;
    mapTable.m_nview.GetZDO().Set(ownerZDOKey, owner);
  }

  public static string RetrieveOwnerFromZDO(this MapTable mapTable) => mapTable.m_nview.GetZDO().GetString(ownerZDOKey, null);

  #endregion

  #region Explored map storage (vanilla ZDO key)

  public static void SyncExploredMap(this MapTable mapTable)
  {
    mapTable.RetrieveExploredMapFromZDO();
    mapTable.StoreExploredMapInZDO();
  }

  private static void RetrieveExploredMapFromZDO(this MapTable mapTable)
  {
    if (mapTable.m_nview.GetZDO().GetByteArray(ZDOVars.s_data) is { } array)
    {
      array = Utils.Decompress(array);
      // AddSharedMapData(...) has been truncated via a Harmony patch so that it only retrieves
      // explored map data from vanilla shared data, and not pins.
      Minimap.instance.AddSharedMapData(array);
    }
  }

  private static void StoreExploredMapInZDO(this MapTable mapTable)
  {
    var array = mapTable.m_nview.GetZDO().GetByteArray(ZDOVars.s_data);
    if (array is not null) array = Utils.Decompress(array);
    // Underlying call to GetSharedMapData(...) has been truncated via a Harmony patch so that it
    // only stores explored map data in vanilla shared data, and not pins.
    var zPackage = mapTable.GetMapData(array);
    mapTable.m_nview.InvokeRPC("MapData", zPackage);
  }

  #endregion

  #region Pin events

  private const string onPinEventRPCName = $"{Plugin.ModGUID}.{nameof(RPC_OnPinEvent)}";
  public static void StartListeningForPinEvents(this MapTable mapTable) => mapTable.m_nview.Register<ZPackage, int>(onPinEventRPCName, RPC_OnPinEvent);
  public static void StopListeningForPinEvents(this MapTable mapTable) => mapTable.m_nview.Unregister(onPinEventRPCName);
  public static void SendPinEvent(this MapTable mapTable, SharablePinData pin, PinEvent pinEvent)
  {
    mapTable.m_nview.InvokeRPC(ZNetView.Everybody, onPinEventRPCName, pin.ToZPackage(), (int)pinEvent);
    mapTable.ReplaceTablePinsWithMinimapPins();
  }
  private static void RPC_OnPinEvent(long senderId, ZPackage zPackage, int pinEventInt)
  {
    var pinEvent = (PinEvent)pinEventInt;
    var currentPlayerPeerID = Player.m_localPlayer.GetZDOID().UserID;
    if (senderId == currentPlayerPeerID) return;
    MinimapManager.HandlePinEvent(zPackage, pinEvent);
  }

  #endregion
}
