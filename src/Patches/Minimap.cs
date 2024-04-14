using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BetterCartographyTable.Managers;
using BetterCartographyTable.Model;
using BetterCartographyTable.UI;
using HarmonyLib;
using static Minimap;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Minimap))]
public static class MinimapPatches
{
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.Awake))]
  private static void InjectSharablePinsAndCreatePinToggles()
  {
    MinimapManager.OnAwake();
    MinimapManagerUI.OnAwake();
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.Start))]
  private static void ResetTogglesVisiblityOnStart() => MinimapManagerUI.OnStart();

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
  private class HandleInteractionsOnMapModeChange
  {
    private static void Prefix(MapMode mode)
    {
      var isMinimapOpening = mode == MapMode.Large;

      // override noMap mode when interacting with a table
      if (isMinimapOpening && InteractionManager.IsInteracting && Game.m_noMap && GamePatches.IsNoMapModeEnabled)
      {
        Game.m_noMap = false;
      }

      // trigger on close callback when not opening map
      if (!isMinimapOpening) InteractionManager.OnMapClose();
    }

    private static void Postfix()
    {
      // restore noMap mode
      if (GamePatches.IsNoMapModeEnabled) Game.m_noMap = true;
    }
  }

  /// <summary>
  /// Ensure sure calls to <c>Minimap.AddPin(...)</c> always add <c>SharablePinData</c> pins to the list and use our own
  /// <c>Add(...)</c> implementation.
  /// </summary>
  [HarmonyTranspiler]
  [HarmonyPatch(nameof(Minimap.AddPin))]
  private static IEnumerable<CodeInstruction> InjectSharablePinsAndSharablePinDataInAddPin(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
        .MatchForward(false, new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(PinData))))
        .ThrowIfInvalid("Could not patch PinData constructor in Minimap.AddPin(...)")
        .SetOperandAndAdvance(AccessTools.Constructor(typeof(SharablePinData)))
        .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(List<PinData>), nameof(Minimap.m_pins.Add))))
        .ThrowIfInvalid("Could not patch List<PinData>.Add(...) method in Minimap.AddPin(...)")
        .SetOperandAndAdvance(AccessTools.Method(typeof(SharablePins), nameof(SharablePins.Add)))
        .InstructionEnumeration();
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.UpdatePins))]
  private static void UpdateUIOnUpdatePins() => MinimapManagerUI.OnUpdatePins();

  #region Click intercepts

  [HarmonyPrefix]
  [HarmonyPatch(nameof(Minimap.OnMapDblClick))]
  private static bool InterceptOnMapDblClick()
  {
    var shouldRunOriginalMethod = InteractionManager.OnMapDblClick();
    return shouldRunOriginalMethod;
  }

  [HarmonyPrefix]
  [HarmonyPatch(nameof(Minimap.OnMapLeftClick))]
  private static bool InterceptOnMapLeftClick()
  {
    var shouldRunOriginalMethod = false;
    InteractionManager.OnMapLeftClick();
    return shouldRunOriginalMethod;
  }

  [HarmonyPrefix]
  [HarmonyPatch(nameof(Minimap.OnMapRightClick))]
  private static bool InterceptOnMapRightClick()
  {
    var shouldRunOriginalMethod = false;
    InteractionManager.OnMapRightClick();
    return shouldRunOriginalMethod;
  }

  #endregion

  #region Map data intercepts

  /// <summary>
  /// Inject private pins as <c>Minimap.instance.m_pins</c> replacement in <c>Minimap.GetMapData()</c> so that world
  /// data stores non-shared pins in a vanilla-compatible way.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetMapData))]
  private class InjectPrivatePinsInGetMapData
  {
    private static List<PinData> privatePins;
    private static void Prefix() => privatePins = MinimapManager.PrivatePins.ToList<PinData>();

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetMapData()")
          .Repeat(m => m.SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPrivatePinsInGetMapData), nameof(privatePins))))
          .InstructionEnumeration();
    }
  }

  /// <summary>
  /// Inject public pins as <c>Minimap.instance.m_pins</c> replacement in <c>Minimap.GetSharedMapData()</c> so that
  /// non-modded clients can retrieve public pins stored in vanilla shared data.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSharedMapData))]
  private class InjectPublicPinsInGetSharedMapData
  {
    private static List<PinData> publicPins;
    private static void Prefix() => publicPins = MinimapManager.PublicPins.ToList<PinData>();

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetSharedMapData()")
          .Repeat(m => m.SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPublicPinsInGetSharedMapData), nameof(publicPins))))
          .InstructionEnumeration();
    }
  }

  /// <summary>
  /// Truncate <c>Minimap.AddSharedMapData()</c> after retrieving explored map data from vanilla shared data, ignoring
  /// pins coming from non-modded clients.
  /// </summary>
  [HarmonyTranspiler]
  [HarmonyPatch(nameof(Minimap.AddSharedMapData))]
  private static IEnumerable<CodeInstruction> TruncateAddSharedMapData(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
        .MatchForward(false,
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Stloc_S),
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Ldc_I4_2),
            new CodeMatch(OpCodes.Blt))
        .ThrowIfInvalid("Could not truncate Minimap.AddSharedMapData()")
        .SetAndAdvance(OpCodes.Ldloc_3, null)
        .SetAndAdvance(OpCodes.Ret, null)
        .InstructionEnumeration();
  }

  #endregion
}
