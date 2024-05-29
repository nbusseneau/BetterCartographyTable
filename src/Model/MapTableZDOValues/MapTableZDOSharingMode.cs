namespace BetterCartographyTable.Model.MapTableZDOValues;

public class MapTableZDOSharingMode(MapTable mapTable) : MapTableZDOValue<SharingMode>(mapTable, "Mode")
{
  public override SharingMode Value
  {
    get => (SharingMode)this.NView.GetZDO().GetInt(this.Key, (int)SharingMode.Public);
    set
    {
      if (this.NView.IsOwner()) this.NView.GetZDO().Set(this.Key, (int)value);
      else this.NView.InvokeRPC(this.StoreRPC, (int)value);
    }
  }

  protected override void RegisterStoreRPC() => this.NView.Register<int>(this.StoreRPC, (_, mode) => this.RPC_Store(mode));
  private void RPC_Store(int mode)
  {
    if (!this.NView.IsOwner()) return;
    this.NView.GetZDO().Set(this.Key, mode);
  }
}
