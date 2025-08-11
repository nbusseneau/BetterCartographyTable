using UnityEngine;

namespace BetterCartographyTable.Extensions;

public static class Vector3Extensions
{
  public static string ToBase64(this Vector3? position)
  {
    if (position is null) return string.Empty;
    ZPackage zPackage = new();
    zPackage.Write(position.Value);
    return zPackage.Compress().GetBase64();
  }
}
