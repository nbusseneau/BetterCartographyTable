using BetterCartographyTable.Model.Managers;
using HarmonyLib;
using UnityEngine;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(ZNetScene))]
public static class ZNetScenePatches
{
  [HarmonyPrefix]
  [HarmonyPatch(nameof(ZNetScene.Destroy))]
  private static void TryRemoveMapTableManagersOnDestroy(GameObject go)
  {
    if (go.GetComponent<ZNetView>() is not { } nview || nview.GetZDO() == null) return;
    MapTableManager.Remove(nview);
  }

  [HarmonyPrefix]
  [HarmonyPatch(nameof(ZNetScene.OnZDODestroyed))]
  private static void TryRemoveMapTableManagersOnZDODestroyed(ZDO zdo)
  {
    if (!ZNetScene.instance.m_instances.TryGetValue(zdo, out var nview)) return;
    MapTableManager.Remove(nview);
  }
}
