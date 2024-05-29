namespace BetterCartographyTable.Model.MapTableZDOValues;

/// <summary>
/// Base helper class for getting / setting values stored in a MapTable's ZDO.
/// </summary>
public abstract class MapTableZDOValue<T>
{
  protected readonly MapTable MapTable;
  protected ZNetView NView => this.MapTable.m_nview;
  protected readonly string Key;
  protected readonly string StoreRPC;
  public abstract T Value { get; set; }

  public MapTableZDOValue(MapTable mapTable, string key)
  {
    this.MapTable = mapTable;
    this.Key = $"{Plugin.ModGUID}.MapTable.{key}";
    this.StoreRPC = $"{Plugin.ModGUID}.RPC_Store.MapTable.{key}";
    this.RegisterStoreRPC();
  }

  protected abstract void RegisterStoreRPC();
}
