namespace BetterCartographyTable.Model.MapTableZDOValues;

public class MapTableZDOOwner(MapTable mapTable) : MapTableZDOValue<string>(mapTable, "Owner")
{
  public override string Value
  {
    get => this.NView.GetZDO().GetString(this.Key, string.Empty);
    set
    {
      if (this.NView.IsOwner()) this.NView.GetZDO().Set(this.Key, value);
      else this.NView.InvokeRPC(this.StoreRPC, value);
    }
  }

  protected override void RegisterStoreRPC() => this.NView.Register<string>(this.StoreRPC, (_, owner) => this.RPC_Store(owner));
  private void RPC_Store(string owner)
  {
    if (!this.NView.IsOwner()) return;
    this.NView.GetZDO().Set(this.Key, owner);
  }
}
