namespace BetterCartographyTable.Model.MapTableZDOValues;

/// <summary>
/// Base helper class for getting / setting values stored in a MapTable's ZDO.
/// </summary>
public abstract class MapTableZDOValue<T>
{
  protected readonly MapTable _mapTable;
  protected ZNetView NView => this._mapTable.m_nview;
  protected readonly string _key;
  protected readonly string _storeRPC;
  public abstract T Value { get; set; }

  public MapTableZDOValue(MapTable mapTable, string key)
  {
    this._mapTable = mapTable;
    this._key = $"{Plugin.ModGUID}.MapTable.{key}";
    this._storeRPC = $"{Plugin.ModGUID}.RPC_Store.MapTable.{key}";
    this.RegisterStoreRPC();
  }

  protected abstract void RegisterStoreRPC();
}
