namespace BetterCartographyTable.Model.MapTableZDOValues;

public class MapTableZDOSharingMode(MapTable mapTable) : MapTableZDOValue<SharingMode>(mapTable, "Mode")
{
  public override SharingMode Value
  {
    get => (SharingMode)this.NView.GetZDO().GetInt(this._key, (int)SharingMode.Public);
    set
    {
      if (this.NView.IsOwner()) this.NView.GetZDO().Set(this._key, (int)value);
      else this.NView.InvokeRPC(this._storeRPC, (int)value);
    }
  }

  protected override void RegisterStoreRPC() => this.NView.Register<int>(this._storeRPC, (_, mode) => this.RPC_Store(mode));
  private void RPC_Store(int mode)
  {
    if (!this.NView.IsOwner()) return;
    this.NView.GetZDO().Set(this._key, mode);
  }
}
