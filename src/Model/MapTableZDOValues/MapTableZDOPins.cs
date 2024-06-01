using System.Collections.Generic;
using BetterCartographyTable.Extensions;

namespace BetterCartographyTable.Model.MapTableZDOValues;

public class MapTableZDOPins(MapTable mapTable) : MapTableZDOValue<IEnumerable<SharablePinData>>(mapTable, "Pins")
{
  public override IEnumerable<SharablePinData> Value
  {
    get
    {
      if (this.NView.GetZDO().GetByteArray(this._key) is { } array)
      {
        array = Utils.Decompress(array);
        ZPackage zPackage = new(array);
        return zPackage.ReadSharablePinDataList();
      }
      return [];
    }
    set
    {
      var compressedZPackage = value.ToCompressedZPackage();
      if (this.NView.IsOwner()) this.NView.GetZDO().Set(this._key, compressedZPackage.GetArray());
      else this.NView.InvokeRPC(this._storeRPC, compressedZPackage);
    }
  }

  protected override void RegisterStoreRPC() => this.NView.Register<ZPackage>(this._storeRPC, (_, compressedZPackage) => this.RPC_Store(compressedZPackage));
  private void RPC_Store(ZPackage compressedZPackage)
  {
    if (!this.NView.IsOwner()) return;
    this.NView.GetZDO().Set(this._key, compressedZPackage.GetArray());
  }
}
