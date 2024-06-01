namespace BetterCartographyTable.Model.MapTableZDOValues;

public class MapTableZDOOwner(MapTable mapTable) : MapTableZDOValue<string>(mapTable, "Owner")
{
  public override string Value
  {
    get => this.NView.GetZDO().GetString(this._key, string.Empty);
    set
    {
      if (this.NView.IsOwner()) this.NView.GetZDO().Set(this._key, value);
      else this.NView.InvokeRPC(this._storeRPC, value);
    }
  }

  protected override void RegisterStoreRPC() => this.NView.Register<string>(this._storeRPC, (_, owner) => this.RPC_Store(owner));
  private void RPC_Store(string owner)
  {
    if (!this.NView.IsOwner()) return;
    this.NView.GetZDO().Set(this._key, owner);
  }
}
